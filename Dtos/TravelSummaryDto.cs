using TravelExpenses.Api.Dtos;

public class TravelSummaryDto
{
    public Guid TravelId { get; set; }
    public string Name { get; set; } = default!;
    public string? CountryCode { get; set; }

    public string HomeCurrencyCode { get; set; } = default!;
    public string TravelCurrencyCode { get; set; } = default!;

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public decimal TotalInTravelCurrency { get; set; }
    public decimal TotalInHomeCurrency { get; set; }

    public decimal ExchangeRateTravelToHome { get; set; }

    public List<CategorySummaryDto> Categories { get; set; } = new();
}
