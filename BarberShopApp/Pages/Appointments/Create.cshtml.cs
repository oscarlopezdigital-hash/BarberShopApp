// Pages/Appointments/Create.cshtml.cs

using BarberShopApp.Data;
using BarberShopApp.Models;
using BarberShopApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BarberShopApp.Pages.Appointments
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ITenantService _tenantService;

        // Constructor con Inyección de Dependencias
        public CreateModel(ApplicationDbContext context, IEmailService emailService, IConfiguration configuration, ITenantService tenantService)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
            _tenantService = tenantService;
        }

        public SelectList BarberOptions { get; set; }
        public SelectList ServiceOptions { get; set; }

        [BindProperty]
        public Appointment Appointment { get; set; } = new Appointment();

        // Propiedades separadas para los inputs de fecha y hora (Paso 97)
        [BindProperty]
        public DateTime SelectedDate { get; set; }

        [BindProperty]
        [DataType(DataType.Time)]
        public DateTime SelectedTime { get; set; }

        // OnGet: Carga inicial de listas desplegables
        public async Task<IActionResult> OnGet()
        {
            await PopulateSelectionLists();
            return Page();
        }

        // ---------------------------------------------------------------------------------------------------
        // 🚀 HANDLER AJAX: Verifica disponibilidad en tiempo real (Paso 99)
        // ---------------------------------------------------------------------------------------------------
        public async Task<IActionResult> OnGetCheckAvailabilityAsync(
            int barberId,
            DateTime date,
            DateTime time,
            int serviceId)
        {
            // 1. Combinar la fecha y la hora para obtener el inicio de la cita
            DateTime appointmentStart = date.Date.Add(time.TimeOfDay);

            // 2. Obtener la duración del servicio
            var service = await _context.Services
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(s => s.ServiceId == serviceId);

            if (service == null)
            {
                // Si serviceId no es válido (ej. 0 o no encontrado)
                return new JsonResult(new { isAvailable = false, message = "Servicio no válido. Por favor, selecciona un servicio." });
            }

            TimeSpan duration = TimeSpan.FromMinutes(service.DurationMinutes);
            DateTime appointmentEnd = appointmentStart.Add(duration);

            // A. Restricción de Días (Excluir Domingo)
            if (appointmentStart.DayOfWeek == DayOfWeek.Sunday)
            {
                return new JsonResult(new { isAvailable = false, message = "❌ La barbería no abre los domingos." });
            }

            // B. Restricción de Horas (9:00 AM a 6:00 PM)
            TimeSpan startTime = new TimeSpan(9, 0, 0);
            TimeSpan endTime = new TimeSpan(18, 0, 0);
            TimeSpan requestedTime = appointmentStart.TimeOfDay;

            if (requestedTime < startTime || requestedTime >= endTime)
            {
                return new JsonResult(new { isAvailable = false, message = "❌ Horario fuera del rango de 9:00 AM a 6:00 PM." });
            }

            // C. Validar Superposición de Horarios del Barbero
            if (await IsBarberUnavailable(barberId, appointmentStart, appointmentEnd))
            {
                return new JsonResult(new { isAvailable = false, message = "❌ El barbero no está disponible en este horario." });
            }

            // Si pasa todas las validaciones
            return new JsonResult(new { isAvailable = true, message = "✅ ¡Barbero disponible!" });
        }


        // ---------------------------------------------------------------------------------------------------
        // 💾 HANDLER DE POSTEO: Guarda la cita y envía correos
        // ---------------------------------------------------------------------------------------------------
        public async Task<IActionResult> OnPostAsync()
        {
            // 1. Validación de Datos (DataAnnotations) en SelectedDate, SelectedTime, etc.
            if (!ModelState.IsValid)
            {
                await PopulateSelectionLists();
                return Page();
            }

            // --- 2. COMBINAR SelectedDate y SelectedTime ---
            Appointment.DateTime = SelectedDate.Date.Add(SelectedTime.TimeOfDay);


            // --- 3. VALIDACIONES DE DÍAS Y HORARIOS DE LA BARBERÍA (Repetidas por seguridad) ---

            // A. Restricción de Días (Excluir Domingo)
            if (Appointment.DateTime.DayOfWeek == DayOfWeek.Sunday)
            {
                ModelState.AddModelError("SelectedDate", "❌ La barbería no abre los domingos. Por favor, elige otro día.");
                await PopulateSelectionLists();
                return Page();
            }

            // B. Restricción de Horas (Entre 9:00 AM y 6:00 PM)
            TimeSpan startTime = new TimeSpan(9, 0, 0);
            TimeSpan endTime = new TimeSpan(18, 0, 0);
            TimeSpan requestedTime = Appointment.DateTime.TimeOfDay;

            if (requestedTime < startTime || requestedTime >= endTime)
            {
                ModelState.AddModelError("SelectedTime", "❌ Horario no disponible. Solo se puede reservar entre las 9:00 AM y las 6:00 PM.");
                await PopulateSelectionLists();
                return Page();
            }

            // --- LÓGICA DE MULTI-TENANCY ---
            Appointment.TenantId = _tenantService.GetCurrentTenantId();

            // 4. Obtener la duración del servicio y calcular la hora de fin
            var service = await _context.Services
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(s => s.ServiceId == Appointment.ServiceId);

            if (service == null)
            {
                ModelState.AddModelError(string.Empty, "Servicio no válido. Por favor, recarga la página.");
                await PopulateSelectionLists();
                return Page();
            }

            TimeSpan duration = TimeSpan.FromMinutes(service.DurationMinutes);
            DateTime appointmentStart = Appointment.DateTime;
            DateTime appointmentEnd = appointmentStart.Add(duration);

            // 5. Comprobar Superposición de Horarios (Validación de Disponibilidad)
            if (await IsBarberUnavailable(Appointment.BarberId, appointmentStart, appointmentEnd))
            {
                // Este mensaje debe coincidir con el del handler AJAX
                ModelState.AddModelError("SelectedTime", "❌ El barbero seleccionado no está disponible en este horario. Por favor, elige otra hora.");
                await PopulateSelectionLists();
                return Page();
            }

            // --- 6. SI TODAS LAS VALIDACIONES PASAN, GUARDAR LA CITA ---
            Appointment.Status = "Confirmada";

            _context.Appointments.Add(Appointment);
            await _context.SaveChangesAsync();

            // Cargar el Barbero y Servicio para la notificación
            var barber = await _context.Barbers.FindAsync(Appointment.BarberId);

            // --- 7. Enviar Notificaciones Automatizadas ---
            // A. Notificación al Cliente
            string clientSubject = $"✅ Cita Confirmada: {service.Name} con {barber!.Name}";
            string clientBody = $@"
                <h1>¡Cita Confirmada, {Appointment.ClientName}!</h1>
                <p>Tu cita ha sido reservada con éxito en BarberShopApp:</p>
                <ul>
                    <li><strong>Barbero:</strong> {barber.Name}</li>
                    <li><strong>Servicio:</strong> {service.Name}</li>
                    <li><strong>Fecha y Hora:</strong> {Appointment.DateTime.ToString("dddd, dd MMMM yyyy, HH:mm")}</li>
                </ul>
                <p>Gracias por tu reserva. ¡Te esperamos!</p>
            ";
            await _emailService.SendEmailAsync(Appointment.ClientEmail, clientSubject, clientBody);


            // B. Notificación al Barbero/Administración
            string barberEmail = _configuration["EmailSettings:BarberEmail"];
            string barberSubject = $"🔔 NUEVA RESERVA: {service.Name} a las {Appointment.DateTime.ToString("HH:mm")}";
            string barberBody = $@"
                <h2>Nueva Cita Reservada</h2>
                <p>Se ha reservado una nueva cita para el Barbero: <strong>{barber.Name}</strong>.</p>
                <ul>
                    <li><strong>Cliente:</strong> {Appointment.ClientName} ({Appointment.ClientEmail})</li>
                    <li><strong>Servicio:</strong> {service.Name}</li>
                    <li><strong>Duración Estimada:</strong> {service.DurationMinutes} minutos</li>
                    <li><strong>Fecha y Hora:</strong> {Appointment.DateTime.ToString("dddd, dd MMMM yyyy, HH:mm")}</li>
                </ul>
            ";
            await _emailService.SendEmailAsync(barberEmail, barberSubject, barberBody);

            // 8. Redirigir a una página de confirmación
            return RedirectToPage("./Confirmation", new { id = Appointment.AppointmentId });
        }

        // ---------------------------------------------------------------------------------------------------
        // 🧩 MÉTODOS AUXILIARES
        // ---------------------------------------------------------------------------------------------------

        private async Task PopulateSelectionLists()
        {
            // El filtro por TenantId ya debe estar aplicado por el Query Filter del DbContext
            BarberOptions = new SelectList(await _context.Barbers.ToListAsync(), "BarberId", "Name");
            ServiceOptions = new SelectList(await _context.Services.ToListAsync(), "ServiceId", "Name");
        }

        private async Task<bool> IsBarberUnavailable(int barberId, DateTime newStart, DateTime newEnd)
        {
            // Lógica de superposición: newEnd > a.DateTime && newStart < a.DateTime + Duración
            var overlappingAppointments = await _context.Appointments
                .Include(a => a.Service)
                .Where(a =>
                    a.BarberId == barberId &&
                    a.Status == "Confirmada" &&
                    newEnd > a.DateTime && // El nuevo fin es después del inicio de la cita existente
                    newStart < a.DateTime.AddMinutes(a.Service!.DurationMinutes) // El nuevo inicio es antes del fin de la cita existente
                )
                .AnyAsync();

            return overlappingAppointments;
        }
    }
}