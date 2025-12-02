using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BarberShopApp.Data;
using BarberShopApp.Models;
using BarberShopApp.Services;

namespace BarberShopApp.Pages.Barbers
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantService _tenantService;

        public DetailsModel(ApplicationDbContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        public Barber Barber { get; set; } = default!;

        // --- Método GET: Cargar el barbero por ID ---
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var barber = await _context.Barbers.FirstOrDefaultAsync(m => m.BarberId == id);

            if (barber == null) return NotFound();

            // Verificación de seguridad: Solo puede ver barberos de su Tenant.
            if (barber.TenantId != _tenantService.GetCurrentTenantId()) return Forbid();

            Barber = barber;
            return Page();
        }
    }
}