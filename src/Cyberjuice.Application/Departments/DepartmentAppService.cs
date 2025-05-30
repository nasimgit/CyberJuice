using Cyberjuice.Departments.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Cyberjuice.Departments;

public class DepartmentAppService(
    IRepository<Department, Guid> departmentRepository,
    DepartmentManager departmentManager) 
    : ApplicationService, IDepartmentAppService
{

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto input)
    {
        var department = await departmentManager.CreateAsync(
            input.Name,
            input.Description,
            input.EmployeeCount
        );

        var createdDepartment = await departmentRepository.InsertAsync(department);

        return ObjectMapper.Map<Department, DepartmentDto>(createdDepartment);
    }

    public async Task<DepartmentDto> UpdateAsync(Guid id, UpdateDepartmentDto input)
    {
        var department = await departmentRepository.GetAsync(id);

        await departmentManager.UpdateAsync(
            department,
            input.Name,
            input.Description,
            input.EmployeeCount
        );

        var updatedDepartment = await departmentRepository.UpdateAsync(department);

        return ObjectMapper.Map<Department, DepartmentDto>(updatedDepartment);
    }

    public async Task<DepartmentDto> GetAsync(Guid id)
    {
        var department = await departmentRepository.GetAsync(id);
        return ObjectMapper.Map<Department, DepartmentDto>(department);
    }

    public async Task<PagedResultDto<DepartmentDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        string sortBy = !string.IsNullOrWhiteSpace(input.Sorting) ? input.Sorting : nameof(Department.CreationTime);

        var queryable = (await departmentRepository.GetQueryableAsync()).AsNoTracking();

        var totalCount = await queryable.CountAsync();

        var query = from d in queryable
                    select new DepartmentDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Description = d.Description,
                        EmployeeCount = d.EmployeeCount,
                        CreationTime = d.CreationTime,
                        CreatorId = d.CreatorId,
                        LastModificationTime = d.LastModificationTime,
                        LastModifierId = d.LastModifierId,
                        IsDeleted = d.IsDeleted,
                        DeleterId = d.DeleterId,
                        DeletionTime = d.DeletionTime
                    };

        var result = await query
            .OrderBy(sortBy)
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<DepartmentDto>(totalCount, result);
    }

    public async Task DeleteAsync(Guid id)
    {
        await departmentRepository.DeleteAsync(id);
    }
}
