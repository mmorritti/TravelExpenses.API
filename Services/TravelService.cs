using TravelExpenses.Api.Services.Interfaces;
using TravelExpenses.Domain.Entities;
using TravelExpenses.Domain.Interfaces;

namespace TravelExpenses.Api.Services;

public class TravelService : BaseService<Travel>, ITravelService
{
    private readonly IBaseRepository<Travel> _repository;

    public TravelService(IBaseRepository<Travel> repository)
        : base(repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Travel>> GetAllAsync(string userId)
    {
        var allTravels = await _repository.GetAllAsync();
        return allTravels.Where(t => t.UserId == userId);
    }
}