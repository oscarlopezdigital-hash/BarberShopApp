using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BarberShopApp.Data;
using BarberShopApp.Models;
using BarberShopApp.Services;
using Microsoft.AspNetCore.Mvc; // Necesario para JsonResult
using System.Linq;
using System; // Necesario para DateTime

namespace BarberShopApp.Pages.Appointments
{
    [Authorize] // Solo usuarios logueados pueden ver esta página
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantService _tenantService;

        // Clase auxiliar para el formato de eventos de FullCalendar
        public class FullCalendarEvent
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public System.DateTime Start { get; set; }
            public System.DateTime End { get; set; }
            public string Color { get; set; }
        }

        // Propiedad que contendrá la lista de citas a mostrar (para la tabla de diagnóstico)
        public IList<Appointment> Appointment { get; set; } = new List<Appointment>();

        // ---------------------------------------------------------------------------------------------------
        // 🎯 NUEVAS PROPIEDADES DE FILTRO DE FECHA PARA FULLCALENDAR (Input)
        // FullCalendar envía estos parámetros en el query string
        [BindProperty(SupportsGet = true)]
        public DateTime start { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime end { get; set; }
        // ---------------------------------------------------------------------------------------------------

        public IndexModel(ApplicationDbContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }


        public async Task OnGetAsync()
        {
            // Este método carga datos para la tabla de diagnóstico (si aún la usas)
            Appointment = await _context.Appointments
                .Include(a => a.Barber)
                .Include(a => a.Service)
                .OrderByDescending(a => a.DateTime)
                .ToListAsync();
        }

        // ---------------------------------------------------------------------------------------------------
        // 🚀 HANDLER AJAX ACTUALIZADO: Filtra citas por el rango de fechas 'start' y 'end'
        // ---------------------------------------------------------------------------------------------------
        public async Task<JsonResult> OnGetCalendarDataAsync()
        {
            // El TenantId ya se gestiona por el HasQueryFilter si lo tienes configurado globalmente.
            // Si no, debes seguir usando _tenantService.GetCurrentTenantId() como en el Paso 106.
            int tenantId = _tenantService.GetCurrentTenantId();

            // 1. Obtener citas filtradas:
            // a) Para el Tenant actual
            // b) Que estén Confirmadas
            // c) Que estén DENTRO del rango de fechas que FullCalendar solicita (start/end)
            var appointmentsQuery = _context.Appointments
                .Where(a => a.TenantId == tenantId && a.Status == "Confirmada");

            // Aplicar el filtro de rango de fechas que FullCalendar pide:
            if (start != default(DateTime) && end != default(DateTime))
            {
                // Incluimos citas cuya hora de inicio esté entre 'start' y 'end'
                appointmentsQuery = appointmentsQuery.Where(a => a.DateTime >= start && a.DateTime <= end);
            }

            var appointments = await appointmentsQuery
                .Include(a => a.Barber)
                .Include(a => a.Service)
                .OrderBy(a => a.DateTime)
                .ToListAsync();

            // 2. Convertir las citas a objetos FullCalendarEvent
            var events = appointments.Select(a => new FullCalendarEvent
            {
                Id = a.AppointmentId.ToString(),

                // Formato: [Barbero] Servicio (ej: [Juan] Corte Caballero)
                Title = $"[{a.Barber!.Name}] {a.Service!.Name}",

                Start = a.DateTime,

                // Calcular la hora de fin sumando la duración del servicio
                End = a.DateTime.AddMinutes(a.Service!.DurationMinutes),

                // Asignar un color basado en el barbero 
                Color = GetBarberColor(a.BarberId)

            }).ToList();

            return new JsonResult(events);
        }

        // Función auxiliar para asignar color a las citas
        private string GetBarberColor(int barberId)
        {
            // Código del switch clásico (compatible con versiones anteriores de C#)
            string color;
            switch (barberId % 4)
            {
                case 0:
                    color = "#3a87ad"; // Azul
                    break;
                case 1:
                    color = "#42d17c"; // Verde
                    break;
                case 2:
                    color = "#ffb347"; // Naranja
                    break;
                case 3:
                    color = "#7C4DFF"; // Púrpura
                    break;
                default:
                    color = "#cccccc"; // Gris (por defecto)
                    break;
            }
            return color;
        }
    }
}