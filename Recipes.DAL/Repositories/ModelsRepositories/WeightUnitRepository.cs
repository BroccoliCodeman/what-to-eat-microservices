using Recipes.DAL.Interfaces.ModelsRepositories;
using Recipes.Data.Models;

namespace Recipes.DAL.Repositories.ModelsRepositories;

public class WeightUnitRepository : GenericRepository<WeightUnit>, IWeightUnitRepository
{
    public WeightUnitRepository(RecipesContext databaseContext) : base(databaseContext)
    {
    }
}