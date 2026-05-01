namespace EasySystems.Domain.Entities;

public class ContactLead
{
    public int Id { get; set; }

    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Business { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Message { get; set; } = "";

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAtUtc { get; set; } =
        DateTime.UtcNow;
}