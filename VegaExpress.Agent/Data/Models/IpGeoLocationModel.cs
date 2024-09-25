namespace VegaExpress.Agent.Data.Models
{
    public class IpGeoLocationModel
    {
        public string? Ip { get; set; }
        public string? City { get; set; }
        public string? Region { get; set; }
        public string? CountryName { get; set; }

        public new string ToString() => $"{City} {Ip}. {CountryName}";
    }
}
