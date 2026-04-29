using EasySystems.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EasySystems.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactController : ControllerBase
{
    private readonly EmailService _emailService;

    public ContactController(EmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost]
    public async Task<IActionResult> Send(ContactRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Subject) ||
            string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest("Missing required fields.");
        }

        var html = $@"
        <h2>New Contact Message</h2>

        <p><strong>Name:</strong> {request.Name}</p>
        <p><strong>Email:</strong> {request.Email}</p>
        <p><strong>Phone:</strong> {request.Phone}</p>
        <p><strong>Business:</strong> {request.Business}</p>
        <p><strong>Subject:</strong> {request.Subject}</p>

        <hr/>

        <p>{request.Message}</p>";

        await _emailService.SendCustomEmail(
            "rentconnectab@gmail.com",
            "New Contact Message",
            html);

        return Ok(new
        {
            message = "Message sent successfully."
        });
    }

    public class ContactRequest
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Business { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Message { get; set; } = "";
    }
}