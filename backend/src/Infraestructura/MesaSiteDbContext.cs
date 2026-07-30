using Microsoft.EntityFrameworkCore;
using MesaSitec.Dominio;

namespace MesaSitec.Infraestructura;

public class MesaSitecDbContext(DbContextOptions<MesaSitecDbContext> options) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Solicitud> Solicitudes => Set<Solicitud>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();

        b.Entity<Usuario>().Property(u => u.Rol).HasConversion<string>();
        b.Entity<Solicitud>().Property(s => s.Prioridad).HasConversion<string>();
        b.Entity<Solicitud>().Property(s => s.Estado).HasConversion<string>();

        b.Entity<Solicitud>().HasIndex(s => new { s.TenantId, s.Codigo }).IsUnique();

        b.Entity<Solicitud>()
            .HasOne(s => s.Agente)
            .WithMany()
            .HasForeignKey(s => s.AgenteId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Solicitud>()
            .HasOne(s => s.Solicitante)
            .WithMany()
            .HasForeignKey(s => s.SolicitanteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}