using Microsoft.EntityFrameworkCore;
using Recipes.DAL.Interfaces.ModelsRepositories;
using Recipes.Data.Models;

namespace Recipes.DAL.Repositories.ModelsRepositories;

public class IngredientRepository : GenericRepository<Ingredient>, IIngredientRepository
{
    public IngredientRepository(RecipesContext databaseContext) : base(databaseContext)
    {
    }


    public override async Task<List<Ingredient>> GetAsync() =>
    await _table.Include(p=>p.WeightUnit).ToListAsync();
}