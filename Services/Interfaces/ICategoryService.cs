using TravelExpenses.Domain.Entities;

namespace TravelExpenses.Api.Services.Interfaces;

public interface ICategoryService : IBaseService<Category>
{
    Task<IEnumerable<Category>> GetAllAsync(string userId);
}
