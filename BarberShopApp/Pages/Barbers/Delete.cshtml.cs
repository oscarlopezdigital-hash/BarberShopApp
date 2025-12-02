// Pages/Barbers/Delete.cshtml.cs
using BarberShopApp.Data;
using BarberShopApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BarberShopApp.Pages.Barbers
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Barber Barber { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Buscamos el barbero. Si no pertenece a este Tenant, será nulo.
            var barber = await _context.Barbers.FirstOrDefaultAsync(m => m.BarberId == id);

            if (barber == null)
            {
                return NotFound();
            }
            Barber = barber;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Buscamos de nuevo el barbero para asegurar que exista antes de eliminar
            var barber = await _context.Barbers.FindAsync(id);

            if (barber != null)
            {
                // El filtro de TenantId actuará aquí a nivel de DB para prevenir
                // que un usuario malicioso elimine un barbero de otro Tenant si intentara saltarse el OnGet.
                _context.Barbers.Remove(barber);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}