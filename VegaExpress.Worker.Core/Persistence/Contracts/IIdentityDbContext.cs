using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using VegaExpress.Worker.Core.Persistence.Entities.Auth;

namespace VegaExpress.Worker.Core.Persistence.Contracts
{
    internal interface IIdentityDbContext
    {
        DbSet<Acceso>? Accesos { get; set; }
        DbSet<GrupoAcceso>? GrupoAccesos { get; set; }
        DbSet<GrupoAccesoAcceso>? GrupoAccesoAccesos { get; set; }
        DbSet<GrupoAccesoRol>? GrupoAccesosRoles { get; set; }
        DbSet<IdentityRole>? Roles { get; set; }
        Task<int> SaveChangesAsync();
    }
}
