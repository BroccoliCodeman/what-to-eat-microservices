using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Recipes.Data.Models;
using Microsoft.EntityFrameworkCore.Storage;
using Recipes.Data.Parsers;

namespace Recipes.DAL.Seeding
{
    public static class DatabaseSeeding
    {
        // ID дефолтного користувача
        private static readonly Guid DEFAULT_USER_ID = Guid.Parse("8e445865-a24d-4543-a6c6-9443d048cdb9");

        public static async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RecipesContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<UserRole>>();

            IDbContextTransaction? transaction = null;
            string csvRecipesFilePath = "SeedData/recipes.csv";
            string csvReviewsFilePath = "SeedData/reviews.csv";

            try
            {
                // Перевіряємо, чи база вже заповнена
                Console.WriteLine("Checking database status...");

                var hasRoles = await roleManager.Roles.AnyAsync();
                var hasUsers = await userManager.Users.AnyAsync();
                var hasWeightUnits = await context.WeightUnits.AnyAsync();
                var hasRecipes = await context.Recipes.AnyAsync();
                var hasIngredients = await context.Ingredients.AnyAsync();

                if (hasRoles && hasUsers && hasWeightUnits && hasRecipes && hasIngredients)
                {
                    Console.WriteLine("Database already seeded. Skipping seeding process.");
                    return;
                }

                Console.WriteLine($"Database status: Roles={hasRoles}, Users={hasUsers}, WeightUnits={hasWeightUnits}, Recipes={hasRecipes}, Ingredients={hasIngredients}");
                Console.WriteLine("Starting seeding process...");

                transaction = await context.Database.BeginTransactionAsync();

                // 1. Базові сутності
                if (!hasRoles)
                {
                    Console.WriteLine("Seeding roles...");
                    await SeedRoles(roleManager);
                }
                else
                {
                    Console.WriteLine("Roles already exist. Skipping.");
                }

                if (!hasUsers)
                {
                    Console.WriteLine("Seeding users...");
                    await SeedUsers(userManager, roleManager);
                }
                else
                {
                    Console.WriteLine("Users already exist. Skipping.");
                }

                if (!hasWeightUnits)
                {
                    Console.WriteLine("Seeding weight units...");
                    await SeedWeightUnits(context);
                }
                else
                {
                    Console.WriteLine("Weight units already exist. Skipping.");
                }

                await transaction.CommitAsync();
                transaction = await context.Database.BeginTransactionAsync();

                // 2. Парсимо та додаємо рецепти з CSV
                if (!hasRecipes || !hasIngredients)
                {
                    Console.WriteLine("Seeding recipes from CSV...");
                    await SeedRecipesFromCsv(context, csvRecipesFilePath, csvReviewsFilePath);
                }
                else
                {
                    Console.WriteLine("Recipes and ingredients already exist. Skipping.");
                }

                // 3. Зв'язки користувачів з рецептами (SavedRecipes)
                if (hasRecipes && hasUsers)
                {
                    Console.WriteLine("Linking recipes to users...");
                    await SeedRecipeUser(context);
                }

                await transaction.CommitAsync();

                Console.WriteLine("Database seeding completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding database: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                }

                throw;
            }
            finally
            {
                transaction?.Dispose();
            }
        }
        private static async Task SeedRecipesFromCsv(RecipesContext context, string csvRecipesFilePath, string csvReviewsFilePath)
        {
            if (!File.Exists(csvRecipesFilePath))
            {
                Console.WriteLine($"Warning: {csvRecipesFilePath} not found. Skipping recipes seeding.");
                return;
            }

            if (!File.Exists(csvReviewsFilePath))
            {
                Console.WriteLine($"Warning: {csvReviewsFilePath} not found. Reviews will be skipped.");
            }

            var csvParser = new RecipeCsvParser();
            var parsedRecipes = csvParser.ParseRecipes(csvRecipesFilePath, csvReviewsFilePath);

            if (parsedRecipes == null || !parsedRecipes.Any())
            {
                Console.WriteLine("No recipes parsed from CSV.");
                return;
            }

            // Отримуємо існуючі заголовки рецептів
            var existingTitles = await context.Recipes
                .AsNoTracking()
                .Select(r => r.Title)
                .ToListAsync();
            var existingTitlesSet = new HashSet<string>(existingTitles);

            var recipesToAdd = new List<Recipe>();

            foreach (var parsedRecipe in parsedRecipes)
            {
                if (string.IsNullOrEmpty(parsedRecipe.Photo))
                {
                    Console.WriteLine("Skipping recipe with no photo.");
                    continue;
                }
                if (string.IsNullOrWhiteSpace(parsedRecipe.Title))
                {
                    Console.WriteLine("Skipping recipe with empty title.");
                    continue;
                }

                if (existingTitlesSet.Contains(parsedRecipe.Title))
                {
                    continue;
                }

                // Створюємо новий рецепт
                var newRecipe = new Recipe
                {
                    Id = parsedRecipe.Id,
                    Title = parsedRecipe.Title,
                    Description = parsedRecipe.Description,
                    Servings = parsedRecipe.Servings,
                    CookingTime = parsedRecipe.CookingTime,
                    Calories = parsedRecipe.Calories,
                    Photo = parsedRecipe.Photo,
                    CreationDate = parsedRecipe.CreationDate,
                    AuthorId = DEFAULT_USER_ID,
                    Ingredients = new List<Ingredient>(),
                    CookingSteps = new List<CookingStep>(),
                    Responds = new List<Respond>()
                };

                var wu = await context.WeightUnits.FirstAsync();
                // Додаємо інгредієнти - кожен інгредієнт є окремим екземпляром з кількістю
                if (parsedRecipe.Ingredients != null)
                {
                    foreach (var parsedIngredient in parsedRecipe.Ingredients)
                    {
                        // Створюємо новий екземпляр інгредієнта для цього рецепта
                        var recipeIngredient = new Ingredient
                        {
                            Id = Guid.NewGuid(),
                            Name = parsedIngredient.Name,
                            Quantity = parsedIngredient.Quantity, // ✅ Зберігаємо кількість з CSV
                            WeightUnit = wu
                        };

                        newRecipe.Ingredients.Add(recipeIngredient);
                    }
                }

                // Додаємо кроки приготування
                if (parsedRecipe.CookingSteps != null)
                {
                    foreach (var step in parsedRecipe.CookingSteps)
                    {
                        newRecipe.CookingSteps.Add(new CookingStep
                        {
                            Id = step.Id,
                            Description = step.Description,
                            Order = step.Order,
                            RecipeId = newRecipe.Id
                        });
                    }
                }

                // Додаємо відгуки
                if (parsedRecipe.Responds != null)
                {
                    foreach (var respond in parsedRecipe.Responds)
                    {
                        newRecipe.Responds.Add(new Respond
                        {
                            Id = respond.Id,
                            Text = respond.Text,
                            Rate = respond.Rate,
                            RecipeId = newRecipe.Id,
                            UserId = respond.UserId
                        });
                    }
                }

                recipesToAdd.Add(newRecipe);
                existingTitlesSet.Add(parsedRecipe.Title);
            }

            if (recipesToAdd.Any())
            {
                await context.Recipes.AddRangeAsync(recipesToAdd);
                await context.SaveChangesAsync();
                Console.WriteLine($"Added {recipesToAdd.Count} new recipes from CSV.");

                // Підраховуємо скільки інгредієнтів було додано
                var totalIngredients = recipesToAdd.Sum(r => r.Ingredients.Count);
                Console.WriteLine($"Added {totalIngredients} ingredient entries with quantities.");
            }
        }
        private static async Task SeedRecipeUser(RecipesContext context)
        {
            // Перевіряємо, чи існує дефолтний користувач
            var defaultUser = await context.Users
                .FirstOrDefaultAsync(u => u.Id == DEFAULT_USER_ID);

            if (defaultUser == null)
            {
                Console.WriteLine($"Warning: Default user with ID {DEFAULT_USER_ID} not found. Skipping recipe-user links.");
                return;
            }

            // Завантажуємо всі рецепти БЕЗ tracking для оптимізації
            var recipeIds = await context.Recipes
                .Select(r => r.Id)
                .ToListAsync();

            if (!recipeIds.Any())
            {
                Console.WriteLine("No recipes found.");
                return;
            }

            var existingSavedRecipeIds = await context.Recipes.Where(p=>p.Author==null).ToListAsync();

            if (!existingSavedRecipeIds.Any())
            {
                Console.WriteLine("All recipes already linked to default user.");
                return;
            }

            // Додаємо зв'язки через raw SQL для оптимізації
            foreach (var recipe in existingSavedRecipeIds)
            {
                recipe.Author = defaultUser;
            }
            await context.SaveChangesAsync();
        }

        private static async Task SeedRoles(RoleManager<UserRole> roleManager)
        {
            var filePath = "SeedData/AspNetRoles.json";
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Warning: {filePath} not found. Skipping roles seeding.");
                return;
            }

            var json = await File.ReadAllTextAsync(filePath);
            var roles = JsonConvert.DeserializeObject<List<AspNetRoleDto>>(json);
            if (roles == null || !roles.Any()) return;

            foreach (var r in roles)
            {
                if (string.IsNullOrWhiteSpace(r.Name)) continue;

                if (!await roleManager.RoleExistsAsync(r.Name))
                {
                    var result = await roleManager.CreateAsync(new UserRole
                    {
                        Id = r.Id,
                        Name = r.Name,
                        NormalizedName = r.NormalizedName
                    });

                    if (result.Succeeded)
                    {
                        Console.WriteLine($"Role '{r.Name}' created successfully.");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to create role '{r.Name}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }

        private static async Task SeedUsers(UserManager<User> userManager, RoleManager<UserRole> roleManager)
        {
            var usersFilePath = "SeedData/AspNetUsers.json";
            var userRolesFilePath = "SeedData/AspNetUserRoles.json";

            if (!File.Exists(usersFilePath))
            {
                Console.WriteLine($"Warning: {usersFilePath} not found. Skipping users seeding.");
                return;
            }

            var json = await File.ReadAllTextAsync(usersFilePath);
            var users = JsonConvert.DeserializeObject<List<AspNetUserDto>>(json);
            if (users == null || !users.Any()) return;

            foreach (var dto in users)
            {
                if (string.IsNullOrWhiteSpace(dto.UserName)) continue;

                var exists = await userManager.FindByIdAsync(dto.Id.ToString());
                if (exists == null)
                {
                    var user = new User
                    {
                        Id = dto.Id,
                        UserName = dto.UserName,
                        Email = dto.Email,
                        FirstName = dto.FirstName,
                        LastName = dto.LastName,
                        Avatar = dto.Avatar,
                        EmailConfirmed = dto.EmailConfirmed,
                        PasswordHash = dto.PasswordHash,
                        SecurityStamp = dto.SecurityStamp
                    };

                    var result = await userManager.CreateAsync(user);

                    if (result.Succeeded)
                    {
                        Console.WriteLine($"User '{user.UserName}' created successfully.");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to create user '{user.UserName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }

            if (!File.Exists(userRolesFilePath))
            {
                Console.WriteLine($"Warning: {userRolesFilePath} not found. Skipping user roles assignment.");
                return;
            }

            var userRolesJson = await File.ReadAllTextAsync(userRolesFilePath);
            var userRoles = JsonConvert.DeserializeObject<List<AspNetUserRoleDto>>(userRolesJson);
            if (userRoles == null || !userRoles.Any()) return;

            var allRoles = await roleManager.Roles.ToListAsync();

            foreach (var ur in userRoles)
            {
                var user = await userManager.FindByIdAsync(ur.UserId.ToString());
                var role = allRoles.FirstOrDefault(r => r.Id == ur.RoleId);

                if (user != null && role != null && !string.IsNullOrWhiteSpace(role.Name))
                {
                    if (!await userManager.IsInRoleAsync(user, role.Name))
                    {
                        var result = await userManager.AddToRoleAsync(user, role.Name);

                        if (result.Succeeded)
                        {
                            Console.WriteLine($"User '{user.UserName}' assigned to role '{role.Name}'.");
                        }
                        else
                        {
                            Console.WriteLine($"Failed to assign user '{user.UserName}' to role '{role.Name}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        }
                    }
                }
            }
        }

        private static async Task SeedWeightUnits(RecipesContext context)
        {
            var filePath = "SeedData/WeightUnits.json";
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Warning: {filePath} not found. Skipping weight units seeding.");
                return;
            }

            var json = await File.ReadAllTextAsync(filePath);
            var units = JsonConvert.DeserializeObject<List<WeightUnitDto>>(json);
            if (units == null || !units.Any()) return;

            var existingUnits = await context.WeightUnits
                .AsNoTracking()
                .Select(w => w.Type)
                .ToListAsync();
            var existingSet = new HashSet<string>(existingUnits);

            var unitsToAdd = new List<WeightUnit>();

            foreach (var unit in units)
            {
                if (string.IsNullOrWhiteSpace(unit.Type)) continue;

                if (!existingSet.Contains(unit.Type))
                {
                    unitsToAdd.Add(new WeightUnit { Type = unit.Type });
                    existingSet.Add(unit.Type);
                }
            }

            if (unitsToAdd.Any())
            {
                await context.WeightUnits.AddRangeAsync(unitsToAdd);
                await context.SaveChangesAsync();
                Console.WriteLine($"Added {unitsToAdd.Count} new weight units.");
            }
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
}