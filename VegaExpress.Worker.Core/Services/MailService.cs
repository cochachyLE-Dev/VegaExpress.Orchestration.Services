using MailKit.Net.Smtp;
using MailKit.Security;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MimeKit;

using VegaExpress.Worker.Core.Models.Settings;
using VegaExpress.Worker.Core.Services.Contracts;

namespace VegaExpress.Worker.Core.Services
{
    public class MailService : IEmailService
    {
        public MailSettings _mailSettings { get; }
        public ILogger<MailService> _logger { get; }

        public MailService(IOptions<MailSettings> mailSettings, ILogger<MailService> logger)
        {
            _mailSettings = mailSettings.Value;
            _logger = logger;
        }
        public async Task SendEmailAsync(MailRequest mailRequest)
        {
            try
            {
                // create message
                var email = new MimeMessage();
                email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
                email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
                email.Subject = mailRequest.Subject;
                var builder = new BodyBuilder();
                builder.HtmlBody = mailRequest.Body;
                email.Body = builder.ToMessageBody();
                using var smtp = new SmtpClient();
                smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                await smtp.SendAsync(email);
                smtp.Disconnect(true);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw new InvalidOperationException(ex.Message);
            }
        }
    }
}
