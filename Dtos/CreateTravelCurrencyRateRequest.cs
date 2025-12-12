namespace TravelExpenses.Api.Dtos;

public class CreateTravelCurrencyRateRequest
{
    public string CurrencyCode { get; set; } = default!;
    public decimal RateToBase { get; set; }
}