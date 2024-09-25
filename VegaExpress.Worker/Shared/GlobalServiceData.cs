using System.Collections.Concurrent;

namespace VegaExpress.Worker.Shared
{
    public static class GlobalServiceData
    {
        public static string? Address { get; set; }
        public static int NumberOfAllowedRequests { get; set; }
        public static int Traffic { get; set; } // cantidad de solicitudes que el servicio maneja
        public static double ErrorRate { get; set; }
        public static bool Availability { get; set; }
        public static string? Location { get; set; }

        public static double AverageResponseTime = ResponseTimes!.Values.Average(time => time.TotalMilliseconds);
        
        public static ConcurrentDictionary<string, TimeSpan> ResponseTimes = new ConcurrentDictionary<string, TimeSpan>();
        public static EventHandler<string>? EventHandler { get; set; }

    }
}
