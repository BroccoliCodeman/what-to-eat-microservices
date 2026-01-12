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

    public async override Task<List<Recipe>> GetAsync()
    {
        return await _table.Include(p => p.Ingredients).Include(p => p.Responds).Include(p => p.CookingSteps).Include(p => p.SavedByUsers).Include(p => p.Author).ToListAsync();
    }

    public async Task<PagedList<Recipe>> GetAsync(PaginationParams? paginationParams, SearchParams? searchParams)
    {
        IQueryable<Recipe> query = _table
            .Include(p => p.Ingredients)
            .Include(p => p.SavedByUsers)
            .Include(p => p.Author)
            .AsNoTracking(); // ✅ Read-only оптимізація

        // ✅ Фільтрація в SQL
        if (searchParams != null)
        {
            if (!string.IsNullOrEmpty(searchParams.Title))
                query = query.Where(r => r.Title.Contains(searchParams.Title));

            if (searchParams.Ingredients != null && searchParams.Ingredients.Any())
            {
                foreach (var ingredient in searchParams.Ingredients)
                {
                    query = query.Where(r => r.Ingredients.Any(i => i.Name.Contains(ingredient)));
                }
            }
        }

        // ✅ Підрахунок в SQL
        var totalCount = await query.CountAsync();

        // ✅ Пагінація в SQL
        var recipes = await query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return new PagedList<Recipe>(recipes, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
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
        return await _table
            .Include(p => p.Ingredients)
                .ThenInclude(x => x.WeightUnit)
            .Include(p => p.CookingSteps)
            .Include(p => p.Author)
            .Include(p => p.Responds)
                .ThenInclude(x => x.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id); // ✅ WHERE в SQL
    }
    public async Task SaveRecipe(Guid UserId, Guid RecipeId)
    {
        // ������������ ����������� � ���� �����, ��������� �������� �������
        var user = await _databaseContext.Users
           .Include(u => u.SavedRecipes)
           .FirstOrDefaultAsync(u => u.Id == UserId);

        if (user == null)
        {
            throw new Exception("User not found.");
        }


        // ������������ ������� � ���� �����
        var recipeToSave = await _databaseContext.Recipes
            .FirstOrDefaultAsync(r => r.Id == RecipeId);

        if (recipeToSave == null)
        {
            throw new Exception("Recipe not found.");
        }

        // ��������� ������� �� ������ ���������� ������� �����������
        user.SavedRecipes.Add(recipeToSave);

        // ���������� ��� � ��� �����
        await _databaseContext.SaveChangesAsync();
    }

    public async Task RemoveRecipeFromSaved(Guid UserId, Guid RecipeId)
    {
    // ������������ ����������� � ���� �����, ��������� �������� �������
        var user = await _databaseContext.Users
            .Include(u => u.SavedRecipes)
            .FirstOrDefaultAsync(u => u.Id == UserId);

        if (user == null)
        {
            throw new Exception("User not found.");
        }

        // ������������ ������� � ���� �����
        var recipeToSave = await _databaseContext.Recipes
            .FirstOrDefaultAsync(r => r.Id == RecipeId);

        if (recipeToSave == null)
        {
            throw new Exception("Recipe not found.");
        }

        // ��������� ������� �� ������ ���������� ������� �����������
        user.SavedRecipes.Remove(recipeToSave);

        // ���������� ��� � ��� �����
        await _databaseContext.SaveChangesAsync();    }

}