using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BarberShopApp.Data;
using BarberShopApp.Models;
using BarberShopApp.Services;

namespace BarberShopApp.Pages.Appointments
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantService _tenantService;

        public DeleteModel(ApplicationDbContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        [BindProperty]
        public Appointment Appointment { get; set; } = default!;

        // --- Método GET: Cargar la cita para confirmar la eliminación ---
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            // Cargar la cita, incluyendo Barbero y Servicio.
            var appointment = await _context.Appointments
                                            .Include(a => a.Barber)
                                            .Include(a => a.Service)
                                            .FirstOrDefaultAsync(m => m.AppointmentId == id);

            if (appointment == null) return NotFound();

            // Verificación de seguridad: solo puede eliminar citas de su Tenant.
            if (appointment.TenantId != _tenantService.GetCurrentTenantId()) return Forbid();

            Appointment = appointment;
            return Page();
        }

        // --- Método POST: Ejecutar la eliminación ---
        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null) return NotFound();

            // Buscar la cita nuevamente (filtrado automáticamente por TenantId).
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment != null)
            {
                // Verificación de seguridad antes de eliminar
                if (appointment.TenantId != _tenantService.GetCurrentTenantId()) return Forbid();

                // 1. Marcar el estado de la entidad como eliminado
                Appointment = appointment;
                _context.Appointments.Remove(Appointment);

                // 2. Guardar los cambios en la base de datos
                await _context.SaveChangesAsync();
            }

            // Redirigir al listado principal
            return RedirectToPage("./Index");
        }
    }
}