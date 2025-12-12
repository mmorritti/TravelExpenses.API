namespace TravelExpenses.Api.Services.ExchangeRates;

public interface IExchangeRateService
{
    /// <summary>
    /// Restituisce il tasso da currencyCode -> EUR.
    /// Se non disponibile, può usare un fallback.
    /// </summary>
    Task<decimal?> GetRateToEurAsync(string currencyCode, CancellationToken ct = default);
}
