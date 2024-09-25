using System.Text.Json;
using System.Text.Json.Serialization;
namespace VegaExpress.Worker.Utilities
{
    public class NetworkMonitor
    {
        private readonly Timer timer;
        public event Action<IpGeoLocation>? InternetAvaliableChanged;
        public bool? IsInternetAvaliable { get; private set; }
        public IpGeoLocation? IpGeoLocation { get; private set; } = null;
        public NetworkMonitor()
        {
            timer = new Timer(async (s) => await CheckInternetConnection(), null, TimeSpan.FromSeconds(3), Timeout.InfiniteTimeSpan);
        }

        public async Task CheckInternetConnection()
        {
            var internetChecker = new InternetAvailabilityChecker();
            bool resultChecker = await internetChecker.IsInternetAvailable();
            if (resultChecker != IsInternetAvaliable)
            {
                IsInternetAvaliable = resultChecker;                
                await GetIpGeoLocationAsync("98cfadbfd6794bf883f896a5e36425e0");                

                if (InternetAvaliableChanged != null)
                    InternetAvaliableChanged(IpGeoLocation!);
            }

            timer.Change(TimeSpan.FromSeconds(3), Timeout.InfiniteTimeSpan);
        }
             
        public async Task GetIpGeoLocationAsync(string apiKey)
        {
            if (IsInternetAvaliable == null || !IsInternetAvaliable!.Value) return;
            using HttpClient client = new HttpClient();
            string url = $"https://api.ipgeolocation.io/ipgeo?apiKey={apiKey}";
            string response = await client.GetStringAsync(url);
            IpGeoLocation = JsonSerializer.Deserialize<IpGeoLocation>(response)!;
        }        
        public class InternetAvailabilityChecker
        {
            public async Task<bool> IsInternetAvailable()
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        var response = await httpClient.GetAsync("https://www.google.com");
                        return response.IsSuccessStatusCode;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }
    }
    public class IpGeoLocation
    {
        [JsonPropertyName("ip")]
        public string? Ip { get; set; }
        [JsonPropertyName("city")]
        public string? City { get; set; }
        [JsonPropertyName("region")]
        public string? Region { get; set; }
        [JsonPropertyName("country_code2")]
        public string? CountryName { get; set; }        
    }
}
