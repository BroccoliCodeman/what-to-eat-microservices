using Recipes.Data.Models;

namespace Recipes.DAL.Interfaces.ModelsRepositories;

public interface IWeightUnitRepository : IGenericRepository<WeightUnit>
{
    Task DeleteAsync(int id);
    WeightUnit GetById(int id);
}