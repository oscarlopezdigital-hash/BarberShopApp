using BarberShopApp.Data;
using BarberShopApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System; // Necesario para DateTime
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarberShopApp.Pages.Client
{
    // Solo usuarios logueados (clientes) pueden acceder a esta página
    [Authorize]
    // Utilizamos el área 'Client' para organizar las páginas
    [Area("Client")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        // Propiedad que contendrá las citas del cliente
        public IList<Appointment> Appointments { get; set; } = new List<Appointment>();

        // Propiedad para diferenciar citas futuras de pasadas
        public IList<Appointment> FutureAppointments { get; set; } = new List<Appointment>();
        public IList<Appointment> PastAppointments { get; set; } = new List<Appointment>();

        // 🎯 NUEVA PROPIEDAD DE DIAGNÓSTICO: Email del usuario logueado
        public string CurrentClientEmail { get; set; } = string.Empty;


        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            // Obtener el email del usuario logueado para filtrar sus citas.
            // User.Identity.Name es el email/username del usuario autenticado (siempre en minúsculas en ASP.NET Identity)
            var clientEmail = User.Identity?.Name?.ToLowerInvariant();

            // Guardamos el email para mostrarlo en la vista (diagnóstico)
            CurrentClientEmail = clientEmail ?? "Usuario No Autenticado";

            // ---------------------------------------------------------------
            // 🎯 DIAGNÓSTICO CLAVE: Imprime el email del usuario logueado en la consola del servidor.
            Console.WriteLine($"DEBUG CLIENT PANEL: Usuario logueado: {clientEmail ?? "NULO"}");
            // ---------------------------------------------------------------

            if (string.IsNullOrEmpty(clientEmail))
            {
                // Si no hay email, no cargamos nada.
                return;
            }

            // 1. Cargar TODAS las citas del cliente, incluyendo Barbero y Servicio
            Appointments = await _context.Appointments
                .Include(a => a.Barber)
                .Include(a => a.Service)
                // FILTRO CORREGIDO Y MÁS ROBUSTO: 
                // Aseguramos que el ClientEmail de la DB esté en minúsculas antes de compararlo
                .Where(a => a.ClientEmail != null && a.ClientEmail.ToLowerInvariant() == clientEmail)
                .OrderByDescending(a => a.DateTime)
                .ToListAsync();

            // 2. Separar citas en futuras y pasadas para mostrarlas por separado en la vista
            FutureAppointments = Appointments
                .Where(a => a.DateTime >= DateTime.Now)
                .ToList();

            PastAppointments = Appointments
                .Where(a => a.DateTime < DateTime.Now)
                .ToList();
        }

        // ---------------------------------------------------------------------------------------------------
        // 🚀 HANDLER PARA CANCELACIÓN DE CITA
        // ---------------------------------------------------------------------------------------------------
        public async Task<IActionResult> OnPostCancelAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointmentToCancel = await _context.Appointments.FindAsync(id);
            // También normalizamos el email del usuario logueado para la comprobación de seguridad
            var clientEmail = User.Identity?.Name?.ToLowerInvariant();

            if (appointmentToCancel == null)
            {
                return NotFound();
            }

            // SEGURIDAD: Confirmar que la cita pertenece al usuario logueado antes de cancelarla
            if (appointmentToCancel.ClientEmail?.ToLowerInvariant() != clientEmail) // Usamos la comprobación normalizada
            {
                // Si no coincide, devolvemos un error de Acceso Denegado
                return Forbid();
            }

            // Actualizar el estado de la cita a "Cancelada"
            appointmentToCancel.Status = "Cancelada";
            _context.Attach(appointmentToCancel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Manejo de error de concurrencia
                throw;
            }

            // Redirigir de nuevo al panel con un mensaje de éxito (usando TempData)
            TempData["StatusMessage"] = "Su cita ha sido cancelada exitosamente.";
            return RedirectToPage();
        }
    }
}
