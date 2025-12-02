// Models/Barber.cs
using System.ComponentModel.DataAnnotations;

namespace BarberShopApp.Models
{
    public class Barber
    {
        public int BarberId { get; set; }

        [Required(ErrorMessage = "El nombre del barbero es obligatorio.")]
        [StringLength(150)]
        public string? Name { get; set; }

        [Display(Name = "Descripción/Especialidad")]
        public string? Description { get; set; }

        // *** AÑADIR ESTAS LÍNEAS ***
        [Required(ErrorMessage = "El email es obligatorio para notificaciones.")]
        [EmailAddress(ErrorMessage = "Formato de email incorrecto.")]
        [Display(Name = "Email de Contacto")]
        [StringLength(256)]
        public string? Email { get; set; }
        // ***************************

        public int TenantId { get; set; }
        public Tenant? Tenant { get; set; }
    }
}