using TravelExpenses.Api.Services.Interfaces;

namespace TravelExpenses.Api.Services.ExchangeRates;

public class ExchangeRateService : IExchangeRateService
{
    // Dizionario statico con i tassi di cambio predefiniti (1 Unità Estera = X Euro)
    // I valori sono approssimazioni medie aggiornate per garantire coerenza nei calcoli
    private static readonly Dictionary<string, decimal> _fixedRates = new(StringComparer.OrdinalIgnoreCase)
    {
        // Europa & Americhe
        { "EUR", 1.0m },      // Euro
        { "USD", 0.95m },     // Dollaro Americano
        { "GBP", 1.20m },     // Sterlina Britannica
        { "CHF", 1.06m },     // Franco Svizzero
        { "CAD", 0.68m },     // Dollaro Canadese
        { "BRL", 0.17m },     // Real Brasiliano
        { "MXN", 0.051m },    // Peso Messicano

        // Africa & Medio Oriente
        { "MAD", 0.093m },    // Dirham Marocchino
        { "EGP", 0.020m },    // Sterlina Egiziana
        { "AED", 0.26m },     // Dirham degli Emirati Arabi
        { "ILS", 0.25m },     // Shekel Israeliano
        { "ZAR", 0.052m },    // Rand Sudafricano
        { "TRY", 0.030m },    // Lira Turca

        // Asia & Oceania
        { "JPY", 0.0062m },   // Yen Giapponese
        { "CNY", 0.13m },     // Yuan Cinese
        { "INR", 0.011m },    // Rupia Indiana
        { "AUD", 0.62m },     // Dollaro Australiano
        { "NZD", 0.58m },     // Dollaro Neozelandese
        { "SGD", 0.70m },     // Dollaro di Singapore
        { "HKD", 0.12m },     // Dollaro di Hong Kong
        { "KRW", 0.0007m },   // Won Sudcoreano
        { "THB", 0.027m },    // Baht Thailandese
        { "IDR", 0.00006m },  // Rupia Indonesiana
        { "VND", 0.000038m }  // Dong Vietnamita
    };

    public ExchangeRateService()
    {
        // Rimosso HttpClient perché non interpelliamo più API esterne
    }

    /// <summary>
    /// Restituisce il tasso di cambio fisso per convertire verso l'Euro.
    /// Esempio: Se chiedi "USD", restituisce 0.95 (1 USD = 0.95 EUR).
    /// </summary>
    public async Task<decimal?> GetRateToEurAsync(string currencyCode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            return null;

        currencyCode = currencyCode.ToUpperInvariant();

        // Cerchiamo direttamente nel dizionario delle valute definite
        if (_fixedRates.TryGetValue(currencyCode, out var rate))
        {
            return await Task.FromResult(rate);
        }

        // Se la valuta non è in elenco, restituiamo null
        return await Task.FromResult<decimal?>(null);
    }
}