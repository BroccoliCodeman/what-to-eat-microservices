using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Recipes.Data.Models;

namespace Recipes.DAL.Configuration;

public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder
            .Property(r => r.Servings)
            .IsRequired();

        builder
            .Property(r => r.CookingTime)
            .IsRequired();

        builder
            .Property(r => r.Title)
            .HasMaxLength(100)
            .IsRequired();

        builder
            .Property(r => r.Photo);

        builder
            .Property(r => r.Description)
            .HasMaxLength(3000)
            .IsRequired();

        builder
            .Property(r => r.Calories)
            .HasMaxLength(20);

        builder
            .Property(r => r.CreationDate)
            .IsRequired();

        // Багато-до-багатьох: Recipe <-> Ingredient
        builder
            .HasMany(r => r.Ingredients)
            .WithMany(i => i.Recipes);

        // Багато-до-багатьох: Recipe <-> User (збережені рецепти)
        builder
            .HasMany(r => r.SavedByUsers)
            .WithMany(u => u.SavedRecipes)
            .UsingEntity(j => j.ToTable("UserSavedRecipes"));

        // Один-до-багатьох: Recipe -> User (автор)
        builder
            .HasOne(r => r.Author)
            .WithMany(u => u.CreatedRecipes)
            .HasForeignKey(r => r.AuthorId)
            .OnDelete(DeleteBehavior.SetNull);

        // Один-до-багатьох: Recipe -> CookingStep
        builder
            .HasMany(r => r.CookingSteps)
            .WithOne(cs => cs.Recipe)
            .HasForeignKey(cs => cs.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}