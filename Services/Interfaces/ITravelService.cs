using TravelExpenses.Domain.Entities;

namespace TravelExpenses.Api.Services.Interfaces;

public interface ITravelService : IBaseService<Travel>
{
    Task<IEnumerable<Travel>> GetAllAsync(string userId);
}
