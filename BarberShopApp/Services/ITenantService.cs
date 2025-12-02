// Services/ITenantService.cs

namespace BarberShopApp.Services
{
    public interface ITenantService
    {
        // Retorna el ID del Tenant activo.
        int GetCurrentTenantId();
    }
}