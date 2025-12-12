namespace TravelExpenses.Api.Services.Interfaces;

public interface IBaseService<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);

    Task<IEnumerable<T>> GetAllAsync();

    Task<T> CreateAsync(T entity);

    Task UpdateAsync(T entity);

    Task DeleteAsync(Guid id);
}
