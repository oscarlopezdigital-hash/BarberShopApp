// Pages/Tenants/Index.cshtml.cs
using BarberShopApp.Data;
using BarberShopApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

// IMPORTANTE: Aquí no aplicamos un filtro de TenantId,
// ya que un Super-Admin necesita ver a *todos* los Tenants.

// Para el control de acceso real, usarías Roles o Claims.
// Por simplicidad, solo autorizamos a usuarios logueados por ahora.

namespace BarberShopApp.Pages.Tenants // Usamos namespace para evitar conflictos
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Tenant> Tenant { get; set; } = new List<Tenant>();

        public async Task OnGetAsync()
        {
            // Aquí NO se aplica el filtro de TenantId, por eso vemos todos.
            Tenant = await _context.Tenants.ToListAsync();
        }
    }
}