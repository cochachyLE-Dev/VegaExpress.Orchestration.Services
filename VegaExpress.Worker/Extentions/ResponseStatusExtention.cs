using VegaExpress.Discovery;

namespace VegaExpress.Worker.Extentions
{
    public static class ResponseStatusExtention
    {
        public static string GetSuccessMessage() => "Successfully processed";
        public static string GetSuccessMessage(int numberOfRequests, TimeSpan responseTime, string serviceLocation)
        {
            return $"Successfully processed {numberOfRequests} requests in {responseTime.TotalSeconds} seconds at {serviceLocation}.";
        }
        public static string GetMessage(this ResponseStatus status)
        {
            switch (status)
            {                
                case ResponseStatus.Timeout:
                    return "The request timed out.";
                case ResponseStatus.ConnectionLost:
                    return "The connection was lost.";
                case ResponseStatus.Unauthorized:
                    return "The request was not authorized.";
                case ResponseStatus.NotFound:
                    return "The requested resource was not found.";
                case ResponseStatus.InternalServerError:
                    return "The server encountered an error.";
                default:
                    return "An unknown error occurred.";
            }
        }

        public static string[] GetPossibleCauses(this ResponseStatus error)
        {
            switch (error)
            {
                case ResponseStatus.Timeout:
                    return new[] { "The server is not responding.", "The network is slow." };
                case ResponseStatus.ConnectionLost:
                    return new[] { "The network connection was lost.", "The server went down." };
                case ResponseStatus.Unauthorized:
                    return new[] { "The credentials are incorrect.", "The token has expired." };
                case ResponseStatus.NotFound:
                    return new[] { "The resource does not exist.", "The URL is incorrect." };
                case ResponseStatus.InternalServerError:
                    return new[] { "There is a bug in the server code.", "The server is overloaded." };
                default:
                    return new[] { "An unknown error occurred." };
            }
        }
    }
}
