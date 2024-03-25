using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Recipes.Data.Models;

namespace Recipes.DAL.Configuration;

public class SavedRecipeConfiguration : IEntityTypeConfiguration<SavedRecipe>
{
    public void Configure(EntityTypeBuilder<SavedRecipe> builder)
    {
        builder.HasKey(x => x.Id);

        builder
            .Property(sr => sr.IsSaved)
            .IsRequired();
        
        builder
            .HasOne(sr => sr.Recipe)
            .WithMany(r => r.SavedRecipes)
            .HasForeignKey(sr => sr.RecipeId);
    }
}