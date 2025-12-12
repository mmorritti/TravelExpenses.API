namespace TravelExpenses.Api.Dtos;

public class UpdateTravelCurrencyRateRequest
{
    public string CurrencyCode { get; set; } = default!;
    public decimal RateToBase { get; set; }
}