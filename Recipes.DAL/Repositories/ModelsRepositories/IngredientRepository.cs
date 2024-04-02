using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Recipes.DAL.Interfaces.ModelsRepositories;
using Recipes.Data.Models;

namespace Recipes.DAL.Repositories.ModelsRepositories;

public class IngredientRepository : GenericRepository<Ingredient>, IIngredientRepository
{
    public IngredientRepository(RecipesContext databaseContext, ILogger<Ingredient> logger) : base(databaseContext, logger)
    {
    }


    public override async Task<List<Ingredient>> GetAsync() =>
    await _table.Include(p=>p.WeightUnit).ToListAsync();
}