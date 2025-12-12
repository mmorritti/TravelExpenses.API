using TravelExpenses.Domain.Entities;

namespace TravelExpenses.Api.Services.Interfaces;

public interface IExpanseService : IBaseService<Expense>
{
    Task<IEnumerable<Expense>> GetByTravelIdAsync(Guid travelId);
}
