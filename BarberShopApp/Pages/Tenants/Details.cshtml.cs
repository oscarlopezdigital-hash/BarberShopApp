using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BarberShopApp.Data;
using BarberShopApp.Models;

namespace BarberShopApp.Pages.Tenants
{
    public class DetailsModel : PageModel
    {
        private readonly BarberShopApp.Data.ApplicationDbContext _context;

        public DetailsModel(BarberShopApp.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public Tenant Tenant { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tenant = await _context.Tenants.FirstOrDefaultAsync(m => m.TenantId == id);
            if (tenant == null)
            {
                return NotFound();
            }
            else
            {
                Tenant = tenant;
            }
            return Page();
        }
    }
}
