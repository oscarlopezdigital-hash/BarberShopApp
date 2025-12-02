// Pages/Services/Edit.cshtml.cs
using BarberShopApp.Data;
using BarberShopApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

[Authorize]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModel(ApplicationDbContext context)
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

        // Buscamos el servicio. El filtro de TenantId está activo aquí.
        // Si el ID existe, pero no pertenece a este Tenant, 'service' será nulo.
        var service = await _context.Services.FirstOrDefaultAsync(m => m.ServiceId == id);

        if (service == null)
        {
            return NotFound();
        }
        Service = service;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            // La base de datos no permitirá que se cambie el TenantId,
            // pero actualizamos los otros campos.
            _context.Attach(Service).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Services.Any(e => e.ServiceId == Service.ServiceId))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return RedirectToPage("./Index");
    }
}