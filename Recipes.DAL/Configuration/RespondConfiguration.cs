using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Recipes.Data.Models;

namespace Recipes.DAL.Configuration;

public class RespondConfiguration : IEntityTypeConfiguration<Respond>
{
    public void Configure(EntityTypeBuilder<Respond> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder
            .Property(r => r.Text)
            .HasMaxLength(5000)
            .IsRequired();

        builder
            .Property(r => r.Rate)
            .IsRequired();
        
        builder
            .HasOne(r => r.Recipe)
            .WithMany(r => r.Responds)
            .HasForeignKey(r => r.RecipeId);
        
        builder
            .HasOne(r => r.User)
            .WithMany(u => u.Responds)
            .HasForeignKey(r => r.UserId);
    }
}