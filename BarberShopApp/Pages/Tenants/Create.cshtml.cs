// Pages/Tenants/Create.cshtml.cs
using BarberShopApp.Data;
using BarberShopApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;


namespace BarberShopApp.Pages.Tenants
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Tenant Tenant { get; set; } = new Tenant();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Tenants.Add(Tenant);
            await _context.SaveChangesAsync();

            // Opcional: Aquí se podría crear automáticamente la cuenta de administrador 
            // de Identity para este nuevo Tenant.

            return RedirectToPage("./Index");
        }
    }
}