using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Recipes.Data.Models;

namespace Recipes.DAL.Seeding
{

    public static class DatabaseSeeding
    {
        public static async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RecipesContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<UserRole>>();

            try
            {
                // 1. Спочатку базові сутності
                await SeedRoles(roleManager);
                await SeedUsers(userManager, roleManager);
                await SeedWeightUnits(context);

                // 2. Інгредієнти (незалежні)
               var ingredients =  await SeedIngredients(context);

                // 3. Рецепти (незалежні)
                await SeedRecipes(context, ingredients);

                // 4. Зв'язки між рецептами та інгредієнтами
               // await SeedIngredientRecipe(context);

                // 5. Інші залежні дані
                await SeedCookingSteps(context);
                await SeedResponds(context);
                await SeedRecipeUser(context);

                Console.WriteLine("Database seeded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding database: {ex.Message}");
                throw;
            }
        }

        private static async Task<List<IngredientDto>>? SeedIngredients(RecipesContext context)
        {
            var json = await File.ReadAllTextAsync("SeedData/Ingredients.json");
            List<IngredientDto>? ingredientsDtos = JsonConvert.DeserializeObject<List<IngredientDto>>(json);
            if (ingredientsDtos == null) return null;
            foreach (var dto in ingredientsDtos)
            {
                dto.OldId = dto.Id;
                dto.Id = Guid.Empty;
            }
            var existingNames = await context.Ingredients.Select(i => i.Name).ToListAsync();
            var existingSet = new HashSet<string>(existingNames);

            foreach (var dto in ingredientsDtos)
            {
                if (!existingSet.Contains(dto.Name))
                {
                    var  item = new Ingredient
                    {
                        Quantity = dto.Quantity,
                        Name = dto.Name,
                        WeightUnitId = dto.WeightUnitId
                    };
                    context.Ingredients.Add(item);
                    dto.Id = item.Id;
                }
            }

            await context.SaveChangesAsync();
            return ingredientsDtos;
        }

        private static async Task SeedRecipes(RecipesContext context, List<IngredientDto> ingredients)
        {
            var json = await File.ReadAllTextAsync("SeedData/Recipes.json");
            var recipesDtos = JsonConvert.DeserializeObject<List<RecipeDto>>(json);
            if (recipesDtos == null) return;
            var jsonL = await File.ReadAllTextAsync("SeedData/IngredientRecipe.json");
            var links = JsonConvert.DeserializeObject<List<RecipeToIngredientDto>>(jsonL);
            if (links == null) return;


            var existingTitles = await context.Recipes.Select(r => r.Title).ToListAsync();
            var existingSet = new HashSet<string>(existingTitles);

            foreach (var dto in recipesDtos)
            {
                var reId = dto.Id;
                var ingredientLinks = links.Where(l => l.RecipesId == reId).ToList();
                var recipeIngredients = ingredients.Where(i => ingredientLinks.Any(l => l.IngredientsId == i.OldId))
                    .ToList();
                var ingredientEntities = context.Ingredients.ToList();
               var ingredintstonsert = ingredientEntities.Where(i => recipeIngredients.Any(ri => ri.Id == i.Id))
                    .ToList();

                if (!existingSet.Contains(dto.Title))
                {
                    context.Recipes.Add(new Recipe
                    {
                        Servings = dto.Servings,
                        CookingTime = dto.CookingTime,
                        Title = dto.Title,
                        Photo = dto.Photo,
                        Description = dto.Description,
                        Calories = dto.Calories,
                        UserId = dto.UserId,
                        CreationDate = dto.CreationDate,
                        Ingredients = ingredintstonsert
                    });
                }
            }
            await context.SaveChangesAsync();
        }

        // АНАЛОГІЧНО ДО SeedCookingSteps
        private static async Task SeedIngredientRecipe(RecipesContext context)
        {
            var json = await File.ReadAllTextAsync("SeedData/IngredientRecipe.json");
            var links = JsonConvert.DeserializeObject<List<RecipeToIngredientDto>>(json);
            if (links == null) return;

            // Читаємо шпаргалки для старих ID
            var recipesJson = await File.ReadAllTextAsync("SeedData/Recipes.json");
            var ingredientsJson = await File.ReadAllTextAsync("SeedData/Ingredients.json");

            var recipeDtos = JsonConvert.DeserializeObject<List<RecipeDto>>(recipesJson);
            var ingredientDtos = JsonConvert.DeserializeObject<List<IngredientDto>>(ingredientsJson);

            // Створюємо мапи: старий ID -> назва/ім'я
            var oldIdToRecipeTitle = recipeDtos.ToDictionary(r => r.Id, r => r.Title);
            var oldIdToIngredientName = ingredientDtos.ToDictionary(i => i.Id, i => i.Name);

            // Завантажуємо реальні об'єкти з бази: назва/ім'я -> новий ID
            var dbRecipes = await context.Recipes.ToDictionaryAsync(r => r.Title, r => r.Id);
            var dbIngredients = await context.Ingredients.ToDictionaryAsync(i => i.Name, i => i.Id);

            // Завантажуємо рецепти з їх інгредієнтами для перевірки існуючих зв'язків
            var recipesWithIngredients = await context.Recipes
                .Include(r => r.Ingredients)
                .ToDictionaryAsync(r => r.Id, r => r);

            foreach (var link in links)
            {
                // Знаходимо назву рецепта та інгредієнта за старими ID
                if (oldIdToRecipeTitle.TryGetValue(link.RecipesId, out var recipeTitle) &&
                    oldIdToIngredientName.TryGetValue(link.IngredientsId, out var ingredientName))
                {
                    // Знаходимо нові ID за назвами
                    if (dbRecipes.TryGetValue(recipeTitle, out var newRecipeId) &&
                        dbIngredients.TryGetValue(ingredientName, out var newIngredientId))
                    {
                        // Перевіряємо чи не існує вже цей зв'язок
                        if (recipesWithIngredients.TryGetValue(newRecipeId, out var recipe))
                        {
                            if (recipe.Ingredients == null)
                                recipe.Ingredients = new List<Ingredient>();

                            if (!recipe.Ingredients.Any(i => i.Id == newIngredientId))
                            {
                                var ingredient = await context.Ingredients.FindAsync(newIngredientId);
                                if (ingredient != null)
                                {
                                    recipe.Ingredients.Add(ingredient);
                                }
                            }
                        }
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedCookingSteps(RecipesContext context)
        {
            var json = await File.ReadAllTextAsync("SeedData/CookingSteps.json");
            var stepDtos = JsonConvert.DeserializeObject<List<CookingStepDto>>(json);
            if (stepDtos == null) return;

            var recipesJson = await File.ReadAllTextAsync("SeedData/Recipes.json");
            var recipeDtos = JsonConvert.DeserializeObject<List<RecipeDto>>(recipesJson);
            var oldIdToTitle = recipeDtos.ToDictionary(r => r.Id, r => r.Title);

            var dbRecipes = await context.Recipes.ToDictionaryAsync(r => r.Title, r => r.Id);

            foreach (var stepDto in stepDtos)
            {
                if (stepDto.RecipeId.HasValue &&
                    oldIdToTitle.TryGetValue(stepDto.RecipeId.Value, out var title) &&
                    dbRecipes.TryGetValue(title, out var newRecipeId))
                {
                    if (!await context.CookingSteps.AnyAsync(s => s.RecipeId == newRecipeId && s.Order == stepDto.Order))
                    {
                        context.CookingSteps.Add(new CookingStep
                        {
                            Description = stepDto.Description,
                            Order = stepDto.Order,
                            RecipeId = newRecipeId
                        });
                    }
                }
            }
            await context.SaveChangesAsync();
        }

        private static async Task SeedResponds(RecipesContext context)
        {
            var json = await File.ReadAllTextAsync("SeedData/Responds.json");
            var dtos = JsonConvert.DeserializeObject<List<RespondDto>>(json);
            if (dtos == null) return;

            var recipesJson = await File.ReadAllTextAsync("SeedData/Recipes.json");
            var recipeDtos = JsonConvert.DeserializeObject<List<RecipeDto>>(recipesJson);
            var oldIdToTitle = recipeDtos.ToDictionary(r => r.Id, r => r.Title);

            var dbRecipes = await context.Recipes.ToDictionaryAsync(r => r.Title, r => r.Id);

            foreach (var dto in dtos)
            {
                if (dto.RecipeId.HasValue &&
                    oldIdToTitle.TryGetValue(dto.RecipeId.Value, out var title) &&
                    dbRecipes.TryGetValue(title, out var newRecipeId))
                {
                    if (!await context.Responds.AnyAsync(r => r.UserId == dto.UserId && r.RecipeId == newRecipeId))
                    {
                        context.Responds.Add(new Respond
                        {
                            Text = dto.Text,
                            Rate = dto.Rate,
                            RecipeId = newRecipeId,
                            UserId = dto.UserId
                        });
                    }
                }
            }
            await context.SaveChangesAsync();
        }

        private static async Task SeedRecipeUser(RecipesContext context)
        {
            var json = await File.ReadAllTextAsync("SeedData/RecipeUser.json");
            var links = JsonConvert.DeserializeObject<List<RecipeUserDto>>(json);
            if (links == null) return;

            var recipesJson = await File.ReadAllTextAsync("SeedData/Recipes.json");
            var recipeDtos = JsonConvert.DeserializeObject<List<RecipeDto>>(recipesJson);
            var oldIdToTitle = recipeDtos.ToDictionary(r => r.Id, r => r.Title);

            var dbRecipes = await context.Recipes.ToDictionaryAsync(r => r.Title, r => r.Id);

            var recipesWithUsers = await context.Recipes
                .Include(r => r.Users)
                .ToDictionaryAsync(r => r.Id, r => r);

            foreach (var link in links)
            {
                if (oldIdToTitle.TryGetValue(link.RecipesId, out var title) &&
                    dbRecipes.TryGetValue(title, out var newRecipeId))
                {
                    if (recipesWithUsers.TryGetValue(newRecipeId, out var recipe))
                    {
                        var user = await context.Users.FindAsync(link.UsersId);
                        if (user != null)
                        {
                            if (recipe.Users == null)
                                recipe.Users = new List<User>();

                            if (!recipe.Users.Any(u => u.Id == user.Id))
                            {
                                recipe.Users.Add(user);
                            }
                        }
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedRoles(RoleManager<UserRole> roleManager)
        {
            var json = await File.ReadAllTextAsync("SeedData/AspNetRoles.json");
            var roles = JsonConvert.DeserializeObject<List<AspNetRoleDto>>(json);
            if (roles == null) return;
            foreach (var r in roles)
            {
                if (!await roleManager.RoleExistsAsync(r.Name))
                    await roleManager.CreateAsync(new UserRole { Id = r.Id, Name = r.Name, NormalizedName = r.NormalizedName });
            }
        }

        private static async Task SeedUsers(UserManager<User> userManager, RoleManager<UserRole> roleManager)
        {
            var json = await File.ReadAllTextAsync("SeedData/AspNetUsers.json");
            var users = JsonConvert.DeserializeObject<List<AspNetUserDto>>(json);
            var userRolesJson = await File.ReadAllTextAsync("SeedData/AspNetUserRoles.json");
            var userRoles = JsonConvert.DeserializeObject<List<AspNetUserRoleDto>>(userRolesJson);

            if (users == null) return;

            foreach (var dto in users)
            {
                if (await userManager.FindByIdAsync(dto.Id.ToString()) == null)
                {
                    var user = new User
                    {
                        Id = dto.Id,
                        UserName = dto.UserName,
                        Email = dto.Email,
                        FirstName = dto.FirstName,
                        LastName = dto.LastName,
                        Avatar = dto.Avatar,
                        EmailConfirmed = dto.EmailConfirmed
                    };
                    user.PasswordHash = dto.PasswordHash;
                    user.SecurityStamp = dto.SecurityStamp;

                    await userManager.CreateAsync(user);
                }
            }

            if (userRoles != null)
            {
                foreach (var ur in userRoles)
                {
                    var user = await userManager.FindByIdAsync(ur.UserId.ToString());
                    var role = await roleManager.FindByIdAsync(ur.RoleId.ToString());
                    if (user != null && role != null && !await userManager.IsInRoleAsync(user, role.Name))
                    {
                        await userManager.AddToRoleAsync(user, role.Name);
                    }
                }
            }
        }

        private static async Task SeedWeightUnits(RecipesContext context)
        {
            var json = await File.ReadAllTextAsync("SeedData/WeightUnits.json");
            var units = JsonConvert.DeserializeObject<List<WeightUnitDto>>(json);
            if (units == null) return;

            foreach (var unit in units)
            {
                if (!await context.WeightUnits.AnyAsync(w => w.Type == unit.Type))
                {
                    context.WeightUnits.Add(new WeightUnit { Type = unit.Type });
                }
            }
            await context.SaveChangesAsync();
        }
    }

    // DTOs
    public class AspNetRoleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
    }

    public class AspNetUserDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string ConcurrencyStamp { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Avatar { get; set; }
    }

    public class AspNetUserRoleDto
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
    }

    public class WeightUnitDto
    {
        public int Id { get; set; }
        public string Type { get; set; }
    }

    public class IngredientDto
    {
        public Guid Id { get; set; }
        public Guid OldId { get; set; }
        public float Quantity { get; set; }
        public string Name { get; set; }
        public int? WeightUnitId { get; set; }
    }

    public class RecipeDto
    {
        public Guid Id { get; set; }
        public int Servings { get; set; }
        public int CookingTime { get; set; }
        public string Title { get; set; }
        public string Photo { get; set; }
        public string Description { get; set; }
        public int Calories { get; set; }
        public Guid? UserId { get; set; }
        public DateTime CreationDate { get; set; }
    }

    public class CookingStepDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public Guid? RecipeId { get; set; }
    }

    public class RecipeToIngredientDto
    {
        public Guid RecipesId { get; set; }
        public Guid IngredientsId { get; set; }
    }

    public class RecipeUserDto
    {
        public Guid RecipesId { get; set; }
        public Guid UsersId { get; set; }
    }

    public class RespondDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public int Rate { get; set; }
        public Guid? RecipeId { get; set; }
        public Guid? UserId { get; set; }
    }
}