using System.ComponentModel.DataAnnotations;

namespace BarberShopApp.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        // Llaves foráneas
        [Required]
        public int BarberId { get; set; }
        public Barber? Barber { get; set; } // Propiedad de navegación

        [Required]
        public int ServiceId { get; set; }
        public Service? Service { get; set; } // Propiedad de navegación

        // Datos del Cliente (Podríamos usar el UserId de Identity, pero por simplicidad de la cita)
        [Required]
        [StringLength(100)]
        public string? ClientName { get; set; }

        [Required]
        [EmailAddress]
        public string? ClientEmail { get; set; } // Clave para las Notificaciones Automáticas

        // Fecha y Hora
        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha y Hora de la Cita")]
        public DateTime DateTime { get; set; }

        // Estado de la cita (Pendiente, Confirmada, Cancelada, etc.)
        public string Status { get; set; } = "Pendiente";



        // Clave Foránea de Tenant
        public int TenantId { get; set; }
        public Tenant? Tenant { get; set; } // Propiedad de Navegación
    }
}
