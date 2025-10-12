using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace WebApp.Infrastructure.Email
{
    public enum SmtpSecurity { None, SslOnConnect, StartTls }

    public sealed class SmtpOptions
    {
        public string Host { get; set; } = "";
        public int Port { get; set; } = 25;
        public string From { get; set; } = "";
        public string? User { get; set; }
        public string? Password { get; set; }
        public SmtpSecurity Security { get; set; } = SmtpSecurity.StartTls;
        public bool DisableAuthIfUserEmpty { get; set; } = true;
        public bool AllowInvalidCertInDevelopment { get; set; } = false;
        public int TimeoutSeconds { get; set; } = 20;
    }
    
    public sealed class MailKitEmailSender : IEmailSender
    {
        private readonly SmtpOptions _opt;
        private readonly ILogger<MailKitEmailSender> _log;
        private readonly IWebHostEnvironment _env;

        public MailKitEmailSender(
            IOptions<SmtpOptions> opt,
            ILogger<MailKitEmailSender> log,
            IWebHostEnvironment env)
        {
            _opt = opt.Value;
            _log = log;
            _env = env;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (string.IsNullOrWhiteSpace(_opt.Host))
                throw new InvalidOperationException("SMTP Host is not configured.");

            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress("BookHub", _opt.From));
            msg.To.Add(MailboxAddress.Parse(email));
            msg.Subject = subject;

            var body = new BodyBuilder
            {
                HtmlBody = htmlMessage,
                TextBody = StripHtml(htmlMessage)
            };
            msg.Body = body.ToMessageBody();

            var secure = _opt.Security switch
            {
                SmtpSecurity.SslOnConnect => SecureSocketOptions.SslOnConnect,
                SmtpSecurity.StartTls     => SecureSocketOptions.StartTls,
                _                          => SecureSocketOptions.None
            };

            using var client = new SmtpClient
            {
                Timeout = _opt.TimeoutSeconds * 1000
            };
            
            client.LocalDomain = Environment.MachineName;
            
            if (_env.EnvironmentName == "Development" && _opt.AllowInvalidCertInDevelopment)
                client.ServerCertificateValidationCallback = (_, _, _, _) => true;

            await client.ConnectAsync(_opt.Host, _opt.Port, secure);

            var serverSupportsAuth = client.Capabilities.HasFlag(MailKit.Net.Smtp.SmtpCapabilities.Authentication);
            var wantAuth = !string.IsNullOrWhiteSpace(_opt.User); 
            
            if (wantAuth && serverSupportsAuth) 
            { 
                await client.AuthenticateAsync(_opt.User!, _opt.Password); 
            }
            else
            { 
                client.AuthenticationMechanisms.Clear(); 
            }

            await client.SendAsync(msg);
            await client.DisconnectAsync(true);

            _log.LogInformation("Email sent to {Email} via {Host}:{Port}", email, _opt.Host, _opt.Port);
        }

        private static string StripHtml(string html) =>
            System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", " ").Trim();
    }
}
