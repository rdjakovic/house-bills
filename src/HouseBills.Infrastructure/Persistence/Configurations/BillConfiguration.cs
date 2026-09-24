using HouseBills.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseBills.Infrastructure.Persistence.Configurations;

internal sealed class BillConfiguration : IEntityTypeConfiguration<Bill>
{
    public void Configure(EntityTypeBuilder<Bill> builder)
    {
        builder.ToTable("Bills");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.RowVersion).IsRowVersion();
        builder.Property(b => b.Description).HasMaxLength(Bill.DescriptionMaxLength).IsRequired();
        builder.Property(b => b.Amount).HasPrecision(MoneyRules.Precision, MoneyRules.Scale);
        builder.Property(b => b.PaidAmount).HasPrecision(MoneyRules.Precision, MoneyRules.Scale);
        builder.Property(b => b.Notes).HasMaxLength(Bill.NotesMaxLength);
        builder.Ignore(b => b.IsPaid);

        builder.HasOne<Payee>().WithMany().HasForeignKey(b => b.PayeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Category>().WithMany().HasForeignKey(b => b.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<RecurringBill>().WithMany().HasForeignKey(b => b.RecurringBillId).OnDelete(DeleteBehavior.SetNull);

        // Bill list and reports filter on a due-date range; include the columns they aggregate so the index covers them.
        builder.HasIndex(b => b.DueDate)
            .IncludeProperties(b => new { b.Amount, b.PaidOn, b.PaidAmount, b.CategoryId });

        // Safety net against generating the same occurrence twice.
        builder.HasIndex(b => new { b.RecurringBillId, b.DueDate }).IsUnique();
    }
}