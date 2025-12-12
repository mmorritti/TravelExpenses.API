using TravelExpenses.Api.Services.Interfaces;
using TravelExpenses.Domain.Entities;
using TravelExpenses.Domain.Interfaces;

namespace TravelExpenses.Api.Services;

public class CategoryService : BaseService<Category>, ICategoryService
{
    public CategoryService(IBaseRepository<Category> repository)
        : base(repository)
    {
    }
}
