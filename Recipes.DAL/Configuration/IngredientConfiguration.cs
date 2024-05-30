using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Recipes.Data.Models;

namespace Recipes.DAL.Configuration;

public class IngredientConfiguration : IEntityTypeConfiguration<Ingredient>
{
    public void Configure(EntityTypeBuilder<Ingredient> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder
            .Property(i => i.Quantity)
            .IsRequired();

        builder
            .HasOne(i => i.WeightUnit).WithMany(p=>p.Ingredients);

        builder.HasMany(p => p.Recipes).WithMany(p => p.Ingredients);
         
    }
}