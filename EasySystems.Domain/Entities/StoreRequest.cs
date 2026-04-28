namespace EasySystems.Domain.Entities;

public class StoreRequest
{
    public int Id { get; set; }

    public int UserAccountId { get; set; }

    public string StoreName { get; set; } = string.Empty;

    public string BusinessType { get; set; } = string.Empty;

    public string PackageName { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public string Status { get; set; } = "Pending";
}