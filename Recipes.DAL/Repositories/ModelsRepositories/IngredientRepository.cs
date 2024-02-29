using Recipes.DAL.Interfaces.ModelsRepositories;
using Recipes.Data.Models;

namespace Recipes.DAL.Repositories.ModelsRepositories;

public class IngredientRepository : GenericRepository<Ingredient>, IIngredientRepository
{
    protected IngredientRepository(RecipesContext databaseContext) : base(databaseContext)
    {
    }
}