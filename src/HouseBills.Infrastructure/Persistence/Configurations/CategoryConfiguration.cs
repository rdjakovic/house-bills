using HouseBills.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseBills.Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.RowVersion).IsRowVersion();
        builder.Property(c => c.Name).HasMaxLength(Category.NameMaxLength).IsRequired();
        builder.HasIndex(c => c.Name).IsUnique();

        builder.HasData(
            new { Id = 1, Name = "Utilities" },
            new { Id = 2, Name = "Rent / Mortgage" },
            new { Id = 3, Name = "Internet & Phone" },
            new { Id = 4, Name = "Insurance" },
            new { Id = 5, Name = "Taxes & Fees" },
            new { Id = 6, Name = "Subscriptions" },
            new { Id = 7, Name = "Other" });
    }
}