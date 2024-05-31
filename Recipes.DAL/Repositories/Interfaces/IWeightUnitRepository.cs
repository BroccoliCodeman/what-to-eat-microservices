using Recipes.DAL.Infrastructure.Interfaces;
using Recipes.Data.Models;

namespace Recipes.DAL.Repositories.Interfaces;

public interface IWeightUnitRepository : IGenericRepository<WeightUnit>
{
    Task DeleteAsync(int id);
    WeightUnit GetById(int id);
}