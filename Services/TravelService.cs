using TravelExpenses.Api.Services.Interfaces;
using TravelExpenses.Domain.Entities;
using TravelExpenses.Domain.Interfaces;

namespace TravelExpenses.Api.Services;

public class TravelService : BaseService<Travel>, ITravelService
{
    public TravelService(IBaseRepository<Travel> repository)
        : base(repository)
    {
    }
}
