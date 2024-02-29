using Recipes.DAL.Interfaces.ModelsRepositories;
using Recipes.Data.Models;

namespace Recipes.DAL.Repositories.ModelsRepositories;

public class CookingStepRepository : GenericRepository<CookingStep>, ICookingStepRepository
{
    protected CookingStepRepository(RecipesContext databaseContext) : base(databaseContext)
    {
    }
}