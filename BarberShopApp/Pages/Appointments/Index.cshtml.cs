using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // Necesario para restringir el acceso
using BarberShopApp.Data;
using BarberShopApp.Models;
using BarberShopApp.Services;

namespace BarberShopApp.Pages.Appointments
{
    [Authorize] // Solo usuarios logueados pueden ver esta página
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantService _tenantService; // No se usa directamente aquí, pero es buena práctica mantener la inyección si la clase lo necesita

        // Propiedad que contendrá la lista de citas a mostrar
        public IList<Appointment> Appointment { get; set; } = new List<Appointment>();

        public IndexModel(ApplicationDbContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        public async Task OnGetAsync()
        {
            // Cargamos todas las citas del Tenant actual.
            // Los métodos .Include() aseguran que traemos los datos de las tablas relacionadas (Barber y Service).
            // La magia del Multi-Tenancy (filtrado por TenantId) se aplica automáticamente
            // por el HasQueryFilter que definiste en ApplicationDbContext.
            Appointment = await _context.Appointments
                .Include(a => a.Barber)
                .Include(a => a.Service)
                .OrderByDescending(a => a.DateTime) // Ordenamos por fecha más reciente primero
                .ToListAsync();
        }
    }
}