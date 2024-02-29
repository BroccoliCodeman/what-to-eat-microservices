using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Recipes.Data.Models;

namespace Recipes.DAL.Configuration;

public class CookingStepConfiguration : IEntityTypeConfiguration<CookingStep>
{
    public void Configure(EntityTypeBuilder<CookingStep> builder)
    {
        builder.HasKey(cs => cs.Id);
        
        builder
            .Property(cs => cs.Description)
            .HasMaxLength(3000)
            .IsRequired();

        builder
            .Property(cs => cs.Order)
            .HasMaxLength(2)
            .IsRequired();
        
        builder
            .HasOne(cs => cs.Recipe)
            .WithMany(r => r.CookingSteps)
            .HasForeignKey(cs => cs.RecipeId);
    }
}