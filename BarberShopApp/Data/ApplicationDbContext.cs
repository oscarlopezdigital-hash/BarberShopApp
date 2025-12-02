// Data/ApplicationDbContext.cs
using BarberShopApp.Data.Configurations;
using BarberShopApp.Models;
using BarberShopApp.Services; // IMPORTACIÓN NECESARIA para ITenantService
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BarberShopApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        private readonly ITenantService _tenantService; // 1. Campo para inyectar el servicio de Tenant

        // 2. MODIFICACIÓN DEL CONSTRUCTOR: Ahora inyecta ITenantService
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantService tenantService)
            : base(options)
        {
            _tenantService = tenantService; // Asignamos el servicio al campo privado
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Barber> Barbers { get; set; }
        public DbSet<Service> Services { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Siempre llama al base para Identity

            builder.ApplyConfiguration(new TenantConfiguration());

            // --- CONFIGURACIÓN DE RELACIONES (Para evitar error de Cascada) ---

            // Relación Appointment -> Tenant
            builder.Entity<Appointment>()
                .HasOne(a => a.Tenant)
                .WithMany()
                .HasForeignKey(a => a.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Barber -> Tenant
            builder.Entity<Barber>()
                .HasOne(b => b.Tenant)
                .WithMany()
                .HasForeignKey(b => b.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Service -> Tenant
            builder.Entity<Service>()
                .HasOne(s => s.Tenant)
                .WithMany()
                .HasForeignKey(s => s.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración para el campo decimal 'Price' (soluciona la advertencia)
            builder.Entity<Service>()
                   .Property(s => s.Price)
                   .HasColumnType("decimal(18, 2)");

            // --- 3. APLICACIÓN DE FILTROS GLOBALES (SEGURIDAD MULTI-TENANCY) ---

            // Filtra citas: solo muestra datos del Tenant activo
            builder.Entity<Appointment>()
                .HasQueryFilter(a => a.TenantId == _tenantService.GetCurrentTenantId());

            // Filtra barberos: solo muestra datos del Tenant activo
            builder.Entity<Barber>()
                .HasQueryFilter(b => b.TenantId == _tenantService.GetCurrentTenantId());

            // Filtra servicios: solo muestra datos del Tenant activo
            builder.Entity<Service>()
                .HasQueryFilter(s => s.TenantId == _tenantService.GetCurrentTenantId());

            // --- FIN DE FILTROS GLOBALES ---
        }
    }
}