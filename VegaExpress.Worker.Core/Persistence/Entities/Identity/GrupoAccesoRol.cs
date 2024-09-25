namespace VegaExpress.Worker.Core.Persistence.Entities.Auth
{
    public class GrupoAccesoRol
    {
        public int GrupoAccesoId { get; set; }
        public string? RolId { get; set; }
        public List<GrupoAcceso>? GrupoAccesos { get; set; }
    }
}
