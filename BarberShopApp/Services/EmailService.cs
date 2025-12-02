// Services/EmailService.cs
using System;
using System.Net;
using System.Net.Security;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace BarberShopApp.Services
{
    // Asegúrate de que IEmailService está definido en tu proyecto
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            // Lectura de la configuración
            var host = _configuration["EmailSettings:SmtpHost"];
            var port = int.Parse(_configuration["EmailSettings:SmtpPort"]);
            var user = _configuration["EmailSettings:SmtpUser"];
            var pass = _configuration["EmailSettings:SmtpPass"];
            var senderEmail = _configuration["EmailSettings:SenderEmail"];

            try
            {
                // --------------------------------------------------------------------------------------
                // SOLUCIÓN: Asignar el callback de validación de certificado a nivel global.
                // --------------------------------------------------------------------------------------
                System.Net.ServicePointManager.ServerCertificateValidationCallback = ValidateCertificate;
                // --------------------------------------------------------------------------------------

                using (var client = new SmtpClient(host, port))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(user, pass);
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail, "BarberShop App"),
                        Subject = subject,
                        Body = message,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                // Si el correo falla, registramos el error para debug.
                Console.WriteLine($"Error al enviar correo a {toEmail}: {ex.Message}");
            }
        }

        // --------------------------------------------------------------------------------------
        // MÉTODO DE VALIDACIÓN DE CERTIFICADO AISLADO
        // --------------------------------------------------------------------------------------
        private static bool ValidateCertificate(object sender,
                                                X509Certificate? certificate,
                                                X509Chain? chain,
                                                SslPolicyErrors sslPolicyErrors)
        {
            // Retorna TRUE siempre para ignorar el error UntrustedRoot en desarrollo.
            return true;
        }
        // --------------------------------------------------------------------------------------
    }
}