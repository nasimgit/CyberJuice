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
