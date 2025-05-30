using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp;

namespace Cyberjuice.Departments;

public class DepartmentManager(IRepository<Department, Guid> departmentRepository) 
    : DomainService, IDepartmentManager
{
    public async Task<Department> CreateAsync(
        string name,
        string description,
        int employeeCount = 0,
        Guid? companyId = null)
    {
        await CheckDepartmentNameNotExistsAsync(name, companyId);

        return new Department(
            GuidGenerator.Create(),
            name,
            description,
            employeeCount);
    }

    public async Task<Department> UpdateAsync(
        Department department,
        string name,
        string description,
        int employeeCount)
    {
        Check.NotNull(department, nameof(department));

        if (department.Name != name)
        {
            await CheckDepartmentNameNotExistsAsync(name, department.CompanyId, department.Id);
        }

        department
            .SetName(name)
            .SetDescription(description)
            .SetEmployeeCount(employeeCount);

        return department;
    }

    private async Task CheckDepartmentNameNotExistsAsync(string name, Guid? companyId, Guid? excludeId = null)
    {
        var existingDepartment = await departmentRepository.FindAsync(x => 
            x.Name == name && 
            x.CompanyId == companyId && 
            (excludeId == null || x.Id != excludeId));

        if (existingDepartment != null)
        {
            throw new BusinessException(CyberjuiceDomainErrorCodes.DepartmentNameAlreadyExists)
                .WithData("Name", name);
        }
    }
}
