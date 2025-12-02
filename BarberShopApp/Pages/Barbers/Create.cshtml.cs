// Pages/Barbers/Create.cshtml.cs

using BarberShopApp.Data;
using BarberShopApp.Models;
using BarberShopApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace BarberShopApp.Pages.Barbers
{
    // La clase debe llamarse CreateModel y no debe estar dentro de un 'namespace' 
    // para que Razor Pages la reconozca automáticamente si el archivo está en Pages/Barbers/.
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantService _tenantService;

        // Constructor: Inyectamos el DbContext y el TenantService
        public CreateModel(ApplicationDbContext context, ITenantService tenantService)
        {
            _context = context; // <-- Esto resuelve el error en _context
            _tenantService = tenantService;
        }

        public IActionResult OnGet() // <-- Esto resuelve el error en OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Barber Barber { get; set; } = new Barber();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // *** LÓGICA DE MULTI-TENANCY CLAVE ***
            // Asignamos el ID del Tenant actual antes de guardar.
            Barber.TenantId = _tenantService.GetCurrentTenantId();

            _context.Barbers.Add(Barber);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}