using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BarberShopApp.Data;
using BarberShopApp.Models;

namespace BarberShopApp.Pages.Barbers
{
    public class DetailsModel : PageModel
    {
        private readonly BarberShopApp.Data.ApplicationDbContext _context;

        public DetailsModel(BarberShopApp.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public Barber Barber { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var barber = await _context.Barbers.FirstOrDefaultAsync(m => m.BarberId == id);
            if (barber == null)
            {
                return NotFound();
            }
            else
            {
                Barber = barber;
            }
            return Page();
        }
    }
}
