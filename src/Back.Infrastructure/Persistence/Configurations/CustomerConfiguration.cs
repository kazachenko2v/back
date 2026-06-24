using Back.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Back.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(customer => customer.Id);

        builder.Ignore(customer => customer.DomainEvents);

        builder.Property(customer => customer.Id)
            .ValueGeneratedNever();

        builder.Property(customer => customer.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(customer => customer.CreatedAtUtc)
            .IsRequired();

        builder.OwnsOne(customer => customer.Email, email =>
        {
            email.Property(valueObject => valueObject.Value)
                .HasColumnName("Email")
                .HasMaxLength(256)
                .IsRequired();

            email.HasIndex(valueObject => valueObject.Value)
                .IsUnique();
        });

        builder.Navigation(customer => customer.Email)
            .IsRequired();
    }
}
