namespace VegaExpress.Worker.Core.Entities
{
    public class RefreshToken
    {
        public int? Id { get; set; }
        public string? Token { get; set; }
        public DateTime? Expira { get; set; }
        public bool HaExpirado => DateTime.Now >= Expira;
        public DateTime? CreadoEn { get; set; }
        public string? CreadoPorIp { get; set; }
        public DateTime? Revocado { get; set; }
        public string? RevocadoPorIp { get; set; }
        public string? ReemplazadoPorToken { get; set; }
        public bool Activo => Revocado == null && !HaExpirado;
    }
}