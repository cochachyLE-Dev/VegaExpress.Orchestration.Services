using System.Net.Http;

namespace VegaExpress.Gateway.Middlewares
{
    public class ProxyMiddleware
    {
        private readonly RequestDelegate next;
        private readonly HttpClient httpClient;

        public ProxyMiddleware(RequestDelegate next, HttpClient httpClient)
        {
            this.next = next;
            this.httpClient = httpClient;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;
            var newHost = "127.0.0.1:8083";
            var newPath = $"https://{newHost}{request.Path}{request.QueryString}"; // Include query string

            var newRequest = new HttpRequestMessage();
            var requestMethod = request.Method;

            if (!HttpMethods.IsGet(requestMethod) &&
                !HttpMethods.IsHead(requestMethod) &&
                !HttpMethods.IsDelete(requestMethod) &&
                !HttpMethods.IsTrace(requestMethod))
            {
                var streamContent = new StreamContent(request.Body);
                newRequest.Content = streamContent;
            }

            foreach (var header in request.Headers)
            {
                newRequest.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }

            newRequest.Headers.Host = newHost;
            newRequest.RequestUri = new Uri(newPath);
            newRequest.Method = new HttpMethod(request.Method);

            using var responseMessage = await httpClient.SendAsync(newRequest, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);

            context.Response.StatusCode = (int)responseMessage.StatusCode;
            foreach (var header in responseMessage.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in responseMessage.Content.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            await responseMessage.Content.CopyToAsync(context.Response.Body);
        }
    }
}
