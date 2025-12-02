// Pages/Services/Index.cshtml.cs
using BarberShopApp.Data;
using BarberShopApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShopApp.Pages.Services
{
    // Restringimos esta página a usuarios autenticados
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Propiedad que contendrá la lista de servicios del Tenant actual
        public IList<Service> Service { get; set; } = new List<Service>();

        public async Task OnGetAsync()
        {
            // NOTA: Gracias al HasQueryFilter en ApplicationDbContext,
            // esta consulta automáticamente agrega WHERE TenantId = CurrentTenantId
            Service = await _context.Services.ToListAsync();
        }
    }
}