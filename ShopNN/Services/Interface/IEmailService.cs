using System.Threading.Tasks;

namespace ShopNN.Services.Interface
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body, string? toName = null);
    }
}
