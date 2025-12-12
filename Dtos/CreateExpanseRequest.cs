namespace TravelExpenses.Api.Dtos;

public class CreateExpanseRequest
{
    public Guid TravelId { get; set; }
    public Guid CategoryId { get; set; }
    public DateTime ExpanseDate { get; set; }
    public string Name { get; set; } = default!;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = default!;
    public string? Description { get; set; }
}
