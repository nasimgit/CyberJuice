using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Users;

namespace Cyberjuice.Companies;

public class CurrentCompany : ICurrentCompany, ISingletonDependency
{
    private readonly AsyncLocal<CompanyCacheItem> _currentCompanyCacheItem = new AsyncLocal<CompanyCacheItem>();
    private readonly ConcurrentDictionary<string, CompanyCacheItem> _workspaceConfigurations = new ConcurrentDictionary<string, CompanyCacheItem>();
    /// <summary>
    /// Gets current Company's Id.
    /// </summary>
    public virtual Guid? Id => _currentCompanyCacheItem.Value?.CompanyId;
    /// <summary>
    /// Gets current Company's name.
    /// </summary>
    public virtual string Name => _currentCompanyCacheItem.Value?.Name;
    /// <summary>
    /// Gets a value indicates that current Company is available.
    /// </summary>
    public virtual bool IsAvailable => Id.HasValue;

    public bool HasAccessToCurrentCompany => _currentCompanyCacheItem.Value == null ? false : _currentCompanyCacheItem.Value.HasAccessToCurrentCompany;

    /// <summary>
    /// Changes current Company Id.
    /// </summary>
    /// <param name="id">Company Id</param>
    /// <returns>A disposable object to restore Company Id when disposed.</returns>
    public virtual IDisposable Change(Guid? id)
    {
        return Change(false, id, null);
    }
    /// <summary>
    /// Changes current Company Id and Name.
    /// </summary>
    /// <param name="id">Company Id</param>
    /// <param name="name">Company Name</param>
    /// <returns>A disposable object to restore Company values when disposed.</returns>
    public virtual IDisposable Change(bool hasAccessToCurrentCompany, Guid? id, string name)
    {
        var companyCacheItem = _currentCompanyCacheItem.Value;
        var previousCompanyId = companyCacheItem?.CompanyId;
        var previousCopanyName = companyCacheItem?.Name;
        if (id == previousCompanyId && name == previousCopanyName)
        {
            return NullCompanyRestore.Instance;
        }
        _currentCompanyCacheItem.Value = new CompanyCacheItem(hasAccessToCurrentCompany, id, name);
        return new CompanyRestore(this, previousCompanyId, previousCopanyName);
    }
    private class CompanyCacheItem
    {
        public Guid? CompanyId { get; }
        public bool HasAccessToCurrentCompany { get; }
        public string Name { get; }
        public CompanyCacheItem(bool hasAccessToCurrentCompany, Guid? companyId, string name = null)
        {
            CompanyId = companyId;
            Name = name;
            HasAccessToCurrentCompany = hasAccessToCurrentCompany;
        }
    }
    private class CompanyRestore : IDisposable
    {
        private readonly CurrentCompany _currentCompany;
        private readonly Guid? _copmanyId;
        private readonly string _companyName;
        private readonly bool _hasAccessToCurrentCompany;
        public CompanyRestore(CurrentCompany currentCompany, Guid? companyId, string companyName = null)
        {
            _currentCompany = currentCompany;
            _copmanyId = companyId;
            _companyName = companyName;
        }
        public void Dispose()
        {
            _currentCompany._currentCompanyCacheItem.Value = new CompanyCacheItem(_hasAccessToCurrentCompany, _copmanyId, _companyName);
        }
    }
    private class NullCompanyRestore : IDisposable
    {
        public static readonly NullCompanyRestore Instance = new NullCompanyRestore();
        private NullCompanyRestore()
        {
        }
        public void Dispose()
        {
        }
    }
}
