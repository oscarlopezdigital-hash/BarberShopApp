using System.ComponentModel.DataAnnotations;

namespace BarberShopApp.Models
{
    public class Tenant
    {
        [Key]
        public int TenantId { get; set; } // ID único de la barbería

        [Required]
        [StringLength(100)]
        public string? Name { get; set; } // Nombre de la Barbería (Ej: "Barbería Classic Cuts")

        [Required]
        [StringLength(256)]
        public string? AdminEmail { get; set; } // Email del administrador principal
    }
}
