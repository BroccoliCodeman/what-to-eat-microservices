using Recipes.DAL.Interfaces.ModelsRepositories;
using Recipes.Data.Models;

namespace Recipes.DAL.Repositories.ModelsRepositories;

public class SavedRecipeRepository : GenericRepository<SavedRecipe>, ISavedRecipeRepository
{
    protected SavedRecipeRepository(RecipesContext databaseContext) : base(databaseContext)
    {
    }
}