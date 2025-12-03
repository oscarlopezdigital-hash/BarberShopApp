// Pages/Appointments/Create.cshtml.cs
using BarberShopApp.Data;
using BarberShopApp.Models;
using BarberShopApp.Services; // Necesario para IEmailService
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; // Necesario para leer BarberEmail
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
        private readonly IConfiguration _configuration; // Para obtener el email del Barbero/Admin
        private readonly ITenantService _tenantService; // <--- 1. DECLARACIÓN

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

        // Pages/Appointments/Create.cshtml.cs

        public async Task<IActionResult> OnPostAsync()
        {
            // 1. Validación de Datos (DataAnnotations) en SelectedDate, SelectedTime, etc.
            if (!ModelState.IsValid)
            {
                await PopulateSelectionLists();
                return Page();
            }

            Appointment.DateTime = SelectedDate.Date.Add(SelectedTime.TimeOfDay);


            // --- 3. NUEVAS VALIDACIONES: DÍAS Y HORARIOS DE LA BARBERÍA ---

            // A. Restricción de Días (Excluir Domingo)
            if (Appointment.DateTime.DayOfWeek == DayOfWeek.Sunday)
            {
                // Se usa la clave "SelectedDate" ya que el error se aplica a ese input.
                ModelState.AddModelError("SelectedDate", "❌ La barbería no abre los domingos. Por favor, elige otro día.");
                await PopulateSelectionLists();
                return Page();
            }

            // B. Restricción de Horas (Entre 9:00 AM y 6:00 PM)
            TimeSpan startTime = new TimeSpan(9, 0, 0);  // 9:00 AM
            TimeSpan endTime = new TimeSpan(18, 0, 0); // 6:00 PM (18:00)
            TimeSpan requestedTime = Appointment.DateTime.TimeOfDay;

            // La cita debe empezar ANTES de la hora de inicio O en la hora de cierre o después (>= 18:00)
            if (requestedTime < startTime || requestedTime >= endTime)
            {
                // Se usa la clave "SelectedTime" ya que el error se aplica a ese input.
                ModelState.AddModelError("SelectedTime", "❌ Horario no disponible. Solo se puede reservar entre las 9:00 AM y las 6:00 PM.");
                await PopulateSelectionLists();
                return Page();
            }

            // ----------------------------------------------------------------------------------


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
                // Se usa la clave "SelectedTime" o "SelectedDate" para mostrar el error junto a los inputs.
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
            string clientSubject = $"✅ Cita Confirmada: {service.Name} con {barber.Name}";
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

        private async Task PopulateSelectionLists()
        {
            BarberOptions = new SelectList(await _context.Barbers.ToListAsync(), "BarberId", "Name");
            ServiceOptions = new SelectList(await _context.Services.ToListAsync(), "ServiceId", "Name");
        }

        private async Task<bool> IsBarberUnavailable(int barberId, DateTime newStart, DateTime newEnd)
        {
            // Lógica de superposición: Si el nuevo rango (newStart/newEnd) se cruza con una cita existente (a)
            var overlappingAppointments = await _context.Appointments
                .Include(a => a.Service)
                .Where(a =>
                    a.BarberId == barberId &&
                    a.Status == "Confirmada" && // Solo comprobar si el estado es 'Confirmada'
                    newEnd > a.DateTime &&
                    newStart < a.DateTime.AddMinutes(a.Service.DurationMinutes)
                )
                .AnyAsync();

            return overlappingAppointments;
        }
    }
}