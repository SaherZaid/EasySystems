using EasySystems.Api.Services;
using EasySystems.Domain.Entities;
using EasySystems.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasySystems.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactController : ControllerBase
{
    private readonly EmailService _emailService;
    private readonly AppDbContext _db;

    public ContactController(
        EmailService emailService,
        AppDbContext db)
    {
        _emailService = emailService;
        _db = db;
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

        var lead = new ContactLead
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Business = request.Business,
            Subject = request.Subject,
            Message = request.Message,
            IsRead = false
        };

        _db.ContactLeads.Add(lead);
        await _db.SaveChangesAsync();

        var html = $@"
        <div style='font-family:Arial;padding:20px'>
            <h2 style='margin-bottom:20px;'>🔥 New Contact Lead</h2>

            <p><strong>Name:</strong> {request.Name}</p>
            <p><strong>Email:</strong> {request.Email}</p>
            <p><strong>Phone:</strong> {request.Phone}</p>
            <p><strong>Business:</strong> {request.Business}</p>
            <p><strong>Subject:</strong> {request.Subject}</p>

            <hr style='margin:20px 0;' />

            <p>{request.Message}</p>
        </div>";

        await _emailService.SendCustomEmail(
            "rentconnectab@gmail.com",
            "🔥 New Contact Lead",
            html);

        return Ok(new
        {
            message = "Message sent successfully."
        });
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpGet("admin")]
    public async Task<IActionResult> GetAll()
    {
        var leads = await _db.ContactLeads
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        return Ok(leads);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPut("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var lead = await _db.ContactLeads
            .FirstOrDefaultAsync(x => x.Id == id);

        if (lead is null)
            return NotFound();

        lead.IsRead = true;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Lead marked as read."
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