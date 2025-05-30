using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Users;

namespace Cyberjuice.Companies;

public abstract class CompanyDbContextBase<TSelf>
: AbpDbContext<TSelf>
where TSelf : DbContext
{
    protected CompanyDbContextBase(DbContextOptions<TSelf> options)
    : base(options)
    {
    }

    protected ICurrentCompany CurrentCompany => LazyServiceProvider.LazyGetRequiredService<ICurrentCompany>();

    protected ICurrentUser CurrentUser =>
    LazyServiceProvider.LazyGetRequiredService<ICurrentUser>();

    protected ICompanyFilter MultiWorkspaceFilter =>
        LazyServiceProvider.LazyGetRequiredService<ICompanyFilter>();

    protected bool IsMultiCompanyFilterEnabled =>
        DataFilter?.IsEnabled<ICompany>() ?? false;

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyCurrentCompanyId();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyCurrentCompanyId();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyCurrentCompanyId()
    {
        if (CurrentCompany?.Id == null) return;

        var currentWorkspaceId = CurrentCompany.Id.Value;

        foreach (var entry in ChangeTracker.Entries()
            .Where(e =>
                e.Entity is ICompany &&
                (e.State == EntityState.Added || e.State == EntityState.Modified)))
        {
            // Stamp the FK column via EF Core API
            entry.Property(nameof(ICompany.CompanyId)).CurrentValue = currentWorkspaceId;

            if (entry.State == EntityState.Modified)
            {
                // Prevent accidental overwrites
                entry.Property(nameof(ICompany.CompanyId)).IsModified = false;
            }
        }
    }

    protected override bool ShouldFilterEntity<TEntity>(IMutableEntityType entityType)
    {
        if (typeof(ICompany).IsAssignableFrom(typeof(TEntity)))
        {
            return true;
        }

        return base.ShouldFilterEntity<TEntity>(entityType);
    }

    protected override Expression<Func<TEntity, bool>> CreateFilterExpression<TEntity>(ModelBuilder modelBuilder)
    {
        var baseFilter = base.CreateFilterExpression<TEntity>(modelBuilder);

        if (!typeof(ICompany).IsAssignableFrom(typeof(TEntity)))
        {
            return baseFilter;
        }

        var companyIdProperty = modelBuilder
            .Entity<TEntity>()
            .Metadata
            .FindProperty(nameof(ICompany.CompanyId));

        if (companyIdProperty is null)
        {
            throw new InvalidOperationException(
                $"Entity '{typeof(TEntity).Name}' implements ICompany but does not define a '{nameof(ICompany.CompanyId)}' property.");
        }

        var companyIdColumnName = companyIdProperty.GetColumnName() ?? companyIdProperty.Name;

        // The dynamic per-request logic goes inside the expression tree
        Expression<Func<TEntity, bool>> companyFilter = entity =>
            !IsMultiCompanyFilterEnabled
            || CurrentCompany.Id == null
            || (
                EF.Property<Guid?>(entity, companyIdColumnName) == CurrentCompany.Id
                && CurrentCompany.HasAccessToCurrentCompany // evaluated per-request
            );

        return baseFilter is null
            ? companyFilter
            : QueryFilterExpressionHelper.CombineExpressions(baseFilter, companyFilter);
    }

}
