using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Recipes.DAL.Infrastructure;
using Recipes.DAL.Repositories.Interfaces;
using Recipes.Data.Helpers;
using Recipes.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace Recipes.DAL.Repositories;

public class RecipeRepository : GenericRepository<Recipe>, IRecipeRepository
{
    public RecipeRepository(RecipesContext databaseContext, ILogger<Recipe> logger) : base(databaseContext, logger)
    {
    }

    public async Task<PagedList<Recipe>> GetAsync(PaginationParams? paginationParams, SearchParams? searchParams)
    {
        var recipes = await _table.Include(p => p.Ingredients).Include(p => p.Users).Include(p => p.User).ToListAsync();
       
        if (searchParams != null)
        {
            if (!string.IsNullOrEmpty(searchParams.Title))
            {
                recipes = recipes.Where(dto => dto.Title.Contains(searchParams.Title, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (searchParams.Ingredients != null && searchParams.Ingredients.Any())
            {
                recipes = recipes.Where(dto => searchParams.Ingredients.All(ingredient =>
                    dto.Ingredients.Any(dtoIngredient => dtoIngredient.Name.Contains(ingredient, StringComparison.OrdinalIgnoreCase))
                )).ToList();
            }
        }

        var totalCount = recipes.Count;

        recipes = recipes
            .Skip((paginationParams!.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
        .ToList();

        return new PagedList<Recipe>(recipes!, (int)totalCount, paginationParams.PageNumber, paginationParams.PageSize);
    }

    public async Task<List<Recipe>> GetByTitle(string title)
    {
        return await _table
            .Where(r => r.Title.ToLower().Contains(title.ToLower()))
            .Take(3)
            .ToListAsync();
    }

    public override async Task<Recipe> GetByIdAsync(Guid id)
    {
        var recipes = await _table.
                        Include(p => p.Ingredients)!
                       .ThenInclude(x=>x.WeightUnit)
                       .Include(p => p.CookingSteps)

                       .ToListAsync();

        var recipe = recipes.Where(x => x.Id == id).FirstOrDefault();

        return recipe!;
    }
    public async Task SaveRecipe(Guid UserId, Guid RecipeId)
    {
        // Завантаження користувача з бази даних, включаючи збережені рецепти
        var user = await _databaseContext.Users
           .Include(u => u.SavedRecipes)
           .FirstOrDefaultAsync(u => u.Id == UserId);

        if (user == null)
        {
            throw new Exception("User not found.");
        }


        // Завантаження рецепта з бази даних
        var recipeToSave = await _databaseContext.Recipes
            .FirstOrDefaultAsync(r => r.Id == RecipeId);

        if (recipeToSave == null)
        {
            throw new Exception("Recipe not found.");
        }

        // Додавання рецепта до списку збережених рецептів користувача
        user.SavedRecipes.Add(recipeToSave);

        // Збереження змін у базі даних
        await _databaseContext.SaveChangesAsync();
    }

    public async Task RemoveRecipeFromSaved(Guid UserId, Guid RecipeId)
    {
    // Завантаження користувача з бази даних, включаючи збережені рецепти
        var user = await _databaseContext.Users
            .Include(u => u.SavedRecipes)
            .FirstOrDefaultAsync(u => u.Id == UserId);

        if (user == null)
        {
            throw new Exception("User not found.");
        }

        // Завантаження рецепта з бази даних
        var recipeToSave = await _databaseContext.Recipes
            .FirstOrDefaultAsync(r => r.Id == RecipeId);

        if (recipeToSave == null)
        {
            throw new Exception("Recipe not found.");
        }

        // Додавання рецепта до списку збережених рецептів користувача
        user.SavedRecipes.Remove(recipeToSave);

        // Збереження змін у базі даних
        await _databaseContext.SaveChangesAsync();    }

}