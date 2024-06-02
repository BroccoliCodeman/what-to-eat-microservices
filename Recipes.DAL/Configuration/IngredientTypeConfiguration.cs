using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Recipes.Data.Models;

namespace Recipes.DAL.Configuration;

public class IngredientTypeConfiguration : IEntityTypeConfiguration<TypeofIngredient>
{
    public void Configure(EntityTypeBuilder<TypeofIngredient> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .HasMaxLength(50);
    }
}