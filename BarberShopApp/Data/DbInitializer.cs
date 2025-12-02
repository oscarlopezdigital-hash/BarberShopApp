// Data/DbInitializer.cs
using BarberShopApp.Models;
using Microsoft.AspNetCore.Identity; // Necesario para UserManager
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks; // Necesario para el código asíncrono

namespace BarberShopApp.Data
{
    public static class DbInitializer
    {
        // 1. Modificación: Ahora acepta UserManager como parámetro
        public static async Task Initialize(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            await context.Database.MigrateAsync(); // Asegura que las migraciones se apliquen

            // --- 2. CREACIÓN DEL TENANT POR DEFECTO ---

            // Crear el primer Tenant si no existe
            if (!context.Tenants.Any())
            {
                var tenant1 = new Tenant
                {
                    Name = "Barbería Principal de Prueba",
                    AdminEmail = "admin@tubarberia.com"
                };
                context.Tenants.Add(tenant1);
                await context.SaveChangesAsync();
            }

            // Obtener el ID del Tenant de prueba
            var defaultTenantId = context.Tenants.First().TenantId;

            // --- 3. CREACIÓN DEL USUARIO ADMINISTRADOR DE IDENTIDAD ---

            string adminEmail = "admin@tubarberia.com"; // Email que usaremos para el login
            string adminPassword = "Password123!"; // ¡Contraseña de prueba! **Cámbiala en producción.**

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true // CLAVE: Confirma el email automáticamente para evitar el error de registro
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    // Puedes añadir lógica aquí para asignar roles o claims si fuera necesario
                }
            }

            // --- 4. ASIGNACIÓN DE DATOS EXISTENTES AL TENANT ID ---
            // Asigna los datos que se crearon antes de la implementación de Multi-Tenancy al Tenant ID 1.

            // a. Asignar TenantId a los Barbers existentes (donde TenantId aún es 0)
            var existingBarbers = await context.Barbers.Where(b => b.TenantId == 0).ToListAsync();
            foreach (var barber in existingBarbers)
            {
                barber.TenantId = defaultTenantId;
            }

            // b. Asignar TenantId a los Services existentes
            var existingServices = await context.Services.Where(s => s.TenantId == 0).ToListAsync();
            foreach (var service in existingServices)
            {
                service.TenantId = defaultTenantId;
            }

            // c. Asignar TenantId a las Appointments existentes
            var existingAppointments = await context.Appointments.Where(a => a.TenantId == 0).ToListAsync();
            foreach (var appointment in existingAppointments)
            {
                appointment.TenantId = defaultTenantId;
            }

            await context.SaveChangesAsync();
        }
    }
}