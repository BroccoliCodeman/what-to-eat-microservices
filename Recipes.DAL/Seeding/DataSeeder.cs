using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Recipes.DAL;
using Recipes.Data;
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

            // Перевірка чи база порожня
            if (await context.Recipes.AnyAsync())
            {
                Console.WriteLine("Database already seeded");
                return;
            }

            try
            {
                // 1. Seed Roles
                await SeedRoles(roleManager);

                // 2. Seed Users
                await SeedUsers(userManager, roleManager);

                // 3. Seed WeightUnits
                await SeedWeightUnits(context);

                // 4. Seed Ingredients
                await SeedIngredients(context);

                // 5. Seed Recipes
                await SeedRecipes(context);

                // 6. Seed CookingSteps
                await SeedCookingSteps(context);

                // 7. Seed IngredientRecipe (many-to-many)
                await SeedIngredientRecipe(context);

                // 8. Seed RecipeUser (saved recipes)
                await SeedRecipeUser(context);

                // 9. Seed Responds
                await SeedResponds(context);

                Console.WriteLine("Database seeded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding database: {ex.Message}");
                throw;
            }
        }

        private static async Task SeedRoles(RoleManager<UserRole> roleManager)
        {
            var json = await File.ReadAllTextAsync("SeedData/AspNetRoles.json");
            var roles = JsonConvert.DeserializeObject<List<AspNetRoleDto>>(json);

            if (roles == null) return;

            foreach (var roleDto in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleDto.Name))
                {
                    var role = new UserRole
                    {
                        Id = roleDto.Id,
                        Name = roleDto.Name,
                        NormalizedName = roleDto.NormalizedName
                    };
                    await roleManager.CreateAsync(role);
                }
            }
        }

        private static async Task SeedUsers(UserManager<User> userManager, RoleManager<UserRole> roleManager)
        {
            var json = await File.ReadAllTextAsync("SeedData/AspNetUsers.json");
            var users = JsonConvert.DeserializeObject<List<AspNetUserDto>>(json);

            if (users == null) return;

            foreach (var userDto in users)
            {
                if (await userManager.FindByIdAsync(userDto.Id.ToString()) == null)
                {
                    var user = new User
                    {
                        Id = userDto.Id,
                        UserName = userDto.UserName,
                        NormalizedUserName = userDto.NormalizedUserName,
                        Email = userDto.Email,
                        NormalizedEmail = userDto.NormalizedEmail,
                        EmailConfirmed = userDto.EmailConfirmed,
                        PasswordHash = userDto.PasswordHash,
                        SecurityStamp = userDto.SecurityStamp,
                        ConcurrencyStamp = userDto.ConcurrencyStamp,
                        PhoneNumber = userDto.PhoneNumber,
                        PhoneNumberConfirmed = userDto.PhoneNumberConfirmed,
                        TwoFactorEnabled = userDto.TwoFactorEnabled,
                        LockoutEnd = userDto.LockoutEnd,
                        LockoutEnabled = userDto.LockoutEnabled,
                        AccessFailedCount = userDto.AccessFailedCount,
                        FirstName = userDto.FirstName,
                        LastName = userDto.LastName,
                        Avatar = userDto.Avatar
                    };
                    await userManager.CreateAsync(user);
                }
            }

            // Seed UserRoles
            var userRolesJson = await File.ReadAllTextAsync("SeedData/AspNetUserRoles.json");
            var userRoles = JsonConvert.DeserializeObject<List<AspNetUserRoleDto>>(userRolesJson);

            if (userRoles != null)
            {
                foreach (var ur in userRoles)
                {
                    var user = await userManager.FindByIdAsync(ur.UserId.ToString());
                    var role = await roleManager.FindByIdAsync(ur.RoleId.ToString());

                    if (user != null && role != null)
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

            if (units == null || units.Count == 0) return;

            // Перевіряємо чи є що додавати
            var existingIds = await context.WeightUnits.Select(w => w.Id).ToListAsync();
            var newUnits = units.Where(u => !existingIds.Contains(u.Id)).ToList();

            if (newUnits.Count == 0) return;

            // Використовуємо транзакцію та пряме SQL
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT WeightUnits ON");

                foreach (var unit in newUnits)
                {
                    await context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO WeightUnits (Id, Type) VALUES ({0}, {1})",
                        unit.Id, unit.Type);
                }

                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT WeightUnits OFF");
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static async Task SeedIngredients(RecipesContext context)
        {
            var json = await File.ReadAllTextAsync("SeedData/Ingredients.json");
            var ingredients = JsonConvert.DeserializeObject<List<IngredientDto>>(json);

            if (ingredients == null || ingredients.Count == 0) return;

            // Ingredients використовують Guid, тому просто додаємо через EF
            foreach (var ingDto in ingredients)
            {
                if (!await context.Ingredients.AnyAsync(i => i.Id == ingDto.Id))
                {
                    context.Ingredients.Add(new Ingredient
                    {
                        Id = ingDto.Id,
                        Quantity = ingDto.Quantity,
                        Name = ingDto.Name,
                        WeightUnitId = ingDto.WeightUnitId
                    });
                }
            }
            await context.SaveChangesAsync();
        }

        private static async Task SeedRecipes(RecipesContext context)
        {
            var json = await File.ReadAllTextAsync("SeedData/Recipes.json");
            var recipes = JsonConvert.DeserializeObject<List<RecipeDto>>(json);

            if (recipes == null || recipes.Count == 0) return;

            // Recipes використовують Guid, тому просто додаємо через EF
            foreach (var recipeDto in recipes)
            {
                if (!await context.Recipes.AnyAsync(r => r.Id == recipeDto.Id))
                {
                    context.Recipes.Add(new Recipe
                    {
                        Id = recipeDto.Id,
                        Servings = recipeDto.Servings,
                        CookingTime = recipeDto.CookingTime,
                        Title = recipeDto.Title,
                        Photo = recipeDto.Photo,
                        Description = recipeDto.Description,
                        Calories = recipeDto.Calories,
                        UserId = recipeDto.UserId,
                        CreationDate = recipeDto.CreationDate
                    });
                }
            }
            await context.SaveChangesAsync();
        }

        private static async Task SeedCookingSteps(RecipesContext context)
        {
            var json = await File.ReadAllTextAsync("SeedData/CookingSteps.json");
            var steps = JsonConvert.DeserializeObject<List<CookingStepDto>>(json);

            if (steps == null || steps.Count == 0) return;

            // CookingSteps використовують Guid, тому просто додаємо через EF
            foreach (var stepDto in steps)
            {
                if (!await context.CookingSteps.AnyAsync(s => s.Id == stepDto.Id))
                {
                    context.CookingSteps.Add(new CookingStep
                    {
                        Id = stepDto.Id,
                        Description = stepDto.Description,
                        Order = stepDto.Order,
                        RecipeId = stepDto.RecipeId
                    });
                }
            }
            await context.SaveChangesAsync();
        }

        private static async Task SeedIngredientRecipe(RecipesContext context)
        {
            var json = await File.ReadAllTextAsync("SeedData/IngredientRecipe.json");
            var relations = JsonConvert.DeserializeObject<List<RecipeToIngredientDto>>(json);

            if (relations == null) return;

            foreach (var rel in relations)
            {
                var recipe = await context.Recipes
                    .Include(r => r.Ingredients)
                    .FirstOrDefaultAsync(r => r.Id == rel.RecipeId);

                var ingredient = await context.Ingredients.FindAsync(rel.IngredientId);

                if (recipe != null && ingredient != null)
                {
                    recipe.Ingredients ??= new List<Ingredient>();
                    if (!recipe.Ingredients.Any(i => i.Id == ingredient.Id))
                    {
                        recipe.Ingredients.Add(ingredient);
                    }
                }
            }
            await context.SaveChangesAsync();
        }

        private static async Task SeedRecipeUser(RecipesContext context)
        {
            var json = await File.ReadAllTextAsync("SeedData/RecipeUser.json");
            var relations = JsonConvert.DeserializeObject<List<RecipeUserDto>>(json);

            if (relations == null) return;

            foreach (var rel in relations)
            {
                var recipe = await context.Recipes
                    .Include(r => r.Users)
                    .FirstOrDefaultAsync(r => r.Id == rel.RecipesId);
                var user = await context.Users.FindAsync(rel.UsersId);

                if (recipe != null && user != null)
                {
                    recipe.Users ??= new List<User>();
                    if (!recipe.Users.Any(u => u.Id == user.Id))
                    {
                        recipe.Users.Add(user);
                    }
                }
            }
            await context.SaveChangesAsync();
        }

        private static async Task SeedResponds(RecipesContext context)
        {
            var json = await File.ReadAllTextAsync("SeedData/Responds.json");
            var responds = JsonConvert.DeserializeObject<List<RespondDto>>(json);

            if (responds == null || responds.Count == 0) return;

            // Responds використовують Guid, тому просто додаємо через EF
            foreach (var respondDto in responds)
            {
                if (!await context.Responds.AnyAsync(r => r.Id == respondDto.Id))
                {
                    context.Responds.Add(new Respond
                    {
                        Id = respondDto.Id,
                        Text = respondDto.Text,
                        Rate = respondDto.Rate,
                        RecipeId = respondDto.RecipeId,
                        UserId = respondDto.UserId
                    });
                }
            }
            await context.SaveChangesAsync();
        }
    }

    // DTOs залишаються без змін
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
        public Guid RecipeId { get; set; }
        public Guid IngredientId { get; set; }
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