using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace NovaStay.Infrastructure.ServicesImple
{
    public class Email : INotifications
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<Email> _logger;

        public string TypeService => "Email";

        public Email(IConfiguration configuration, ILogger<Email> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmail(RequestSendMessage request)
        {
            try
            {
                var smtpHost = _configuration["SMTP:Host"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["SMTP:Port"] ?? "587");
                var smtpUser = _configuration["SMTP:Username"] ?? "";
                var smtpPass = _configuration["SMTP:Password"] ?? "";

                using (SmtpClient smtp = new SmtpClient(smtpHost, smtpPort))
                {
                    smtp.Credentials = new NetworkCredential(smtpUser, smtpPass);
                    smtp.EnableSsl = true;

                    using (MailMessage message = new MailMessage())
                    {
                        message.From = new MailAddress(smtpUser, "NovaStay");
                        message.To.Add(request.To);
                        message.Subject = request.Subject;
                        message.Body = request.Body;
                        message.IsBodyHtml = true;

                        await smtp.SendMailAsync(message);
                    }
                }

                _logger.LogInformation("Email sent successfully to {To}!", request.To);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {To}", request.To);
                return false;
            }
        }
    }
}
