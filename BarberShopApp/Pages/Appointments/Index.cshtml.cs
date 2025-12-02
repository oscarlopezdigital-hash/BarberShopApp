using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BarberShopApp.Data;
using BarberShopApp.Models;

namespace BarberShopApp.Pages.Appointments
{
    public class IndexModel : PageModel
    {
        private readonly BarberShopApp.Data.ApplicationDbContext _context;

        public IndexModel(BarberShopApp.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Appointment> Appointment { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Appointment = await _context.Appointments
                .Include(a => a.Barber)
                .Include(a => a.Service).ToListAsync();
        }
    }
}
