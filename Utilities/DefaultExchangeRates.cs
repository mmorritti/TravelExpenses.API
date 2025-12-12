namespace TravelExpenses.Api.Utilities;

public static class DefaultExchangeRates
{
    public static readonly Dictionary<string, decimal> ToEur = new()
    {
        { "EUR", 1.0m },
        { "MAD", 0.091m },
        { "USD", 0.95m },
        { "GBP", 1.15m },
        { "JPY", 0.006m }
    };
}
