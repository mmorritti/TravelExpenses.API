namespace TravelExpenses.Api.Dtos;

public class CreateTravelRequest
{
    public string Name { get; set; } = default!;
    public string? CountryCode { get; set; }

    // es. "EUR"
    public string HomeCurrencyCode { get; set; } = "EUR";

    // es. "MAD"
    public string TravelCurrencyCode { get; set; } = default!;

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}