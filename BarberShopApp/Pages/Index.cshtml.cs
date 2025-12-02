using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BarberShopApp.Data;
using BarberShopApp.Models;
using BarberShopApp.Services;

namespace BarberShopApp.Pages
{
    // Restringimos el acceso al Dashboard solo a usuarios autenticados
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantService _tenantService;

        public IndexModel(ApplicationDbContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        // Propiedad para almacenar las citas de hoy
        public IList<Appointment> AppointmentsToday { get; set; } = new List<Appointment>();

        // Propiedad para el KPI: Citas totales de hoy
        public int TotalAppointmentsToday { get; set; }

        // Propiedad para la próxima cita
        public Appointment? NextAppointment { get; set; }


        public async Task OnGetAsync()
        {
            var today = DateTime.Today;
            var currentTenantId = _tenantService.GetCurrentTenantId();

            // 1. Obtener todas las citas confirmadas del Tenant actual para HOY
            var allAppointmentsToday = await _context.Appointments
                .Include(a => a.Barber)
                .Include(a => a.Service)
                .Where(a =>
                    a.TenantId == currentTenantId &&
                    a.DateTime.Date == today &&
                    a.Status == "Confirmada"
                )
                .OrderBy(a => a.DateTime) // Ordenar cronológicamente
                .ToListAsync();

            AppointmentsToday = allAppointmentsToday;
            TotalAppointmentsToday = allAppointmentsToday.Count;

            // 2. Identificar la Próxima Cita (que aún no ha pasado)
            NextAppointment = allAppointmentsToday
                .Where(a => a.DateTime > DateTime.Now)
                .FirstOrDefault();
        }
    }
}