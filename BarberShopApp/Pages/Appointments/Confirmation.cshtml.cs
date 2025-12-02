using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BarberShopApp.Data;
using BarberShopApp.Models;
using BarberShopApp.Services;

namespace BarberShopApp.Pages.Appointments
{
    public class ConfirmationModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantService _tenantService;

        public ConfirmationModel(ApplicationDbContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        public Appointment? Appointment { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Cargar la cita, incluyendo el Barbero y el Servicio (necesarios para la vista).
            // El filtro de TenantId se aplica automáticamente gracias al HasQueryFilter en DbContext.
            Appointment = await _context.Appointments
                .Include(a => a.Barber)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(m => m.AppointmentId == id);

            if (Appointment == null)
            {
                return NotFound();
            }

            // Seguridad: Doble chequeo para asegurar que la cita pertenezca al Tenant actual.
            if (Appointment.TenantId != _tenantService.GetCurrentTenantId())
            {
                return Forbid();
            }

            return Page();
        }
    }
}