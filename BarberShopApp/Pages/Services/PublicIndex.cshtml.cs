using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BarberShopApp.Data;
using BarberShopApp.Models;
using BarberShopApp.Services;

namespace BarberShopApp.Pages.Services
{
    // NO usamos [Authorize] para que sea accesible públicamente
    public class PublicIndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantService _tenantService;

        public PublicIndexModel(ApplicationDbContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        // Propiedad que contendrá la lista de servicios a mostrar
        public IList<Service> Service { get; set; } = new List<Service>();

        public async Task OnGetAsync()
        {
            // Cargamos todos los Servicios del Tenant actual.
            // La magia del Multi-Tenancy (filtrado por TenantId) se aplica automáticamente
            // gracias al HasQueryFilter que definiste.
            Service = await _context.Services
                .OrderBy(s => s.Name)
                .ToListAsync();
        }
    }
}