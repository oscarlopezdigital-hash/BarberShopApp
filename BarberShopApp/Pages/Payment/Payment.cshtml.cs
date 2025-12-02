// Pages/Payment/Payment.cshtml.cs
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration; // Necesario para IConfiguration
using Stripe; // Necesario para Stripe
using System.Linq; // Necesario para List

namespace BarberShopApp.Pages.Payment
{
    public class PaymentModel : PageModel
    {
        private readonly IConfiguration _configuration;

        // Propiedades que se usarán en la vista (Payment.cshtml)
        [BindProperty]
        public string ClientSecret { get; set; } = string.Empty;

        public string StripePublishableKey { get; set; } = string.Empty;

        // Se puede usar para mostrar el monto en la vista (ej: $15.00)
        public int AmountInCents { get; set; } = 1500; // Valor por defecto: 15.00 EUR/USD

        public PaymentModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult OnGet(int? appointmentId)
        {
            // 1. Obtener la clave publicable del proyecto (guardada en User Secrets)
            StripePublishableKey = _configuration["Stripe:PublishableKey"] ?? string.Empty;

            // 2. (Opcional) Lógica para calcular el monto real si usas appointmentId
            // if (appointmentId.HasValue) 
            // { 
            //    // Buscar la cita y obtener el precio 
            //    // AmountInCents = service.Price * 100;
            // }

            // 3. Crear las opciones para el intento de pago
            var options = new PaymentIntentCreateOptions
            {
                Amount = AmountInCents,
                Currency = "eur", // Ajusta a la moneda que uses ("eur", "usd", etc.)
                PaymentMethodTypes = new List<string> { "card" },
                Description = "Pago por reserva de cita BarberShopApp"
            };

            // 4. Llamar al servicio de Stripe para crear el intento
            try
            {
                var service = new PaymentIntentService();
                var intent = service.Create(options);

                // 5. Guardar el ClientSecret para el front-end
                ClientSecret = intent.ClientSecret;
            }
            catch (StripeException e)
            {
                // Manejo de error si la creación del PaymentIntent falla
                TempData["ErrorMessage"] = $"Error al iniciar el pago: {e.Message}";
                return RedirectToPage("/Error");
            }

            return Page();
        }
    }
}