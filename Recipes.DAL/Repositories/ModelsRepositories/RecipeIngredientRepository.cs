using Recipes.DAL.Interfaces.ModelsRepositories;
using Recipes.Data.Models;

namespace Recipes.DAL.Repositories.ModelsRepositories;

public class RecipeIngredientRepository : GenericRepository<RecipeIngredient>, IRecipeIngredientRepository
{
    protected RecipeIngredientRepository(RecipesContext databaseContext) : base(databaseContext)
    {
    }
}