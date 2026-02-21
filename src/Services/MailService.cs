using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using MimeKit.Text;
using Scriban;

namespace KidFit.Services
{
    public class MailServiceOptions
    {
        public string HostEmail { get; set; } = "";
        public string HostAppPassword { get; set; } = "";
        public string SmtpHost { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public string BaseURL { get; set; } = "";
    }

    public class WelcomeEmailParam
    {
        public string Name { get; set; } = "";
        public string Username { get; set; } = "";
        public string ResetPasswordUrl { get; set; } = "";
    }

    public class MailService(IOptions<MailServiceOptions> options)
    {
        private readonly string _hostEmail = options.Value.HostEmail;
        private readonly string _hostAppPassword = options.Value.HostAppPassword;
        private readonly string _smtpHost = options.Value.SmtpHost;
        private readonly int _smtpPort = options.Value.SmtpPort;
        private readonly string _baseURL = options.Value.BaseURL;

        public string GenerateResetPasswordUrl(string id, string token)
        {
            token = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(token));
            return $"{_baseURL}/auth/resetPassword?id={id}&token={token}";
        }

        public string PrepareWelcomeEmailTemplate(WelcomeEmailParam param)
        {
            // Read template from file
            var source = File.ReadAllText(Directory.GetCurrentDirectory() + "/wwwroot/templates/welcome_email.html");
            var template = Template.ParseLiquid(source);
            return template.Render(param);
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
            client.Connect(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
            client.Authenticate(_hostEmail, _hostAppPassword);
            client.Send(message);
            client.Disconnect(true);
        }
    }
}
