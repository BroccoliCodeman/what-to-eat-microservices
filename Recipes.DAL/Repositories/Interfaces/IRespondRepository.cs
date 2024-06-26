using Recipes.DAL.Infrastructure.Interfaces;
using Recipes.Data.Models;

namespace Recipes.DAL.Repositories.Interfaces;

public interface IRespondRepository : IGenericRepository<Respond>
{
    Task<List<Respond>> GetAsync();
}