using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BarberShopApp.Data;
using BarberShopApp.Models;
using BarberShopApp.Services;

namespace BarberShopApp.Pages.Appointments
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantService _tenantService;

        [BindProperty]
        public Appointment Appointment { get; set; } = default!;

        public EditModel(ApplicationDbContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        // --- Método GET: Cargar la cita para editar ---
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            // Cargar la cita incluyendo Barbero y Servicio, filtrado por TenantId (automático).
            var appointment = await _context.Appointments
                                            .Include(a => a.Barber)
                                            .Include(a => a.Service)
                                            .FirstOrDefaultAsync(m => m.AppointmentId == id);

            if (appointment == null) return NotFound();

            // Verificación de seguridad adicional (aunque el QueryFilter debería ser suficiente)
            if (appointment.TenantId != _tenantService.GetCurrentTenantId()) return Forbid();

            Appointment = appointment;
            await PopulateSelectionLists();
            return Page();
        }

        // --- Método POST: Procesar la edición ---
        public async Task<IActionResult> OnPostAsync()
        {
            // La validación del modelo es crucial.
            if (!ModelState.IsValid)
            {
                await PopulateSelectionLists();
                return Page();
            }

            // 1. VALIDACIÓN DE HORARIOS (Replicamos las reglas de Create)

            // A. Restricción de Días (Excluir Domingo)
            if (Appointment.DateTime.DayOfWeek == DayOfWeek.Sunday)
            {
                ModelState.AddModelError("Appointment.DateTime", "❌ La barbería no abre los domingos. Por favor, elige otro día.");
                await PopulateSelectionLists();
                return Page();
            }

            // B. Restricción de Horas (Entre 9:00 AM y 6:00 PM)
            TimeSpan startTime = new TimeSpan(9, 0, 0);
            TimeSpan endTime = new TimeSpan(18, 0, 0);
            TimeSpan requestedTime = Appointment.DateTime.TimeOfDay;

            if (requestedTime < startTime || requestedTime > endTime)
            {
                ModelState.AddModelError("Appointment.DateTime", "❌ Horario no disponible. Solo se puede reservar entre las 9:00 AM y las 6:00 PM.");
                await PopulateSelectionLists();
                return Page();
            }

            // 2. VALIDACIÓN DE DISPONIBILIDAD (Excluyendo la cita que estamos editando)

            // Necesitamos la duración del servicio seleccionado para calcular el final de la cita.
            var service = await _context.Services.AsNoTracking().FirstOrDefaultAsync(s => s.ServiceId == Appointment.ServiceId);
            if (service == null)
            {
                ModelState.AddModelError(string.Empty, "Servicio no válido. Por favor, recarga la página.");
                await PopulateSelectionLists();
                return Page();
            }

            TimeSpan duration = TimeSpan.FromMinutes(service.DurationMinutes);
            DateTime appointmentStart = Appointment.DateTime;
            DateTime appointmentEnd = appointmentStart.Add(duration);

            if (await IsBarberUnavailable(Appointment.BarberId, appointmentStart, appointmentEnd, Appointment.AppointmentId))
            {
                ModelState.AddModelError("Appointment.DateTime", "❌ El barbero seleccionado no está disponible en este horario. Por favor, elige otra hora.");
                await PopulateSelectionLists();
                return Page();
            }

            // 3. GUARDAR LOS CAMBIOS

            // Adjuntamos la entidad para que Entity Framework sepa que ya existe y debe actualizarse
            _context.Attach(Appointment).State = EntityState.Modified;

            try
            {
                // Aseguramos que el TenantId no se pueda modificar
                _context.Entry(Appointment).Property(a => a.TenantId).IsModified = false;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppointmentExists(Appointment.AppointmentId)) return NotFound();
                else throw;
            }

            return RedirectToPage("./Index");
        }

        // --- Método Auxiliar: Comprobar la existencia de la cita ---
        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.AppointmentId == id);
        }

        // --- Método Auxiliar: Validar Disponibilidad (Modificado para Edición) ---
        private async Task<bool> IsBarberUnavailable(int barberId, DateTime newStart, DateTime newEnd, int currentAppointmentId)
        {
            var overlappingAppointments = await _context.Appointments
                .Include(a => a.Service)
                .Where(a =>
                    a.BarberId == barberId &&
                    a.Status == "Confirmada" &&
                    a.AppointmentId != currentAppointmentId && // <--- ¡EXCLUIR LA CITA ACTUAL!
                    newEnd > a.DateTime &&
                    newStart < a.DateTime.AddMinutes(a.Service!.DurationMinutes)
                )
                .AnyAsync();

            return overlappingAppointments;
        }


        // --- Método Auxiliar: Llenar las listas desplegables ---
        private async Task PopulateSelectionLists()
        {
            var currentTenantId = _tenantService.GetCurrentTenantId();

            // Cargar Barbers (filtrado automáticamente por TenantId)
            ViewData["BarberId"] = new SelectList(
                await _context.Barbers.ToListAsync(),
                "BarberId",
                "Name",
                Appointment.BarberId
            );

            // Cargar Services (filtrado automáticamente por TenantId)
            ViewData["ServiceId"] = new SelectList(
                await _context.Services.ToListAsync(),
                "ServiceId",
                "Name",
                Appointment.ServiceId
            );
        }
    }
}