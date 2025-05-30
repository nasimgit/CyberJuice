using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace Cyberjuice.Departments;

public interface IDepartmentManager : IDomainService
{
    Task<Department> CreateAsync(string name, string description, int employeeCount = 0, Guid? companyId = null);
    Task<Department> UpdateAsync(Department department, string name, string description, int employeeCount);
}
