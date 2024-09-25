using System.Text.Json;
using System.Web;

namespace VegaExpress.Gateway.Middlewares
{
    public class RedirectMiddleware    
    {
        private readonly RequestDelegate next;
        private readonly HttpClient httpClient;

        public RedirectMiddleware(RequestDelegate next, HttpClient httpClient)
        {
            this.next = next;
            this.httpClient = httpClient;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path == "/home" || context.Request.Path == "/favicon.ico")
            {
                await next(context);
                return;
            }

            // Obtener el método HTTP y la ruta de destino
            var method = context.Request.Method;
            var destinationPath = $"{context.Request.Path}{context.Request.QueryString}"; //context.Request.Headers["X-Destination-Path"];
            var newPath = $"https://127.0.0.1:8083{destinationPath}"; // Include query string

            // Check if user is authenticated before redirection
            if (!context.User.Identity!.IsAuthenticated)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            // Verificar si hay una ruta de destino y si el método es compatible
            if (!string.IsNullOrEmpty(destinationPath) && (method == "GET" || method == "POST" || method == "PUT" || method == "DELETE" || method == "PATCH"))
            {
                // Redirigir la solicitud a la ruta de destino
                
                context.Request.Path = new PathString(destinationPath);

                // Reenviar los parámetros y el body según el método y el Content-Type
                if (method == "GET")
                {
                    await ForwardGETParameters(context);
                }
                else if (method == "POST" || method == "PUT" || method == "PATCH")
                {
                    var contentType = context.Request.ContentType;

                    if (contentType!.StartsWith("application/x-www-form-urlencoded"))
                    {
                        await ForwardFormUrlencodedParameters(context);
                    }
                    else if (contentType.StartsWith("multipart/form-data"))
                    {
                        await ForwardFormDataParameters(context);
                    }
                    else if (contentType == "application/json")
                    {
                        await ForwardJSONBody(context);
                    }
                    else
                    {
                        await ForwardRawBody(context);
                    }
                }
                else if (method == "DELETE")
                {
                    await ForwardGETParameters(context);
                }

                // Invocar el siguiente middleware en la cadena
                await next(context);
            }
            else
            {
                // Continuar con la cadena de middleware si no hay redirección
                await next(context);
            }
        }

        private async Task ForwardGETParameters(HttpContext context)
        {
            // Read query string parameters
            var queryString = context.Request.Query;

            // Create new query string with redirected parameters
            var newQueryString = new QueryString();
            foreach (var key in queryString.Keys)
            {
                newQueryString.Add(key, queryString[key]!);
            }

            // Update request query string
            context.Request.QueryString = newQueryString;

            await Task.CompletedTask;
        }

        private async Task ForwardFormUrlencodedParameters(HttpContext context)
        {
            // Read request body as string
            var bodyString = await ReadBodyAsString(context.Request);

            // Decode URLencoded string
            var formValues = HttpUtility.ParseQueryString(bodyString);

            // Create new query string with redirected parameters
            var newQueryString = new QueryString();
            foreach (var key in formValues.AllKeys)
            {
                newQueryString.Add(key!, formValues[key]!);
            }

            // Update request query string
            context.Request.QueryString = newQueryString;
        }

        private async Task ForwardFormDataParameters(HttpContext context)
        {
            // Read request body as form data
            var formData = await ReadFormAsync(context.Request);

            // Create new query string with redirected parameters
            var newQueryString = new QueryString();
            foreach (var key in formData.Keys)
            {
                newQueryString.Add(key, formData[key]!);
            }

            // Update request query string
            context.Request.QueryString = newQueryString;
        }

        private async Task ForwardJSONBody(HttpContext context)
        {
            // Read request body as JSON
            var json = await ReadBodyAsString(context.Request);

            // Deserialize JSON to dynamic object
            dynamic data = JsonSerializer.Deserialize<dynamic>(json)!;

            // Create new query string with redirected parameters
            var newQueryString = new QueryString();
            foreach (var property in data)
            {
                newQueryString.Add(property.Name, data[property].ToString());
            }

            // Update request query string
            context.Request.QueryString = newQueryString;
        }

        private async Task ForwardRawBody(HttpContext context)
        {
            // Read request body as raw bytes
            var rawBody = await ReadBodyAsByteArray(context.Request);

            // Create a new Stream object from the raw bytes
            var newBodyStream = new MemoryStream(rawBody);

            // Set the request body to the new stream
            context.Request.Body = newBodyStream;

            // Reset the stream position to allow reading from the beginning
            newBodyStream.Position = 0;
        }

        private async Task<string> ReadBodyAsString(HttpRequest request)
        {
            using (var reader = new StreamReader(request.Body))
            {
                var body = await reader.ReadToEndAsync();
                return body;
            }
        }

        private async Task<IFormCollection> ReadFormAsync(HttpRequest request)
        {
            if (!request.HasFormContentType)
            {
                throw new InvalidOperationException("Request doesn't have form content type.");
            }
            return await request.ReadFormAsync();
        }

        private async Task<byte[]> ReadBodyAsByteArray(HttpRequest request)
        {
            request.Body.Position = 0; // Ensure we read from the beginning
            using (var memoryStream = new MemoryStream())
            {
                await request.Body.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
