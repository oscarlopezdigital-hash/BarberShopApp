using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BarberShopApp.Data;
using BarberShopApp.Models;
using BarberShopApp.Services;

namespace BarberShopApp.Pages.Appointments
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantService _tenantService;

        public DetailsModel(ApplicationDbContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        public Appointment Appointment { get; set; } = default!;

        // --- Método GET: Cargar la cita por ID ---
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Cargamos la cita, asegurando incluir Barbero y Servicio.
            var appointment = await _context.Appointments
                .Include(a => a.Barber)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(m => m.AppointmentId == id);

            if (appointment == null)
            {
                return NotFound();
            }

            // Verificación de seguridad: Asegurar que el usuario solo vea citas de su Tenant.
            if (appointment.TenantId != _tenantService.GetCurrentTenantId())
            {
                return Forbid();
            }

            Appointment = appointment;
            return Page();
        }
    }
}