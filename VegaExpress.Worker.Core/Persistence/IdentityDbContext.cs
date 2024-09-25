using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using VegaExpress.Worker.Core.Persistence.Contracts;
using VegaExpress.Worker.Core.Entities;
using VegaExpress.Worker.Core.Persistence.Seeds;
using VegaExpress.Worker.Core.Persistence.Entities.Auth;

namespace VegaExpress.Worker.Core.Persistence
{
    public class IdentityDbContext : IdentityDbContext<ApplicationUser>, IIdentityDbContext
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
        {
        }
        public DbSet<Acceso>? Accesos { get; set; }
        public DbSet<GrupoAcceso>? GrupoAccesos { get; set; }
        public DbSet<GrupoAccesoAcceso>? GrupoAccesoAccesos { get; set; }
        public DbSet<GrupoAccesoRol>? GrupoAccesosRoles { get; set; }
        public DbSet<Log>? Logs { get; set; }

        public async Task<int> SaveChangesAsync() => await base.SaveChangesAsync();
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.HasDefaultSchema("Identity");

            modelBuilder.Entity<AccesoAccionTipo>().HasKey(o => o.Id);
            modelBuilder.Entity<Acceso>().HasKey(o => o.Id);
            modelBuilder.Entity<Acceso>().HasOne<AccesoAccionTipo>(o => o.AccesoAccionTipo);

            modelBuilder.Entity<GrupoAcceso>().HasKey(o => o.Id);
            modelBuilder.Entity<GrupoAcceso>().HasMany<GrupoAccesoAcceso>(o => o.GrupoAccesoAccesos);

            modelBuilder.Entity<GrupoAccesoAcceso>().HasKey(o => new { o.GrupoAccesoId, o.AccesoId });
            modelBuilder.Entity<GrupoAccesoAcceso>().HasKey(o => new { o.GrupoAccesoId, o.AccesoId });

            modelBuilder.Entity<GrupoAccesoRol>().HasKey(o => new { o.RolId, o.GrupoAccesoId });
            modelBuilder.Entity<Log>().HasKey(o => o.Id);

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable(name: "Usuario");
                entity.Property(c => c.Id).HasColumnName("Id");
                entity.Property(c => c.AccessFailedCount).HasColumnName("ErroresAccesoCant");
                entity.Property(c => c.ConcurrencyStamp).HasColumnName("SelloSimultaneidad");
                entity.Property(c => c.Email).HasColumnName("CorreoElectronico");
                entity.Property(c => c.EmailConfirmed).HasColumnName("CorreoElectronicoConfirmado");
                entity.Property(c => c.FirstName).HasColumnName("Nombres");
                entity.Property(c => c.LastName).HasColumnName("Apellidos");
                entity.Property(c => c.LockoutEnabled).HasColumnName("BloqueoHabilitado");
                entity.Property(c => c.LockoutEnd).HasColumnName("BloqueoFin");
                entity.Property(c => c.NormalizedEmail).HasColumnName("CorreoElectronicoNormalizado");
                entity.Property(c => c.NormalizedUserName).HasColumnName("NombreUsuarioNormalizado");
                entity.Property(c => c.PasswordHash).HasColumnName("ContraseniaHash");
                entity.Property(c => c.PhoneNumber).HasColumnName("NumeroTelefono");
                entity.Property(c => c.PhoneNumberConfirmed).HasColumnName("NumeroTelefonoConfirmado");
                entity.Property(c => c.SecurityStamp).HasColumnName("SelloSeguridad");
                entity.Property(c => c.TwoFactorEnabled).HasColumnName("TwoFactorHabilitado");
                entity.Property(c => c.UserName).HasColumnName("NombreUsuario");
            });
            modelBuilder.Entity<RefreshToken>().HasKey(p => p.Id);
            modelBuilder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable(name: "Rol");
                entity.Property(c => c.Id).HasColumnName("Id");
                entity.Property(c => c.Name).HasColumnName("Nombre");
                entity.Property(c => c.NormalizedName).HasColumnName("NombreNormalizado");
                entity.Property(c => c.ConcurrencyStamp).HasColumnName("SelloSimultaneidad");
            });
            modelBuilder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.ToTable("UsuarioRoles");
                entity.Property(c => c.RoleId).HasColumnName("RolId");
                entity.Property(c => c.UserId).HasColumnName("UsuarioId");
            });
            modelBuilder.Entity<IdentityUserClaim<string>>(entity =>
            {
                entity.ToTable("UsuarioClaims");
                entity.Property(c => c.Id).HasColumnName("Id");
                entity.Property(c => c.UserId).HasColumnName("UserId");
                entity.Property(c => c.ClaimType).HasColumnName("ClaimType");
                entity.Property(c => c.ClaimValue).HasColumnName("ClaimValue");
            });
            modelBuilder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.ToTable("UsuarioLogins");
                entity.Property(c => c.LoginProvider).HasColumnName("LoginProveedor");
                entity.Property(c => c.ProviderKey).HasColumnName("ProveedorKey");
                entity.Property(c => c.ProviderDisplayName).HasColumnName("ProveedorNombre");
                entity.Property(c => c.UserId).HasColumnName("UserId");
            });
            modelBuilder.Entity<IdentityRoleClaim<string>>(entity =>
            {
                entity.ToTable("RolClaims");
                entity.Property(c => c.Id).HasColumnName("Id");
                entity.Property(c => c.RoleId).HasColumnName("RoleId");
                entity.Property(c => c.ClaimType).HasColumnName("ClaimType");
                entity.Property(c => c.ClaimValue).HasColumnName("ClaimValue");
            });
            modelBuilder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.ToTable("UsuarioTokens");
                entity.Property(c => c.UserId).HasColumnName("UserId");
                entity.Property(c => c.LoginProvider).HasColumnName("LoginProveedor");
                entity.Property(c => c.Name).HasColumnName("Nombre");
                entity.Property(c => c.Value).HasColumnName("Valor");
            });

            modelBuilder.Seed();
        }
    }
}
