using Recipes.DAL.Interfaces.ModelsRepositories;
using Recipes.Data.Models;

namespace Recipes.DAL.Repositories.ModelsRepositories;

public class CookingStepRepository : GenericRepository<CookingStep>, ICookingStepRepository
{
    public CookingStepRepository(RecipesContext databaseContext) : base(databaseContext)
    {
    }
}