using TravelExpenses.Api.Services.Interfaces;
using TravelExpenses.Domain.Entities;
using TravelExpenses.Domain.Interfaces;

namespace TravelExpenses.Api.Services;

public class TravelCurrencyRateService : BaseService<TravelCurrencyRate>, ITravelCurrencyRateService
{
    private readonly IBaseRepository<TravelCurrencyRate> _repository;

    public TravelCurrencyRateService(IBaseRepository<TravelCurrencyRate> repository)
        : base(repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<TravelCurrencyRate>> GetByTravelIdAsync(Guid travelId)
    {
        var allTravelCurrencyRate = await _repository.GetAllAsync();
        return allTravelCurrencyRate.Where(travelCurrencyRate => travelCurrencyRate.TravelId == travelId);
    }
}
