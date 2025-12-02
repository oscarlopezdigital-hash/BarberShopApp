// Pages/Barbers/Edit.cshtml.cs
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
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
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

            // Buscamos el barbero. Si no existe o no pertenece a este Tenant, será nulo.
            var barber = await _context.Barbers.FirstOrDefaultAsync(m => m.BarberId == id);

            if (barber == null)
            {
                return NotFound();
            }
            Barber = barber;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Solo se actualizarán los campos visibles, manteniendo el TenantId original
                _context.Attach(Barber).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Barbers.Any(e => e.BarberId == Barber.BarberId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }
    }
}