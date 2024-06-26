using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Recipes.Data.Models;
using System.Reflection.Emit;

namespace Recipes.DAL.Configuration;

public class CookingStepConfiguration : IEntityTypeConfiguration<CookingStep>
{
    public void Configure(EntityTypeBuilder<CookingStep> builder)
    {
        builder.HasKey(cs => cs.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();


        builder
            .Property(cs => cs.Description)
            .HasMaxLength(3000)
            .IsRequired();

        builder
            .Property(cs => cs.Order)
            .HasMaxLength(2)
            .IsRequired();
        
    }
}