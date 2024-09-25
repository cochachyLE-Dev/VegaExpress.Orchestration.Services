namespace VegaExpress.Worker.Core.Persistence.Entities.Auth
{
    public class GrupoAccesoAcceso
    {
        public int GrupoAccesoId { get; set; }
        public int AccesoId { get; set; }
        public Acceso? Acceso { get; set; }
    }
}
