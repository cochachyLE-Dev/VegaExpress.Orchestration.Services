namespace VegaExpress.Worker.Core.Persistence.Entities.Auth
{
    public class GrupoAcceso
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public bool Habilitado { get; set; }
        public DateTime CreadoEn { get; set; }
        public string? CreadoPor { get; set; }

        public List<GrupoAccesoAcceso>? GrupoAccesoAccesos { get; set; }
    }
}
