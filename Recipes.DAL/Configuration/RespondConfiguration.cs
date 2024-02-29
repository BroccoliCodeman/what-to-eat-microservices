using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Recipes.Data.Models;

namespace Recipes.DAL.Configuration;

public class RespondConfiguration : IEntityTypeConfiguration<Respond>
{
    public void Configure(EntityTypeBuilder<Respond> builder)
    {
        builder.HasKey(x => x.Id);
        builder
            .Property(r => r.Text)
            .HasMaxLength(1000)
            .IsRequired();

        builder
            .Property(r => r.Rate)
            .HasMaxLength(1)
            .IsRequired();
        
        builder
            .HasOne(r => r.Recipe)
            .WithMany(r => r.Responds)
            .HasForeignKey(r => r.RecipeId);
    }
}