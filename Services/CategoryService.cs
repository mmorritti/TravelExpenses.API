using TravelExpenses.Api.Services.Interfaces;
using TravelExpenses.Domain.Entities;
using TravelExpenses.Domain.Interfaces;

namespace TravelExpenses.Api.Services;

public class CategoryService : BaseService<Category>, ICategoryService
{
    private readonly IBaseRepository<Category> _repository;

    public CategoryService(IBaseRepository<Category> repository)
        : base(repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Category>> GetAllAsync(string userId)
    {
        var allCategories = await _repository.GetAllAsync();

        // Filtra: Categorie dell'utente OPPURE Categorie globali (UserId è null)
        return allCategories.Where(c => c.UserId == userId || c.UserId == null);
    }
}