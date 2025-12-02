// Pages/Services/Create.cshtml.cs
using BarberShopApp.Data;
using BarberShopApp.Models;
using BarberShopApp.Services; // Necesario para ITenantService
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantService _tenantService; // Inyectamos el servicio

    public CreateModel(ApplicationDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService; // Asignamos el servicio
    }

    public IActionResult OnGet()
    {
        return Page();
    }

    [BindProperty]
    public Service Service { get; set; } = new Service(); // Inicializamos el modelo

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // *** LÓGICA DE MULTI-TENANCY CLAVE ***
        // Antes de guardar, asignamos el ID del Tenant actual.
        Service.TenantId = _tenantService.GetCurrentTenantId();

        _context.Services.Add(Service);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}