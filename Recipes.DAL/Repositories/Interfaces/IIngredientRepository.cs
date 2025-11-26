using Recipes.DAL.Infrastructure.Interfaces;
using Recipes.Data.Models;

namespace Recipes.DAL.Repositories.Interfaces;

public interface IIngredientRepository : IGenericRepository<Ingredient>
{
    Task<List<Ingredient>> GetByName(string name);
    Task<List<Ingredient>> MapMultiple(IEnumerable<string> modelNames);
}