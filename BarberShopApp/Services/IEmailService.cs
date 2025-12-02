// Services/IEmailService.cs
using System.Threading.Tasks;

namespace BarberShopApp.Services
{
    public interface IEmailService
    {
        // ¡DEBE TERMINAR EN PUNTO Y COMA, NO DEBE TENER LLAVES!
        Task SendEmailAsync(string toEmail, string subject, string message);
    }
}