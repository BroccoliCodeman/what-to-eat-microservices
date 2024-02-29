using Recipes.DAL.Interfaces.ModelsRepositories;
using Recipes.Data.Models;

namespace Recipes.DAL.Repositories.ModelsRepositories;

public class SavedRecipeRepository : GenericRepository<SavedRecipe>, ISavedRecipeRepository
{
    public SavedRecipeRepository(RecipesContext databaseContext) : base(databaseContext)
    {
    }
}