using System.Text.Json;
using System.Text.Json.Serialization;
using TravelExpenses.Api.Services.Interfaces;

namespace TravelExpenses.Api.Services.ExchangeRates;

public class ExchangeRateService : IExchangeRateService
{
    private readonly HttpClient _httpClient;

    // cache in memoria: "USD" -> 0.95m (significa 1 USD = 0.95 EUR)
    private readonly Dictionary<string, decimal> _ratesToEur = new(StringComparer.OrdinalIgnoreCase);
    private DateTime _lastUpdatedUtc = DateTime.MinValue;
    private readonly object _lock = new();

    // fallback statici (per valute non presenti o in caso di errore API)
    private static readonly Dictionary<string, decimal> _fallbackRates = new(StringComparer.OrdinalIgnoreCase)
    {
        { "EUR", 1.0m },
        { "MAD", 0.093m }, 
        { "USD", 0.95m },
        { "GBP", 1.15m },
        { "JPY", 0.006m }
    };

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ExchangeRateService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<decimal?> GetRateToEurAsync(string currencyCode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            return null;

        currencyCode = currencyCode.ToUpperInvariant();

        // 1) Se in cache aggiornata oggi → restituisci subito
        lock (_lock)
        {
            if (_ratesToEur.TryGetValue(currencyCode, out var cached) &&
                _lastUpdatedUtc.Date == DateTime.UtcNow.Date)
            {
                return cached;
            }
        }

        // 2) Prova ad aggiornare la cache da API esterna (una volta al giorno)
        await EnsureRatesLoadedAsync(ct);

        lock (_lock)
        {
            if (_ratesToEur.TryGetValue(currencyCode, out var rate))
            {
                return rate;
            }
        }

        // 3) Fallback statico se non lo trovi neanche dopo l’update
        if (_fallbackRates.TryGetValue(currencyCode, out var fallback))
        {
            return fallback;
        }

        // 4) Ultima spiaggia: nessun rate disponibile
        return null;
    }

    private async Task EnsureRatesLoadedAsync(CancellationToken ct)
    {
        lock (_lock)
        {
            if (_lastUpdatedUtc.Date == DateTime.UtcNow.Date && _ratesToEur.Count > 0)
                return; // già aggiornato oggi
        }

        Dictionary<string, decimal> newRates;

        try
        {
            // chiamiamo /latest con base EUR (default di Frankfurter)
            using var response = await _httpClient.GetAsync("latest", ct);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            var data = await JsonSerializer.DeserializeAsync<FrankfurterLatestResponse>(stream, _jsonOptions, ct);

            if (data == null || data.Rates == null || data.Rates.Count == 0)
                throw new Exception("Risposta Frankfurter vuota.");

            newRates = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            // se base è EUR, i rates sono: 1 EUR = rate[CUR] CUR
            if (string.Equals(data.Base, "EUR", StringComparison.OrdinalIgnoreCase))
            {
                newRates["EUR"] = 1m;

                foreach (var kvp in data.Rates)
                {
                    var currency = kvp.Key;
                    var eurToCur = kvp.Value;

                    if (eurToCur <= 0)
                        continue;

                    // noi vogliamo: 1 CUR = X EUR  → X = 1 / eurToCur
                    newRates[currency] = 1m / eurToCur;
                }
            }
            else
            {
                // Caso teorico: base != EUR.
                // Se c'è EUR tra i rates, possiamo ricavare tutti i ToEur.
                if (!data.Rates.TryGetValue("EUR", out var baseToEur) || baseToEur <= 0)
                    throw new Exception("EUR non presente nei rates di Frankfurter.");

                // 1 BASE = baseToEur EUR → 1 BASE = baseToEur EUR
                // quindi: 1 CUR = ? EUR
                // data.Rates[cur] = 1 BASE = r CUR → 1 CUR = (baseToEur / r) EUR
                newRates[data.Base] = baseToEur;

                foreach (var kvp in data.Rates)
                {
                    var currency = kvp.Key;
                    var baseToCur = kvp.Value;

                    if (baseToCur <= 0)
                        continue;

                    // 1 CUR = (baseToEur / baseToCur) EUR
                    newRates[currency] = baseToEur / baseToCur;
                }
            }

            // integriamo anche i fallback mancanti (es. MAD) senza sovrascrivere i valori reali
            foreach (var kvp in _fallbackRates)
            {
                if (!newRates.ContainsKey(kvp.Key))
                {
                    newRates[kvp.Key] = kvp.Value;
                }
            }
        }
        catch
        {
            // se l’API esterna fallisce, usiamo SOLO i fallback
            newRates = new Dictionary<string, decimal>(_fallbackRates, StringComparer.OrdinalIgnoreCase);
        }

        lock (_lock)
        {
            _ratesToEur.Clear();
            foreach (var kvp in newRates)
            {
                _ratesToEur[kvp.Key] = kvp.Value;
            }

            _lastUpdatedUtc = DateTime.UtcNow;
        }
    }

    // DTO per deserializzare la risposta di Frankfurter
    private sealed class FrankfurterLatestResponse
    {
        [JsonPropertyName("base")]
        public string Base { get; set; } = default!;

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("rates")]
        public Dictionary<string, decimal> Rates { get; set; } = new();
    }
}
