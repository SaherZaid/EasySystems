namespace EasySystems.Domain.Entities;

public class StoreRequest
{
    public int Id { get; set; }

    public int UserAccountId { get; set; }

    public string StoreName { get; set; } = "";
    public string BusinessType { get; set; } = "";
    public string PackageName { get; set; } = "";
    public string Notes { get; set; } = "";

    public string Status { get; set; } = "Pending";

    public string Priority { get; set; } = "Medium";

    public string AssignedTo { get; set; } = "";

    public string InternalNote { get; set; } = "";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}