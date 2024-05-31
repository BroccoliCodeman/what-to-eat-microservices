using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Recipes.DAL.Infrastructure;
using Recipes.DAL.Repositories.Interfaces;
using Recipes.Data.Helpers;
using Recipes.Data.Models;

namespace Recipes.DAL.Repositories;

public class RecipeRepository : GenericRepository<Recipe>, IRecipeRepository
{
    public RecipeRepository(RecipesContext databaseContext, ILogger<Recipe> logger) : base(databaseContext, logger)
    {
    }

    public async Task<PagedList<Recipe>> GetAsync(PaginationParams? paginationParams)
    {
        var recipes = await _table.Include(p => p.Ingredients).Include(p=>p.SavedRecipes).ToListAsync();

        var totalCount = recipes.Count;

        recipes = recipes
            .Skip((paginationParams!.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
        .ToList();

        return new PagedList<Recipe>(recipes!, (int)totalCount, paginationParams.PageNumber, paginationParams.PageSize);
    }
    public override async Task<Recipe> GetByIdAsync(Guid id)
    {
        var recipes = await _table.Include(p => p.Ingredients).ThenInclude(x=>x.WeightUnit).Include(p => p.SavedRecipes).ToListAsync();

        var recipe = recipes.Where(x => x.Id == id).FirstOrDefault();

        return recipe!;
    }
}