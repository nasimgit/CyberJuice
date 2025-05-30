using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cyberjuice.Companies;

/// <summary>
/// Defines the interface for Company filtering
/// </summary>
public interface ICompanyFilter
{
    /// <summary>
    /// Gets a value indicating whether this filter is enabled or not.
    /// </summary>
    bool IsEnabled { get; }
    /// <summary>
    /// Gets a value indicating whether the Company filtering should be applied.
    /// </summary>
    bool IsCompanyEnabled { get; }
    /// <summary>
    /// Disables data filtering. Returns a disposable object to re-enable it later.
    /// </summary>
    /// <returns>A disposable object to re-enable it later.</returns>
    IDisposable Disable();
    /// <summary>
    /// Enables data filtering.
    /// </summary>
    /// <returns>A disposable object to disable filtering.</returns>
    IDisposable Enable();
    /// <summary>
    /// Applies the filter to the query.
    /// </summary>
    /// <typeparam name="TEntity">Type of the entity</typeparam>
    /// <param name="query">The query to filter</param>
    /// <returns>Filtered query</returns>
    IQueryable<TEntity> ApplyFilter<TEntity>(IQueryable<TEntity> query)
        where TEntity : class, ICompany;
    /// <summary>
    /// Configures the filter on the query.
    /// </summary>
    /// <typeparam name="TEntity">Type of the entity</typeparam>
    /// <param name="query">The query to configure</param>
    /// <returns>A task</returns>
    Task ConfigureFilterAsync<TEntity>(IQueryable<TEntity> query)
        where TEntity : class, ICompany;
}
