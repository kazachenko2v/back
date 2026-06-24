using Back.Application.Abstractions;
using Back.Domain.Common;
using Back.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace Back.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);
        ClearDomainEvents();

        return result;
    }

    private void ClearDomainEvents()
    {
        var entities = ChangeTracker
            .Entries()
            .Select(entry => entry.Entity)
            .OfType<IHasDomainEvents>()
            .Where(entity => entity.DomainEvents.Count > 0);

        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }
    }
}
