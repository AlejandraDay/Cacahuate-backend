using System.Net;
using System.Net.Mail;
using Cacahuate.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Cacahuate.Services.Implementations;

public class EmailService(IConfiguration config, ILogger<EmailService> logger) : IEmailService
{
    public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        var host = config["Email:Host"];
        var portStr = config["Email:Port"];
        var fromEmail = config["Email:From"];
        var fromName = config["Email:FromName"] ?? "Cacahuate";
        var username = config["Email:Username"];
        var password = config["Email:Password"];

        if (string.IsNullOrEmpty(password))
        {
            logger.LogInformation(
                "[EMAIL MOCK] To: {Email} ({Name}) | Subject: {Subject}\n{Body}",
                toEmail, toName, subject, htmlBody);
            return;
        }

        try
        {
            var port = int.TryParse(portStr, out var p) ? p : 587;

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username ?? fromEmail, password),
                EnableSsl = true,
            };

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true,
            };
            message.To.Add(new MailAddress(toEmail, toName));

            await client.SendMailAsync(message);
            logger.LogInformation("Email sent to {Email}: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {Email}", toEmail);
        }
    }
}
