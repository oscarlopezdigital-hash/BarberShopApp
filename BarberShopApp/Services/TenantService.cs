// Services/TenantService.cs

using BarberShopApp.Services;

namespace BarberShopApp.Services
{
    public class TenantService : ITenantService
    {
        // Nota: En una aplicación real, este valor se obtendría del usuario logueado.
        // Lo dejamos hardcodeado temporalmente para la prueba de Multi-Tenancy.
        public int GetCurrentTenantId()
        {
            // !!! CAMBIA ESTE VALOR AL ID DE UN TENANT QUE EXISTA EN TU BASE DE DATOS !!!
            return 1; // Ejemplo: Usamos 2 si fue el primer Tenant creado en el CRUD.
        }
    }
}