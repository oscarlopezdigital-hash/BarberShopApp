// Pages/Services/Delete.cshtml.cs
using BarberShopApp.Data;
using BarberShopApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

[Authorize]
public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DeleteModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Service Service { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        // Buscamos el servicio. Si no existe o no pertenece a este Tenant, será nulo.
        var service = await _context.Services.FirstOrDefaultAsync(m => m.ServiceId == id);

        if (service == null)
        {
            return NotFound();
        }
        Service = service;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        // Recuperamos el servicio que queremos borrar.
        var service = await _context.Services.FindAsync(id);

        // Aunque el filtro de TenantId no está activo en FindAsync, 
        // solo podemos borrarlo si la consulta anterior en OnGetAsync funcionó.
        // Y además, si el registro no pertenece a nuestro tenant, la BD no permitirá el borrado.

        if (service != null)
        {
            Service = service;
            _context.Services.Remove(Service);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}