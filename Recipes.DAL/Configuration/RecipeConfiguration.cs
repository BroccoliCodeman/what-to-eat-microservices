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

        builder.HasMany(p => p.Ingredients).WithMany(p => p.Recipes);
        
    }
}