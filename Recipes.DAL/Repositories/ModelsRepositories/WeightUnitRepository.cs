using Recipes.DAL.Interfaces.ModelsRepositories;
using Recipes.Data.Models;

namespace Recipes.DAL.Repositories.ModelsRepositories;

public class WeightUnitRepository : GenericRepository<WeightUnit>, IWeightUnitRepository
{
    public WeightUnitRepository(RecipesContext databaseContext) : base(databaseContext)
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