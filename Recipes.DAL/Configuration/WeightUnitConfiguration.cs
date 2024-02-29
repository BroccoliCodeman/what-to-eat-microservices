using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Recipes.Data.Models;

namespace Recipes.DAL.Configuration;

public class WeightUnitConfiguration : IEntityTypeConfiguration<WeightUnit>
{
    public void Configure(EntityTypeBuilder<WeightUnit> builder)
    {
        builder.HasKey(wu => wu.Id);
        
        builder
            .Property(wu => wu.Type)
            .HasMaxLength(20)
            .IsRequired();
        
        
    }
}