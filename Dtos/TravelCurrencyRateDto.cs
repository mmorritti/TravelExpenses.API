namespace TravelExpenses.Api.Dtos;

public class TravelCurrencyRateDto
{
    public Guid TravelCurrencyRateId { get; set; }
    public Guid TravelId { get; set; }
    public string CurrencyCode { get; set; } = default!;
    public decimal RateToBase { get; set; }
}




