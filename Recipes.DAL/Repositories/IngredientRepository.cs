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
    await _table.Include(p=>p.WeightUnit).Include(p=>p.Recipes).ToListAsync();

    // duplicate ingredients fixing support
    public async Task<List<Ingredient>> GetByName(string name)
    {
        var ingredients = await _table
            .Where(r => r.Name.ToLower().Contains(name.ToLower()))
            .ToListAsync();

        var groupedIngredients = ingredients
            .GroupBy(r => String.Join(" ", r.Name.ToLower().Split(' ').OrderBy(s => s)))
            .Select(g => g.First())
            .Take(3)
            .ToList();

        return groupedIngredients;
    }
}