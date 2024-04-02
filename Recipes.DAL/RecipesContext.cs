using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Recipes.Data.Models;

namespace Recipes.DAL;

public class RecipesContext : DbContext
{
    public RecipesContext(DbContextOptions<RecipesContext> options) : base(options) {
        Database.EnsureCreated();
    }
    
    public DbSet<Recipe> Recipes { get; set; } = null!;
    public DbSet<Ingredient> Ingredients { get; set; } = null!;
   // public DbSet<RecipeIngredient> RecipeIngredients { get; set; } = null!;
    public DbSet<CookingStep> CookingSteps { get; set; } = null!;
    public DbSet<Respond> Responds { get; set; } = null!;
    public DbSet<SavedRecipe> SavedRecipes { get; set; } = null!;
    public DbSet<WeightUnit> WeightUnits { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}