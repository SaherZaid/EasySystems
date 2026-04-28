namespace EasySystems.Domain.Entities;

public class StoreQuestionAnswer
{
    public int Id { get; set; }

    public int StoreRequestId { get; set; }

    public string Question { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;
}