using Recipes.DAL.Infrastructure.Interfaces;
using Recipes.Data.Helpers;
using Recipes.Data.Models;

namespace Recipes.DAL.Repositories.Interfaces;

public interface IRecipeRepository : IGenericRepository<Recipe>
{
    Task<PagedList<Recipe>> GetAsync(PaginationParams? paginationParams);
}