namespace EasySystems.Application.Dtos;

public class CreateStoreRequestDto
{
    public string StoreName { get; set; } = string.Empty;

    public string BusinessType { get; set; } = string.Empty;

    public string PackageName { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public List<QuestionAnswerDto> Answers { get; set; } = new();
}