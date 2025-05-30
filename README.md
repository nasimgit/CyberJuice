# Cyberjuice - Multi-Company ABP Application

## About this solution

This is a layered startup solution based on [Domain Driven Design (DDD)](https://abp.io/docs/latest/framework/architecture/domain-driven-design) practises with **Multi-Company Architecture**. All the fundamental ABP modules are already installed. Check the [Application Startup Template](https://abp.io/docs/latest/solution-templates/layered-web-application) documentation for more info.

## Multi-Company Architecture

This application implements a sophisticated multi-company system where:
- **Companies** are the main organizational units
- **Employees** can belong to multiple companies (many-to-many relationship)
- **Departments** belong to a single company (one-to-many relationship)
- **Data filtering** is automatically applied based on the current company context
- **Company switching** is handled via HTTP headers and middleware

### Core Entities

#### 1. Company Entity
```csharp
public class Company : AuditedAggregateRoot<Guid>
{
    public string Name { get; set; }
}
```

#### 2. Employee Entity (Many-to-Many with Companies)
```csharp
public class Employee : FullAuditedAggregateRoot<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    // ... other properties

    // Navigation property for many-to-many relationship
    public virtual ICollection<CompanyEmployee> CompanyEmployees { get; set; } = new List<CompanyEmployee>();

    public void UpdateCompanies(IEnumerable<Guid> companyIds)
    {
        CompanyEmployees.Clear();
        foreach (var companyId in companyIds)
        {
            CompanyEmployees.Add(new CompanyEmployee(Id, companyId));
        }
    }
}
```

#### 3. Department Entity (Company-Specific)
```csharp
public class Department : FullAuditedAggregateRoot<Guid>, ICompany
{
    public Guid? CompanyId { get; set; }  // Required by ICompany interface
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }

    public Department(
        Guid id,
        string name,
        string description,
        int employeeCount = 0,
        Guid? companyId = null
    ) : base(id)
    {
        SetName(name);
        SetDescription(description);
        SetEmployeeCount(employeeCount);
        CompanyId = companyId; // Automatically set by middleware
    }
}
```

#### 4. ICompany Interface
```csharp
public interface ICompany
{
    Guid? CompanyId { get; set; }
}
```

### Multi-Company Data Filtering System

#### 1. Client-Side Company Selection
The application header contains a company dropdown that switches the current company context:

**HTTP Interceptor (http-interceptor.js)**
```javascript
function initJQueryInterceptor() {
    if (!window.jQuery) return;

    jQuery(document).ajaxSend(function (event, xhr, settings) {
        const workspaceId = getCompanyId(); // From localStorage
        if (workspaceId) {
            xhr.setRequestHeader('X-Company-Id', workspaceId);
        }
    });
}
```

#### 2. Server-Side Company Resolution
**CompanyResolutionMiddleware.cs**
```csharp
public class CompanyResolutionMiddleware : IMiddleware, ITransientDependency
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var companyResolveContext = new CompanyResolveContext(context);

        foreach (var companyResolver in _options.CompanyResolvers)
        {
            await companyResolver.ResolveAsync(companyResolveContext);
            if (companyResolveContext.CompanyId.HasValue)
            {
                logger.LogDebug($"Company resolved: {companyResolveContext.CompanyId}");
                break;
            }
        }

        if (companyResolveContext.CompanyId.HasValue)
        {
            using (currentCompany.Change(companyResolveContext.CompanyId.Value))
            {
                await next(context); // All subsequent operations use this company context
            }
        }
        else
        {
            await next(context);
        }
    }
}
```

**CompanyIdHeaderResolveContributor.cs**
```csharp
public class CompanyIdHeaderResolveContributor : ICompanyResolveContributor
{
    public const string HeaderName = "X-Company-Id";

    public Task ResolveAsync(ICompanyResolveContext context)
    {
        var httpContext = context.GetHttpContext();
        var companyIdHeader = httpContext.Request.Headers[HeaderName];
        
        if (Guid.TryParse(companyIdHeader[0], out var companyId))
        {
            context.CompanyId = companyId;
        }
        return Task.CompletedTask;
    }
}
```

#### 3. Current Company Management
**CurrentCompany.cs**
```csharp
public class CurrentCompany : ICurrentCompany, ISingletonDependency
{
    private readonly AsyncLocal<CompanyCacheItem> _currentCompanyCacheItem = new();

    public virtual Guid? Id => _currentCompanyCacheItem.Value?.CompanyId;
    public virtual string Name => _currentCompanyCacheItem.Value?.Name;
    public virtual bool IsAvailable => Id.HasValue;

    public virtual IDisposable Change(Guid? id, string name = null)
    {
        var previousId = _currentCompanyCacheItem.Value?.CompanyId;
        var previousName = _currentCompanyCacheItem.Value?.Name;
        
        _currentCompanyCacheItem.Value = new CompanyCacheItem(id, name);
        return new CompanyRestore(this, previousId, previousName);
    }
}
```

#### 4. Entity Framework Data Filtering
**CompanyDbContextBase.cs**
```csharp
public abstract class CompanyDbContextBase<TSelf> : AbpDbContext<TSelf>
    where TSelf : DbContext
{
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyCurrentCompanyId(); // Automatically set CompanyId on save
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyCurrentCompanyId()
    {
        if (CurrentCompany?.Id == null) return;

        var currentCompanyId = CurrentCompany.Id.Value;

        foreach (var entry in ChangeTracker.Entries()
            .Where(e => e.Entity is ICompany && 
                       (e.State == EntityState.Added || e.State == EntityState.Modified)))
        {
            // Automatically stamp the CompanyId
            entry.Property(nameof(ICompany.CompanyId)).CurrentValue = currentCompanyId;

            if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(ICompany.CompanyId)).IsModified = false;
            }
        }
    }

    protected override Expression<Func<TEntity, bool>> CreateFilterExpression<TEntity>(ModelBuilder modelBuilder)
    {
        var baseExpression = base.CreateFilterExpression<TEntity>(modelBuilder);

        if (!typeof(ICompany).IsAssignableFrom(typeof(TEntity)))
        {
            return baseExpression;
        }

        // Automatic filtering for ICompany entities
        Expression<Func<TEntity, bool>> companyFilter = e =>
            !IsMultiCompanyFilterEnabled
            || CurrentCompany.Id == null
            || EF.Property<Guid?>(e, nameof(ICompany.CompanyId)) == CurrentCompany.Id;

        return baseExpression == null ? companyFilter : 
               QueryFilterExpressionHelper.CombineExpressions(baseExpression, companyFilter);
    }
}
```

### How It Works: Complete Flow Example

#### 1. User Switches Company
```javascript
// User selects company from dropdown
localStorage.setItem('selectedCompanyId', '12345678-1234-1234-1234-123456789012');
// Page refresh or AJAX request automatically includes X-Company-Id header
```

#### 2. Server Processes Request
```csharp
// 1. CompanyResolutionMiddleware extracts company ID from header
// 2. Sets CurrentCompany context for the request
// 3. All subsequent operations use this company context
```

#### 3. Department CRUD Operations
```csharp
// Create Department
public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto input)
{
    var department = await _departmentManager.CreateAsync(
        input.Name,
        input.Description,
        input.EmployeeCount,
        _currentCompany.Id  // Automatically set from context
    );

    var createdDepartment = await _departmentRepository.InsertAsync(department);
    // ApplyCurrentCompanyId() automatically sets CompanyId during save
    
    return ObjectMapper.Map<Department, DepartmentDto>(createdDepartment);
}

// Query Departments (automatically filtered by current company)
public async Task<PagedResultDto<DepartmentDto>> GetListAsync(PagedAndSortedResultRequestDto input)
{
    var queryable = (await _departmentRepository.GetQueryableAsync())
        .AsNoTracking()
        .Where(d => d.CompanyId == _currentCompany.Id); // Automatic filtering
        
    // Only departments for current company are returned
}
```

#### 4. Employee Multi-Company Management
```csharp
// Employees can belong to multiple companies
public async Task<EmployeeDto> CreateAsync(CreateUpdateEmployeeInput input)
{
    var employee = await _employeeManager.CreateAsync(
        input.FirstName,
        input.LastName,
        input.Email,
        // ... other properties
        input.CompanyIds  // Multiple company assignments
    );
    
    // CompanyEmployee junction table automatically managed
}

// Query employees for current company only
var queryable = (await _employeeRepository.GetQueryableAsync())
    .Include(e => e.CompanyEmployees)
    .Where(e => e.CompanyEmployees.Any(ce => ce.CompanyId == _currentCompany.Id.Value));
```

### Key Benefits

1. **Automatic Data Isolation**: Entities implementing `ICompany` are automatically filtered by current company
2. **Seamless Company Switching**: Users can switch companies without losing context
3. **Multi-Company Employees**: Employees can work across multiple companies
4. **Company-Specific Departments**: Each company has its own departments
5. **DDD Compliance**: Proper domain modeling with aggregate roots and domain services

### Architecture Patterns Used

- **Multi-Tenancy Pattern**: Company-based data isolation
- **Domain Driven Design**: Aggregate roots, domain services, and business logic encapsulation
- **Repository Pattern**: Data access abstraction
- **Middleware Pattern**: Request processing pipeline
- **Strategy Pattern**: Multiple company resolution strategies

## Pre-requirements

* [.NET9.0+ SDK](https://dotnet.microsoft.com/download/dotnet)
* [Node v18 or 20](https://nodejs.org/en)

## Configurations

The solution comes with a default configuration that works out of the box. However, you may consider to change the following configuration before running your solution:

* Check the `ConnectionStrings` in `appsettings.json` files under the `Cyberjuice.Web` and `Cyberjuice.DbMigrator` projects and change it if you need.

## Before running the application

* Run `abp install-libs` command on your solution folder to install client-side package dependencies.
* Run `Cyberjuice.DbMigrator` to create the initial database and seed data.

### Generating a Signing Certificate

In the production environment, you need to use a production signing certificate:

```bash
dotnet dev-certs https -v -ep openiddict.pfx -p 5f7e981d-b5bc-42a1-9ef9-2b6b95752b21
```

## Solution structure

This is a layered monolith application that consists of the following applications:

* `Cyberjuice.DbMigrator`: Console application for database migrations and seeding
* `Cyberjuice.Web`: ASP.NET Core MVC / Razor Pages web application

## Additional resources

* [Web Application Development Tutorial](https://abp.io/docs/latest/tutorials/book-store/part-1)
* [Application Startup Template](https://abp.io/docs/latest/startup-templates/application/index)
* [ABP Framework Documentation](https://docs.abp.io/)
