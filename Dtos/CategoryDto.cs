namespace TravelExpenses.Api.Dtos;

public class CategoryDto
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = default!;
    public string? Icon { get; set; }
    public string? ColorHex { get; set; }
    public int SortOrder { get; set; }
}




