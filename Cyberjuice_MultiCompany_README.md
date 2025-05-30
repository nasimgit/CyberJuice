# Cyberjuice Multi-Company User Access Upgrade

## Executive Summary

This document outlines the solution for enabling **users (emails) to manage multiple companies** in the existing Cyberjuice multi-company application. 

## System Overview

The current Cyberjuice application already has:

- **Companies**: Core organizational units
- **Employees**: Can belong to multiple companies (many-to-many via `CompanyEmployee` table)
- **Company Context Switching**: Via HTTP headers and middleware
- **Data Filtering**: Automatic company-scoped filtering using `ICompany` interface
- **Frontend Company Selector**: JavaScript-based company switching

---

## 🔧 1. Database Design

### Entities

#### Company Entity
```csharp
using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Cyberjuice.Companies;

public class Company : AuditedAggregateRoot<Guid>
{
    public string Name { get; set; }
}
```

#### Employee Entity (Multi-Company Support)
```csharp
using Cyberjuice.Companies;
using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Cyberjuice.Employees;

public class Employee : FullAuditedAggregateRoot<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }

    // Navigation property for many-to-many relationship
    public virtual List<Company> Companies { get; set; } = [];

    protected Employee()
    {
        // Required by EF Core
    }

    public Employee(
        Guid id,
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        DateTime dateOfBirth
    ) : base(id)
    {
        SetFirstName(firstName);
        SetLastName(lastName);
        SetEmail(email);
        SetPhoneNumber(phoneNumber);
        DateOfBirth = dateOfBirth;
        Companies = [];
    }

    public Employee SetFirstName(string firstName)
    {
        FirstName = Check.NotNullOrWhiteSpace(firstName, nameof(firstName), EmployeeConsts.MaxFirstNameLength);
        return this;
    }

    public Employee SetLastName(string lastName)
    {
        LastName = Check.NotNullOrWhiteSpace(lastName, nameof(lastName), EmployeeConsts.MaxLastNameLength);
        return this;
    }

    public Employee SetEmail(string email)
    {
        Email = Check.NotNullOrWhiteSpace(email, nameof(email), EmployeeConsts.MaxEmailLength);
        return this;
    }

    public Employee SetPhoneNumber(string phoneNumber)
    {
        PhoneNumber = Check.NotNullOrWhiteSpace(phoneNumber, nameof(phoneNumber), EmployeeConsts.MaxPhoneNumberLength);
        return this;
    }
 
    public void UpdateCompanies(IEnumerable<Company> companies)
    {
        // Clear existing company assignments
        Companies.Clear();

        // Add new company assignments
        Companies.AddRange(companies);
    }
}
```

#### Department Entity (Company-Scoped)
```csharp
using Cyberjuice.Companies;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Cyberjuice.Departments;

public class Department : FullAuditedAggregateRoot<Guid>, ICompany
{
    public Guid? CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int EmployeeCount { get; private set; }

    protected Department() { } // Required by EF Core

    public Department(
        Guid id,
        string name,
        string description,
        int employeeCount = 0
    ) : base(id)
    {
        SetName(name);
        SetDescription(description);
        SetEmployeeCount(employeeCount);
    }

    public Department SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), DepartmentConsts.MaxNameLength);
        return this;
    }

    public Department SetDescription(string description)
    {
        Description = Check.Length(description, nameof(description), DepartmentConsts.MaxDescriptionLength);
        return this;
    }

    public Department SetEmployeeCount(int employeeCount)
    {
        if (employeeCount < 0)
        {
            throw new ArgumentException("Employee count cannot be negative.", nameof(employeeCount));
        }
        EmployeeCount = employeeCount;
        return this;
    }

    public void IncrementEmployeeCount()
    {
        EmployeeCount++;
    }

    public void DecrementEmployeeCount()
    {
        if (EmployeeCount > 0)
        {
            EmployeeCount--;
        }
    }
}
```

#### CompanyEmployee Junction Table
```csharp
using Cyberjuice.Companies;
using System;
using Volo.Abp.Domain.Entities;

namespace Cyberjuice.Employees;

public class CompanyEmployee : Entity
{
    public Guid EmployeeId { get; set; }
    public Guid CompanyId { get; set; }
    
    protected CompanyEmployee() {}

    public CompanyEmployee(Guid employeeId, Guid companyId)
    {
        EmployeeId = employeeId;
        CompanyId = companyId;
    }

    public override object[] GetKeys()
    {
        return [EmployeeId, CompanyId];
    }
}
```

#### ICompany Interface (Company-Scoped Filtering)
```csharp
using System;

namespace Cyberjuice.Companies;

public interface ICompany
{
    Guid? CompanyId { get; set; }
}
```

### Database Schema Summary

```sql
-- Current Schema (Already Multi-Company Ready)
IdentityUser (1:1) → Employee (M:N) → Companies
                         ↓
                    CompanyEmployee (Junction Table)
```

**Key Insights:**
- ✅ **Employee-Company Many-to-Many** relationship already exists
- ✅ **Company filtering** via `ICompany` interface already implemented
- ✅ **Data isolation** automatically handled by existing middleware
- ✅ **No major schema changes** required

---

## 🛠️ 2. Backend Impact

### Minimal Backend Changes Required

The existing backend architecture already supports multi-company user access. The key component is the **ICompany interface implementation**:

#### ICompany Interface (Current Implementation)
```csharp
using System;

namespace Cyberjuice.Companies;

public interface ICompany
{
    Guid? CompanyId { get; set; }
}
```

**How it Works:**
1. **Automatic Data Filtering**: All entities implementing `ICompany` are automatically filtered by the current company context
2. **Company Stamping**: When saving entities, the current company ID is automatically applied
3. **Data Isolation**: Users only see data for their current company context

#### Current Backend Services (No Changes Needed)
- **CompanyDbContextBase**: Already handles automatic filtering and stamping

### Backend Impact Assessment
| Component | Change Required | Status |
|-----------|----------------|--------|
| Entity Models | None | ✅ Ready |
| Data Filtering | None | ✅ Implemented |
| Company Context | None | ✅ Working |
| Authorization | None | ✅ Company-scoped |
| Application Services | None | ✅ Multi-company aware |

---

## 🧭 3. Company Context Handling

### Current Implementation (Already Working)

#### Frontend HTTP Interceptor
```javascript
/**
 * Company HTTP Interceptor
 * Automatically includes the Company id in API requests
 */
(function () {
    'use strict';

    const CONFIG = window.WORKSPACE_CONSTANTS || {
        STORAGE_KEY: 'selectedCompanyId',
        HEADER_NAME: 'X-Company-Id'
    };

    function getCompanyId() {
        return localStorage.getItem(CONFIG.STORAGE_KEY);
    }

    function initJQueryInterceptor() {
        if (!window.jQuery) return;

        jQuery(document).ajaxSend(function (event, xhr, settings) {
            const workspaceId = getCompanyId();

            if (workspaceId) {
                xhr.setRequestHeader(CONFIG.HEADER_NAME, workspaceId);
            }
        });
    }

    function initialize() {
        initJQueryInterceptor();
    }

    // Trigger initialization immediately and on key events
    initialize();

    document.addEventListener('abp.dynamicScriptsInitialized', initialize);
    document.addEventListener('DOMContentLoaded', initialize);
    window.addEventListener('load', initialize);

})();
```

**How it Works:**
- ✅ **Automatic Header Injection**: Every AJAX request includes `X-Company-Id` header
- ✅ **Local Storage**: Company selection persisted across sessions
- ✅ **Universal Coverage**: Works with all jQuery AJAX requests

#### Company Header Resolution
```csharp
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Cyberjuice.Companies;

/// <summary>
/// Resolves Company from the X-Company-Id header in HTTP requests
/// </summary>
public class CompanyIdHeaderResolveContributor : ICompanyResolveContributor, ITransientDependency
{
    /// <summary>
    /// Default header name: X-Company-ID.
    /// </summary>
    public const string HeaderName = "x-company-id";

    /// <summary>
    /// Default contributor name: CompanyIdHeader.
    /// </summary>
    public const string ContributorName = "CompanyIdHeader";
    /// <summary>
    /// Name of the contributor.
    /// </summary>
    public string Name => ContributorName;
    private readonly ILogger<CompanyIdHeaderResolveContributor> _logger;
    public CompanyIdHeaderResolveContributor(ILogger<CompanyIdHeaderResolveContributor> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Tries to resolve current Company from HTTP header.
    /// </summary>
    public Task ResolveAsync(ICompanyResolveContext context)
    {
        var httpContext = context.GetHttpContext();
        if (httpContext == null)
        {
            return Task.CompletedTask;
        }
        var WorkspaceIdHeader = httpContext.Request.Headers[HeaderName];
        if (WorkspaceIdHeader.Count == 0 || string.IsNullOrWhiteSpace(WorkspaceIdHeader[0]))
        {
            return Task.CompletedTask;
        }
        if (Guid.TryParse(WorkspaceIdHeader[0], out var workspaceId))
        {
            _logger.LogDebug($"Company Id found in request header: {workspaceId}");
            context.CompanyId = workspaceId;
        }
        else
        {
            _logger.LogDebug($"Invalid Company Id format in request header: {WorkspaceIdHeader[0]}");
        }
        return Task.CompletedTask;
    }
}
```

**How it Works:**
- ✅ **Header Parsing**: Extracts company ID from `X-Company-Id` header
- ✅ **Context Setting**: Sets company context for the request

#### Company Resolution Middleware
```csharp
using Cyberjuice.Employees;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Cyberjuice.Companies;

public class CompanyResolutionMiddleWare(
    IOptions<CompanyResolveOptions> options,
    IEmployeeAppService employeeAppService,
    ILogger<CompanyResolutionMiddleWare> logger,
    ICurrentCompany currentCompany)
    : IMiddleware, ITransientDependency
{
    private readonly CompanyResolveOptions _options = options.Value;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var companyResolveContext = new CompanyResolveContext(context);

        foreach (var companyResolver in _options.CompanyResolvers)
        {
            await companyResolver.ResolveAsync(companyResolveContext);

            if (companyResolveContext.CompanyId.HasValue)
            {
                logger.LogDebug($"Company resolved by {companyResolver.Name}: {companyResolveContext.CompanyId}");
                break;
            }
        }
        if (companyResolveContext.CompanyId.HasValue)
        {
            // Set current Company using scoped ICurrentWorkspace service
            bool hasAccessToCompany = await employeeAppService.GetEmployeeHasAccessToCompanyAsync(companyResolveContext.CompanyId);
            using (currentCompany.Change(hasAccessToCompany, companyResolveContext.CompanyId.Value, companyResolveContext.CompanyName))
            {
                await next(context);
            }
        }
        else
        {
            await next(context);
        }
    }
}
```

**How it Works:**
- ✅ **Multi-Strategy Resolution**: Uses multiple resolvers (header, query, etc.)
- ✅ **Access Validation**: Checks if user has access to resolved company
- ✅ **Context Management**: Sets company context for entire request pipeline
- ✅ **Security**: Only allows access to companies user belongs to

### Complete Flow Explanation

#### 1. User Company Switching Flow
```
1. User selects company from dropdown (Frontend)
   ↓
2. JavaScript stores company ID in localStorage
   ↓ 
3. Page refresh triggers new request with X-Company-Id header
   ↓
4. CompanyIdHeaderResolveContributor extracts company ID
   ↓
5. CompanyResolutionMiddleware validates user access
   ↓
6. CurrentCompany context set for request
   ↓
7. All subsequent operations filtered by company
```

#### 2. Data Access Flow
```
1. User makes API request (e.g., get departments)
   ↓
2. HTTP Interceptor adds X-Company-Id header
   ↓
3. Middleware resolves and validates company access
   ↓
4. CompanyDbContextBase applies ICompany filtering
   ↓
5. Only company-specific data returned
```

#### 3. Multi-Company User Scenario
```javascript
// Example: User manages 4 companies
const userCompanies = [
    { id: "company-a-id", name: "Company A" },
    { id: "company-b-id", name: "Company B" }, 
    { id: "company-c-id", name: "Company C" },
    { id: "company-d-id", name: "Company D" }
];

// User switches to Company B
localStorage.setItem('selectedCompanyId', 'company-b-id');
location.reload(); // All data now filtered for Company B
```

---

## 🚀 4. Safe Rollout Strategy

### Minimal Database Changes Required

The current database schema supports multi-company users. Only **data treatment** is needed for existing records:

#### Current Database State Assessment (Example)
```sql
-- Check current employee-company relationships
SELECT 
    e.Email,
    COUNT(ce.CompanyId) as CompanyCount,
    STRING_AGG(c.Name, ', ') as Companies
FROM CyberjuiceEmployees e
LEFT JOIN CompanyEmployee ce ON e.Id = ce.EmployeeId
LEFT JOIN CyberjuiceCompanies c ON ce.CompanyId = c.Id
WHERE e.Id IN (SELECT Id FROM AbpUsers)
GROUP BY e.Id, e.Email;
```

#### Data Treatment Migration (Optional)
```csharp
public partial class TreatExistingMultiCompanyData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Optional: Ensure all users have at least one company assignment
        migrationBuilder.Sql(@"
            -- Find users without company assignments
            INSERT INTO CompanyEmployee (EmployeeId, CompanyId)
            SELECT 
                u.Id as EmployeeId,
                (SELECT TOP 1 Id FROM CyberjuiceCompanies) as CompanyId
            FROM AbpUsers u
            WHERE u.Id NOT IN (
                SELECT DISTINCT ce.EmployeeId 
                FROM CompanyEmployee ce
            )
            AND u.Id IN (SELECT Id FROM CyberjuiceEmployees)
        ");
    }
}
```

### Rollout Strategy

#### Phase 1: Validation (Week 1)
```bash
# 1. Validate current system supports multi-company users
# 2. Test existing company switching functionality
# 3. Verify data isolation works correctly

# No deployment changes needed - system already works!
```

#### Phase 2: Documentation & Training (Week 1)
```markdown
# User Guide: Managing Multiple Companies

1. **Company Selection**: Use dropdown in header to switch companies
2. **Data Isolation**: Each company's data is completely separated  
3. **Access Control**: You only see companies you're assigned to
4. **Switching**: All data updates when you switch companies
```

#### Phase 3: User Onboarding (Ongoing)
```csharp
// Add users to multiple companies (existing functionality)
public async Task AddUserToMultipleCompaniesAsync(Guid userId, List<Guid> companyIds)
{
    var employee = await _employeeRepository.GetAsync(userId);
    var companies = await _companyRepository.GetListAsync(c => companyIds.Contains(c.Id));
    
    employee.UpdateCompanies(companies);
    await _employeeRepository.UpdateAsync(employee);
}
```

### Implementation Checklist

| Task | Status | Notes |
|------|--------|-------|
| ✅ Database Schema | Ready | Employee-Company M:N exists |
| ✅ Company Filtering | Working | ICompany interface implemented |
| ✅ Company Switching | Working | Frontend + middleware ready |
| ✅ Data Isolation | Working | Automatic filtering active |
| ✅ Access Control | Working | User-company validation exists |
| 📋 User Training | Needed | Document multi-company usage |
| 📋 Data Validation | Optional | Ensure all users have companies |

---

## Enabling Multi-Company Access Today

### For New Users
```csharp
// Creating employee with multiple companies (already supported)
var employee = await _employeeManager.CreateAsync(
    "John",
    "Doe", 
    "john@example.com",
    "+1234567890",
    DateTime.Now.AddYears(-30),
    new[] { companyAId, companyBId, companyCId, companyDId } // 4 companies!
);
```

### For Existing Users
```csharp
// Adding existing user to additional companies
var employee = await _employeeRepository.GetAsync(userId);
var additionalCompanies = await _companyRepository.GetListAsync(c => 
    newCompanyIds.Contains(c.Id));

var allCompanies = employee.Companies.Concat(additionalCompanies);
employee.UpdateCompanies(allCompanies);
await _employeeRepository.UpdateAsync(employee);
```

### User Experience
1. **Login**: User logs in normally
2. **Company Selection**: Company dropdown shows all accessible companies
3. **Switch Companies**: Click dropdown, select different company
4. **Isolated Data**: All data (departments, employees, etc.) filtered by selected company
5. **Secure Access**: Only companies user belongs to are accessible

---

## Conclusion

**The Cyberjuice application supports users managing multiple companies!** 

### Capabilities ✅
- ✅ **Multi-Company User Access**: Via Employee-Company relationship
- ✅ **Company Switching**: Frontend dropdown with backend validation  
- ✅ **Data Isolation**: Automatic filtering via ICompany interface
- ✅ **Security**: User can only access assigned companies
- ✅ **Scalability**: Supports unlimited companies per user
 