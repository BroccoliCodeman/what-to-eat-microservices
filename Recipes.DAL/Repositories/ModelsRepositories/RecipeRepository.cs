using Recipes.DAL.Interfaces.ModelsRepositories;
using Recipes.Data.Models;

namespace Recipes.DAL.Repositories.ModelsRepositories;

public class RecipeRepository : GenericRepository<Recipe>, IRecipeRepository
{
    protected RecipeRepository(RecipesContext databaseContext) : base(databaseContext)
    {
    }
}