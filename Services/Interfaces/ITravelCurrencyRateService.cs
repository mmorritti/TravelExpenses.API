using TravelExpenses.Domain.Entities;

namespace TravelExpenses.Api.Services.Interfaces;

public interface ITravelCurrencyRateService : IBaseService<TravelCurrencyRate>
{
    Task<IEnumerable<TravelCurrencyRate>> GetByTravelIdAsync(Guid travelId);
}
