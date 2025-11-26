using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Recipes.DAL.Infrastructure;
using Recipes.DAL.Repositories.Interfaces;
using Recipes.Data.Models;
using FuzzySharp;

namespace Recipes.DAL.Repositories;

public class IngredientRepository : GenericRepository<Ingredient>, IIngredientRepository
{
    public IngredientRepository(RecipesContext databaseContext, ILogger<Ingredient> logger) : base(databaseContext, logger)
    {
    }


    public override async Task<List<Ingredient>> GetAsync() =>
    await _table.Include(p=>p.WeightUnit).ToListAsync();

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

    private List<Ingredient> _dbIngredients;

    // Load all ingredients once (optional caching)
    public async Task LoadIngredientsAsync()
    {
        _dbIngredients = await _databaseContext.Ingredients.ToListAsync();
    }

    public Ingredient? MapIngredient(string modelName)
    {
        if (_dbIngredients == null || !_dbIngredients.Any())
            return null;

        var bestMatch = Process.ExtractOne(
            modelName,
            _dbIngredients.Select(i => i.Name)
        );

        // Similarity threshold (tweak if needed)
        if (bestMatch.Score < 60)
            return null;

        return _dbIngredients.FirstOrDefault(i => i.Name == bestMatch.Value);
    }

    public async Task<List<Ingredient>> MapMultiple(IEnumerable<string> modelNames)
    {
        await LoadIngredientsAsync();

        var result = new List<Ingredient>();

        foreach (var name in modelNames)
        {
            var match = MapIngredient(name);
            if (match != null)
                result.Add(match);
        }

        return result;
    }
}