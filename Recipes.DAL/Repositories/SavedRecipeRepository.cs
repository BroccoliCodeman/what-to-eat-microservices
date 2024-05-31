using Microsoft.Extensions.Logging;
using Recipes.DAL.Infrastructure;
using Recipes.DAL.Repositories.Interfaces;
using Recipes.Data.Models;

namespace Recipes.DAL.Repositories;

public class SavedRecipeRepository : GenericRepository<SavedRecipe>, ISavedRecipeRepository
{
    public SavedRecipeRepository(RecipesContext databaseContext, ILogger<SavedRecipe> logger) : base(databaseContext, logger)
    {
    }
}