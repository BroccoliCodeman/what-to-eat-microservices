using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;
using Recipes.BLL.Configurations;
using Recipes.BLL.Interfaces;
using System.Text;

namespace Recipes.BLL.Services
{

    public class EmailSender : IEmailSender
    {
        public EmailSender(ILogger<EmailSender> logger, EmailSenderConfiguration configuration)
        {
            Logger = logger;
            Configuration = configuration;
        }

        public ILogger<EmailSender> Logger { get; }
        public EmailSenderConfiguration Configuration { get; }

        public async Task SendEmailAsync(string recipientEmail, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(Configuration.Name, Configuration.Adress));
            message.To.Add(new MailboxAddress("", recipientEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = body;

            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                client.Connect(Configuration.Host, Configuration.Port, Configuration.UseSsl);
                client.Authenticate(Configuration.Adress, Configuration.Password);

                await client.SendAsync(message);
                client.Disconnect(true);
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Email Confirmation Message");
            stringBuilder.AppendLine("--------------------------");
            stringBuilder.AppendLine($"TO: {recipientEmail}");
            stringBuilder.AppendLine($"SUBJECT: {subject}");
            stringBuilder.AppendLine($"CONTENTS: {body}");
            stringBuilder.AppendLine();
            Logger.Log(LogLevel.Information, stringBuilder.ToString());
        }
    }
}