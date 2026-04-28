namespace EasySystems.Domain.Entities;

public class EmailVerificationCode
{
    public int Id { get; set; }

    public int UserAccountId { get; set; }

    public string Code { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public bool IsUsed { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}