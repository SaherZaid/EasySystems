namespace EasySystems.Domain.Entities;

public class PackagePlan
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int MaxProducts { get; set; }

    public bool IncludesOnlinePayment { get; set; }

    public bool IncludesMultiLanguage { get; set; }

    public bool IncludesSupport { get; set; }

    public bool IsActive { get; set; } = true;
}