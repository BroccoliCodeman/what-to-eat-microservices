using Recipes.DAL.Infrastructure.Interfaces;
using Recipes.Data.Helpers;
using Recipes.Data.Models;

namespace Recipes.DAL.Repositories.Interfaces;

public interface IRecipeRepository : IGenericRepository<Recipe>
{
    Task<PagedList<Recipe>> GetAsync(PaginationParams? paginationParams);
    Task<List<Recipe>> GetByTitle(string title);
    Task RemoveRecipeFromSaved(Guid UserId, Guid RecipeId);
    Task SaveRecipe(Guid UserId, Guid RecipeId);
}