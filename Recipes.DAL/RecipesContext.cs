using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Recipes.DAL.Seeding;
using Recipes.Data.Models;

namespace Recipes.DAL;

public class RecipesContext : IdentityDbContext<User, UserRole, Guid>
{
    public RecipesContext(DbContextOptions<RecipesContext> options) : base(options) {
        Database.EnsureCreated();
    }
    
    public DbSet<CookingStep> CookingSteps { get; set; } = null!;
    public DbSet<Ingredient> Ingredients { get; set; } = null!;
    public DbSet<Recipe> Recipes { get; set; } = null!;
    public DbSet<Respond> Responds { get; set; } = null!;
    public DbSet<WeightUnit> WeightUnits { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}