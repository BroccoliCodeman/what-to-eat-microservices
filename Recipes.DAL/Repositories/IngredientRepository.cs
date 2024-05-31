using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Recipes.DAL.Infrastructure;
using Recipes.DAL.Repositories.Interfaces;
using Recipes.Data.Models;

namespace Recipes.DAL.Repositories;

public class IngredientRepository : GenericRepository<Ingredient>, IIngredientRepository
{
    public IngredientRepository(RecipesContext databaseContext, ILogger<Ingredient> logger) : base(databaseContext, logger)
    {
    }


    public override async Task<List<Ingredient>> GetAsync() =>
    await _table.Include(p=>p.WeightUnit).ToListAsync();
}