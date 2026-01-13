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
    public override async Task UpdateAsync(Recipe entity)
    {
        using var transaction = await _databaseContext.Database.BeginTransactionAsync();

        try
        {
            // 1. Завантажуємо існуючий рецепт з усіма зв'язками
            var existingRecipe = await _table
                .Include(r => r.Ingredients)
                    .ThenInclude(i => i.WeightUnit)
                .Include(r => r.CookingSteps)
                .Include(r => r.Author)
                .FirstOrDefaultAsync(x => x.Id == entity.Id);

            if (existingRecipe == null)
            {
                throw new KeyNotFoundException($"Recipe with ID {entity.Id} not found");
            }

            // 2. Оновлюємо скалярні властивості
            existingRecipe.Title = entity.Title;
            existingRecipe.Description = entity.Description;
            existingRecipe.CookingTime = entity.CookingTime;
            existingRecipe.Servings = entity.Servings;
            existingRecipe.Calories = entity.Calories;
            existingRecipe.Photo = entity.Photo;
            existingRecipe.AuthorId = existingRecipe.AuthorId;

            // 3. Оновлюємо кроки приготування
            UpdateCookingSteps(existingRecipe, entity.CookingSteps);

            // 4. Оновлюємо інгредієнти (many-to-many через проміжну таблицю)
            await UpdateIngredientsAsync(existingRecipe, entity.Ingredients);

            // 5. Зберігаємо зміни
            await _databaseContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private void UpdateCookingSteps(Recipe existingRecipe, ICollection<CookingStep> newSteps)
    {
        // Видаляємо кроки, яких немає в новому списку
        var stepsToRemove = existingRecipe.CookingSteps
            .Where(existing => !newSteps.Any(newStep => newStep.Id == existing.Id))
            .ToList();

        foreach (var step in stepsToRemove)
        {
            existingRecipe.CookingSteps.Remove(step);
        }

        foreach (var newStep in newSteps)
        {
            var existingStep = existingRecipe.CookingSteps
                .FirstOrDefault(s => s.Id == newStep.Id);

            if (existingStep != null)
            {
                // Оновлюємо існуючий крок
                existingStep.Order = newStep.Order;
                existingStep.Description = newStep.Description;
            }
            else
            {
                // Додаємо новий крок
                newStep.RecipeId = existingRecipe.Id;
                existingRecipe.CookingSteps.Add(newStep);
            }
        }
    }

    private async Task UpdateIngredientsAsync(Recipe existingRecipe, ICollection<Ingredient> newIngredients)
    {
        if (!newIngredients.Any())
        {
            existingRecipe.Ingredients.Clear();
            return;
        }

        // 1. Завантажуємо необхідні WeightUnits одним запитом
        var requiredUnitTypes = newIngredients
            .Where(i => i.WeightUnit != null)
            .Select(i => i.WeightUnit!.Type.ToLower())
            .Distinct()
            .ToList();

        var weightUnitsList = await _databaseContext.WeightUnits
            .Where(u => requiredUnitTypes.Contains(u.Type.ToLower()))
            .ToListAsync();

        // Створюємо словник з захистом від дублікатів
        var weightUnits = weightUnitsList
            .GroupBy(w => w.Type.ToLower())
            .ToDictionary(g => g.Key, g => g.First());

        // 2. Готуємо інгредієнти та їх WeightUnits
        var ingredientSearchCriteria = new List<(string Name, float Quantity, int? WeightUnitId)>();

        foreach (var newIngredient in newIngredients)
        {
            WeightUnit? weightUnit = null;

            if (newIngredient.WeightUnit != null)
            {
                var unitTypeKey = newIngredient.WeightUnit.Type.ToLower();

                if (weightUnits.TryGetValue(unitTypeKey, out var existingUnit))
                {
                    weightUnit = existingUnit;
                }
                else
                {
                    // Створюємо нову одиницю виміру
                    weightUnit = new WeightUnit { Type = newIngredient.WeightUnit.Type };
                    _databaseContext.WeightUnits.Add(weightUnit);
                    await _databaseContext.SaveChangesAsync(); // Зберігаємо, щоб отримати ID
                    weightUnits[unitTypeKey] = weightUnit;
                }
            }

            ingredientSearchCriteria.Add((newIngredient.Name, newIngredient.Quantity, weightUnit?.Id));
        }

        // 3. Завантажуємо всі потенційно існуючі інгредієнти одним запитом
        var names = ingredientSearchCriteria.Select(c => c.Name).Distinct().ToList();

        var existingIngredientsList = await _databaseContext.Set<Ingredient>()
            .Include(i => i.WeightUnit)
            .Where(i => names.Contains(i.Name))
            .ToListAsync();

        // Створюємо словник для швидкого пошуку з захистом від дублікатів
        var existingIngredientsDict = existingIngredientsList
            .GroupBy(i => $"{i.Name}_{i.Quantity}_{i.WeightUnitId}")
            .ToDictionary(g => g.Key, g => g.First());

        // 4. Створюємо список нових зв'язків
        var newIngredientLinks = new List<Ingredient>();

        for (int i = 0; i < ingredientSearchCriteria.Count; i++)
        {
            var (name, quantity, weightUnitId) = ingredientSearchCriteria[i];

            var key = $"{name}_{quantity}_{weightUnitId}";

            if (existingIngredientsDict.TryGetValue(key, out var existingIngredient))
            {
                // Використовуємо існуючий інгредієнт
                newIngredientLinks.Add(existingIngredient);
            }
            else
            {
                // Перевіряємо, чи ми вже створили такий інгредієнт в поточному циклі
                if (!existingIngredientsDict.ContainsKey(key))
                {
                    // Створюємо новий інгредієнт
                    var ingredient = new Ingredient
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Quantity = quantity,
                        WeightUnitId = weightUnitId
                    };

                    _databaseContext.Set<Ingredient>().Add(ingredient);
                    existingIngredientsDict[key] = ingredient;
                    newIngredientLinks.Add(ingredient);
                }
                else
                {
                    // Використовуємо щойно створений інгредієнт
                    newIngredientLinks.Add(existingIngredientsDict[key]);
                }
            }
        }

        // 5. Оновлюємо зв'язки: видаляємо старі, додаємо нові
        existingRecipe.Ingredients.Clear();

        foreach (var ingredient in newIngredientLinks)
        {
            existingRecipe.Ingredients.Add(ingredient);
        }
    }
    public async Task<List<Recipe>> GetByUserIdAsync(Guid userId)
    {
        var res = await _table.Include(p => p.Ingredients)
            .Include(p => p.Author).Where(p=>p.SavedByUsers.Any(p=>p.Id == userId))
            .ToListAsync();
        return res;
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
            .AsSplitQuery()
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