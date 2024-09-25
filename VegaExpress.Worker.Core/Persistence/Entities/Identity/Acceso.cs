namespace VegaExpress.Worker.Core.Persistence.Entities.Auth
{    
    public class Acceso
    {
        public int Id { get; set; }
        public int? PadreAccesoId { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public int Posicion { get; set; }
        public int AccesoAccionTipoID { get; set; }
        public bool Habilitado { get; set; }
        public bool Visible { get; set; }
        public int? Icono { get; set; }
        public DateTime CreadoEn { get; set; }
        public string? CreadoPor { get; set; }

        public AccesoAccionTipo? AccesoAccionTipo { get; set; }
    }
}
