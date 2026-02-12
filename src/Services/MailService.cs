using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace KidFit.Services
{
    public class MailServiceOptions
    {
        public string HostEmail { get; set; } = "";
        public string HostAppPassword { get; set; } = "";
    }

    public class MailService(IOptions<MailServiceOptions> options)
    {
        private readonly string _hostEmail = options.Value.HostEmail;
        private readonly string _hostAppPassword = options.Value.HostAppPassword;

        private readonly string SMTP_HOST = "smtp.gmail.com";
        private readonly int SMTP_PORT = 587;

        public static string PrepareWelcomeEmailTemplate(string name, string username, string id, string token)
        {
            var template = new StringBuilder();

            template.AppendLine($"<p>Welcome to KidFit, {name}</p>");
            template.AppendLine($"<p>Your username is {username}</p>");
            template.AppendLine($"<p>Please use the <b>link</b> below to login and change your password</p>");
            template.AppendLine($"<p><a href=\"http://localhost:5046/api/change-password?id={id}&token={token}\">Change Password</a></p>");

            return template.ToString();
        }

        public void SendEmail(IEnumerable<(string Name, string Email)> recipients, string subject, string body)
        {
            // Create message
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("KidFit", _hostEmail));
            foreach (var (name, email) in recipients)
            {
                message.To.Add(new MailboxAddress(name, email));
            }
            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Html) { Text = body };

            // Send message
            using var client = new SmtpClient();
            client.Connect(SMTP_HOST, SMTP_PORT, SecureSocketOptions.StartTls);
            client.Authenticate(_hostEmail, _hostAppPassword);
            client.Send(message);
            client.Disconnect(true);
        }
    }
}
