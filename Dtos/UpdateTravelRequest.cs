namespace TravelExpenses.Api.Dtos;


public class UpdateTravelRequest
{
    public string Name { get; set; } = default!;
    public string? CountryCode { get; set; }
    public string HomeCurrencyCode { get; set; } = default!;
    public string TravelCurrencyCode { get; set; } = default!;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
