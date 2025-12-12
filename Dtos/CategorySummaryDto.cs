namespace TravelExpenses.Api.Dtos;

public class CategorySummaryDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;
    public decimal TotalInTravelCurrency { get; set; }
    public decimal TotalInHomeCurrency { get; set; }
    public decimal PercentageOfTotalHomeCurrency { get; set; }
}


