using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BarberShopApp.Data;
using BarberShopApp.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System; // Necesario para DateTime

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

        // 🎯 PROPIEDAD DE DIAGNÓSTICO: Email del usuario logueado
        public string CurrentClientEmail { get; set; } = string.Empty;


        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            // Obtener el email del usuario logueado, normalizado a minúsculas
            // Usamos ToLower() en lugar de ToLowerInvariant() para asegurar la traducción a SQL
            var clientEmail = User.Identity?.Name?.ToLower();

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
                // FILTRO CORREGIDO (SQL Translation FIX): 
                // Usamos ToLower() en lugar de ToLowerInvariant() para que EF Core lo pueda traducir a SQL (LOWER()).
                .Where(a => !string.IsNullOrEmpty(a.ClientEmail) && a.ClientEmail.ToLower() == clientEmail)
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
            // También normalizamos el email del usuario logueado para la comprobación de seguridad (Usamos ToLower())
            var clientEmail = User.Identity?.Name?.ToLower();

            if (appointmentToCancel == null)
            {
                return NotFound();
            }

            // SEGURIDAD: Confirmar que la cita pertenece al usuario logueado antes de cancelarla
            if (appointmentToCancel.ClientEmail?.ToLower() != clientEmail) // Usamos ToLower()
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