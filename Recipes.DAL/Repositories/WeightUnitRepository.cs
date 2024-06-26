using Microsoft.Extensions.Logging;
using Recipes.DAL.Infrastructure;
using Recipes.DAL.Repositories.Interfaces;
using Recipes.Data.Models;

namespace Recipes.DAL.Repositories;

public class WeightUnitRepository : GenericRepository<WeightUnit>, IWeightUnitRepository
{
    public WeightUnitRepository(RecipesContext databaseContext, ILogger<WeightUnit> logger) : base(databaseContext, logger)
    {
    }



    public Task DeleteAsync(int id)
    {
        _table.Where(p => p.Id == id);

        _table.Remove(_table.Where(p => p.Id == id).FirstOrDefault());

        _databaseContext.SaveChanges();

        return Task.CompletedTask;
    }




    public WeightUnit GetById(int id)
    {

        return _table.Where(p => p.Id == id).FirstOrDefault();

    }
         
         
}