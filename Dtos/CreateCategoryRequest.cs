namespace TravelExpenses.Api.Dtos;

public class CreateCategoryRequest
{
    public string Name { get; set; } = default!;
    public string? Icon { get; set; }
    public string? ColorHex { get; set; }
    public int SortOrder { get; set; } = 0;
}
