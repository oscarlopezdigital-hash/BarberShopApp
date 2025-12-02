// Models/Service.cs

using System.ComponentModel.DataAnnotations;

namespace BarberShopApp.Models
{
    public class Service
    {
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "El nombre del servicio es obligatorio.")]
        [StringLength(100)]
        public string? Name { get; set; }

        // *** ¡AÑADIR ESTA PROPIEDAD! ***
        [Display(Name = "Descripción del Servicio")]
        public string? Description { get; set; }
        // ********************************

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, 1000.00, ErrorMessage = "El precio debe ser positivo.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "La duración es obligatoria.")]
        [Display(Name = "Duración (minutos)")]
        public int DurationMinutes { get; set; }

        // Clave Foránea de Tenant
        public int TenantId { get; set; }
        public Tenant? Tenant { get; set; } // Propiedad de Navegación
    }
}