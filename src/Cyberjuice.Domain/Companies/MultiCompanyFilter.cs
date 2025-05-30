using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace Cyberjuice.Companies;

/// <summary>
/// Implementation of the multi-Company filter
/// </summary>
public class MultiCompanyFilter(
    ICurrentCompany CurrentWorkspace,
    IDataFilter DataFilter)
    : ICompanyFilter, ITransientDependency
{
    protected readonly IDataFilter DataFilter = DataFilter;

    public bool IsEnabled => DataFilter.IsEnabled<ICompany>();
    public virtual bool IsCompanyEnabled => IsEnabled && CurrentWorkspace.Id.HasValue;
    public IDisposable Disable()
    {
        return DataFilter.Disable<ICompany>();
    }
    public IDisposable Enable()
    {
        return DataFilter.Enable<ICompany>();
    }

    public IQueryable<TEntity> ApplyFilter<TEntity>(IQueryable<TEntity> query)
        where TEntity : class, ICompany
    {
        if (!IsCompanyEnabled)
        {
            return query;
        }
        return query.Where(e => e.CompanyId == CurrentWorkspace.Id);
    }
    public Task ConfigureFilterAsync<TEntity>(IQueryable<TEntity> query)
        where TEntity : class, ICompany
    {
        // Implementation might not be needed for all cases, 
        // but provided for completeness with IMultiTenant pattern
        return Task.CompletedTask;
    }
}
