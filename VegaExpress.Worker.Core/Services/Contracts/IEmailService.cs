using VegaExpress.Worker.Core.Models.Settings;

namespace VegaExpress.Worker.Core.Services.Contracts
{
    public interface IEmailService
    {
        Task SendEmailAsync(MailRequest mailRequest);
    }
}
