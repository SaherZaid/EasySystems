using System.Net;
using System.Net.Mail;

namespace EasySystems.Api.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendVerificationCode(string toEmail, string code)
    {
        var host = _configuration["EmailSettings:Host"];
        var port = int.Parse(_configuration["EmailSettings:Port"]!);
        var fromEmail = _configuration["EmailSettings:Email"];
        var password = _configuration["EmailSettings:Password"];
        var displayName = _configuration["EmailSettings:DisplayName"];

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(fromEmail, password),
            EnableSsl = true
        };

        var mail = new MailMessage
        {
            From = new MailAddress(fromEmail!, displayName),
            Subject = "Your EasySystems Verification Code",
            Body = $@"
Hello,

Your verification code is:

{code}

This code expires soon.

EasySystems Team
",
            IsBodyHtml = false
        };

        mail.To.Add(toEmail);

        await client.SendMailAsync(mail);
    }
}