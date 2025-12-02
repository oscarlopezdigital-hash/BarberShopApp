// Pages/Barbers/Index.cshtml.cs
using BarberShopApp.Data;
using BarberShopApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShopApp.Pages.Barbers
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Propiedad que contendrá la lista de barberos del Tenant actual
        public IList<Barber> Barber { get; set; } = new List<Barber>();

        public async Task OnGetAsync()
        {
            // NOTA: La consulta es segura; solo trae barberos del TenantId del usuario logeado.
            Barber = await _context.Barbers.ToListAsync();
        }
    }
}