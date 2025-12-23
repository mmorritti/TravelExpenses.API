using TravelExpenses.Api.Services.Interfaces;
using TravelExpenses.Domain.Entities;
using TravelExpenses.Domain.Interfaces;

namespace TravelExpenses.Api.Services;

public class ExpanseService : BaseService<Expense>, IExpanseService
{
    private readonly IBaseRepository<Expense> _repository;

    public ExpanseService(IBaseRepository<Expense> repository)
        : base(repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Expense>> GetByTravelIdAsync(Guid travelId)
    {
        var all = await _repository.GetAllAsync();
        return all.Where(e => e.TravelId == travelId);
    }

    public async Task<IEnumerable<Expense>> GetAllAsync(string userId)
    {
        var all = await _repository.GetAllAsync();
        return all.Where(e => e.UserId == userId);
    }
}
