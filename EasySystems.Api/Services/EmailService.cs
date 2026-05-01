using EasySystems.Domain.Entities;
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
    public async Task SendAdminLeadEmail(ContactLead lead)
    {
        var html = BuildEmailHtml(
            title: "New Contact Lead",
            subtitle: "CRM Lead Notification",
            body: $@"
            <p>A new contact lead has been submitted from the website.</p>

            <div class='info-box'>
                <p><strong>Name:</strong> {lead.Name}</p>
                <p><strong>Email:</strong> {lead.Email}</p>
                <p><strong>Phone:</strong> {lead.Phone}</p>
                <p><strong>Business:</strong> {lead.Business}</p>
                <p><strong>Subject:</strong> {lead.Subject}</p>
                <p><strong>Message:</strong> {lead.Message}</p>
            </div>"
        );

        await SendEmail(
            "rentconnectab@gmail.com",
            "New EasySystems contact lead",
            html);
    }
    public async Task SendCustomEmail(string toEmail, string subject, string htmlBody)
    {
        var html = BuildEmailHtml(
            title: subject,
            subtitle: "New Contact Message",
            body: htmlBody);

        await SendEmail(toEmail, subject, html);
    }

    public async Task SendVerificationCode(string toEmail, string code)
    {
        var html = BuildEmailHtml(
            title: "Your Verification Code",
            subtitle: "Secure Account Access",
            body: $@"
                <p>Use the code below to complete your login securely.</p>
                <div class='code-box'>{code}</div>
                <p>If you did not request this code, you can safely ignore this email.</p>"
        );

        await SendEmail(toEmail, "Your EasySystems Verification Code", html);
    }

    public async Task SendProjectRequestConfirmationToClient(
        string toEmail,
        string clientName,
        string storeName,
        string packageName,
        string status)
    {
        var html = BuildEmailHtml(
            title: "We received your request",
            subtitle: "Project Request Confirmation",
            body: $@"
                <p>Hello {clientName},</p>
                <p>Thank you for submitting your online store request. We received it successfully and our team will review it soon.</p>

                <div class='info-box'>
                    <p><strong>Project:</strong> {storeName}</p>
                    <p><strong>Package:</strong> {packageName}</p>
                    <p><strong>Status:</strong> {status}</p>
                </div>

                <p>We will contact you with the next step.</p>"
        );

        await SendEmail(toEmail, "We received your EasySystems request", html);
    }

    public async Task SendNewProjectRequestToAdmin(
        string adminEmail,
        string clientName,
        string clientEmail,
        string clientPhone,
        string storeName,
        string businessType,
        string packageName,
        string notes)
    {
        var html = BuildEmailHtml(
            title: "New project request",
            subtitle: "Admin Notification",
            body: $@"
                <p>A new online store request has been submitted.</p>

                <div class='info-box'>
                    <p><strong>Client:</strong> {clientName}</p>
                    <p><strong>Email:</strong> {clientEmail}</p>
                    <p><strong>Phone:</strong> {clientPhone}</p>
                    <p><strong>Store:</strong> {storeName}</p>
                    <p><strong>Business Type:</strong> {businessType}</p>
                    <p><strong>Package:</strong> {packageName}</p>
                    <p><strong>Notes:</strong> {notes}</p>
                </div>"
        );

        await SendEmail(adminEmail, "New EasySystems project request", html);
    }

    public async Task SendProjectStatusUpdateToClient(
        string toEmail,
        string clientName,
        string storeName,
        string status)
    {
        var html = BuildEmailHtml(
            title: "Project status updated",
            subtitle: "EasySystems Project Update",
            body: $@"
                <p>Hello {clientName},</p>
                <p>Your project status has been updated.</p>

                <div class='info-box'>
                    <p><strong>Project:</strong> {storeName}</p>
                    <p><strong>Current Status:</strong> {status}</p>
                </div>

                <p>You can log in to your dashboard to follow your request.</p>"
        );

        await SendEmail(toEmail, "Your EasySystems project status was updated", html);
    }

    private async Task SendEmail(string toEmail, string subject, string html)
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
            Subject = subject,
            Body = html,
            IsBodyHtml = true
        };

        mail.To.Add(toEmail);

        await client.SendMailAsync(mail);
    }

    private static string BuildEmailHtml(string title, string subtitle, string body)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
<style>
    .code-box {{
        display:inline-block;
        padding:18px 34px;
        font-size:38px;
        font-weight:900;
        letter-spacing:8px;
        border-radius:18px;
        background:#eff6ff;
        color:#2563eb;
        border:2px dashed #93c5fd;
        margin:30px 0;
    }}

    .info-box {{
        background:#f8fafc;
        border:1px solid #e2e8f0;
        border-radius:18px;
        padding:20px;
        margin:24px 0;
        color:#0f172a;
    }}

    .info-box p {{
        margin:8px 0;
    }}
</style>
</head>

<body style='margin:0;padding:0;background:#f8fafc;font-family:Arial,sans-serif;'>

<table width='100%' cellpadding='0' cellspacing='0'>
<tr>
<td align='center' style='padding:40px 20px;'>

<table width='620' cellpadding='0' cellspacing='0'
style='max-width:620px;background:#ffffff;border-radius:22px;
overflow:hidden;box-shadow:0 20px 60px rgba(0,0,0,.08);'>

<tr>
<td style='background:linear-gradient(135deg,#0f172a,#1e3a8a);
padding:40px;text-align:center;color:white;'>

<div style='font-size:34px;font-weight:900;'>EasySystems</div>
<div style='margin-top:10px;font-size:16px;color:#dbeafe;'>
{subtitle}
</div>

</td>
</tr>

<tr>
<td style='padding:45px;'>

<h2 style='margin:0 0 12px;color:#0f172a;font-size:28px;'>
{title}
</h2>

<div style='margin:0;color:#64748b;font-size:16px;line-height:1.8;'>
{body}
</div>

</td>
</tr>

<tr>
<td style='padding:24px 40px;background:#f8fafc;
text-align:center;color:#94a3b8;font-size:13px;'>

© EasySystems • Build your online store with confidence

</td>
</tr>

</table>

</td>
</tr>
</table>

</body>
</html>";
    }
}