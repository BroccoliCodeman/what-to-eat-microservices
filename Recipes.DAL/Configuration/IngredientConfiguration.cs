using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Recipes.Data.Models;

namespace Recipes.DAL.Configuration;

public class IngredientConfiguration : IEntityTypeConfiguration<Ingredient>
{
    public void Configure(EntityTypeBuilder<Ingredient> builder)
    {
        builder.HasKey(i => i.Id);
        
        builder
            .Property(i => i.Quantity)
            .IsRequired();

        builder
            .HasOne(i => i.WeightUnit)
            .WithMany(wu => wu.Ingredients)
            .HasForeignKey(i => i.WeightUnitId);
    }
}