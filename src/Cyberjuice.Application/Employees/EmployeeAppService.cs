using Cyberjuice.Employees.Dtos;
using Cyberjuice.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace Cyberjuice.Employees;

public class EmployeeAppService(
    IdentityUserManager identityUserManager,
    IRepository<IdentityRole> identityRoles,
    IRepository<Employee, Guid> employeeRepository,
    EmployeeManager employeeManager)
    : ApplicationService, IEmployeeAppService
{
    [Authorize(CyberjuicePermissions.Employees.Default)]
    public async Task<EmployeeDto> GetAsync(Guid id)
    {
        var employeeQueryable = (await employeeRepository.GetQueryableAsync()).AsNoTracking();

        var employeeDto = await employeeQueryable
            .Where(e => e.Id == id)
            .Select(e => new EmployeeDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                PhoneNumber = e.PhoneNumber,
                DateOfBirth = e.DateOfBirth,
                CompanyIds = e.Companies.Select(c => c.Id).ToList()
            }).SingleOrDefaultAsync();

        return employeeDto;
    }


    [Authorize(CyberjuicePermissions.Employees.Default)]
    public async Task<List<EmployeeDto>> GetListAsync()
    {
        var employees = await employeeRepository.GetListAsync(includeDetails: true);
        var employeeDtos = ObjectMapper.Map<List<Employee>, List<EmployeeDto>>(employees);

        // Set company IDs from navigation property
        foreach (var dto in employeeDtos)
        {
            var employee = employees.First(e => e.Id == dto.Id);
            dto.CompanyIds = [.. employee.Companies.Select(ce => ce.Id)];
        }

        return employeeDtos;
    }

    [Authorize(CyberjuicePermissions.Employees.Default)]
    public async Task<PagedResultDto<EmployeeDto>> GetPagedListAsync(EmployeeFilter input)
    {
        string sortBy = !string.IsNullOrWhiteSpace(input.Sorting) ? input.Sorting : nameof(Employee.DateOfBirth);

        var queryable = (await employeeRepository.GetQueryableAsync())
            .Include(e => e.Companies)
            .AsNoTracking();

        // Apply search filter
        if (!string.IsNullOrEmpty(input.Filter))
        {
            queryable = queryable.Where(e =>
                e.FirstName.Contains(input.Filter) ||
                e.LastName.Contains(input.Filter) ||
                e.Email.Contains(input.Filter) ||
                e.PhoneNumber.Contains(input.Filter));
        }

        var totalCount = await queryable.CountAsync();

        var employees = await queryable
            .OrderBy(sortBy)
            .PageBy(input)
            .ToListAsync();

        var employeeDtos = ObjectMapper.Map<List<Employee>, List<EmployeeDto>>(employees);

        // Set company IDs from navigation property
        foreach (var dto in employeeDtos)
        {
            var employee = employees.First(e => e.Id == dto.Id);
            dto.CompanyIds = employee.Companies.Select(ce => ce.Id).ToList();
        }

        return new PagedResultDto<EmployeeDto>(totalCount, employeeDtos);
    }

    [Authorize(CyberjuicePermissions.Employees.Create)]
    public async Task<bool> CreateAsync(CreateUpdateEmployeeInput input)
    {
        var employee = await employeeManager.CreateAsync(
            input.FirstName,
            input.LastName,
            input.Email,
            input.PhoneNumber,
            input.DateOfBirth,
            input.CompanyIds
        );

        await CreateAbpUserAsync(input, employee.Id);

        await employeeRepository.InsertAsync(employee, autoSave: true);

        return true;
    }

    private async Task CreateAbpUserAsync(CreateUpdateEmployeeInput employee, Guid id)
    {
        var user = new IdentityUser(
            id: id,
            userName: employee.UserName,
            email: employee.Email
        );

        var defaultPass = "1q2w3E*";

        (await identityUserManager.CreateAsync(user, defaultPass)).CheckErrors();

        (await identityUserManager.SetLockoutEnabledAsync(user, false)).CheckErrors();

        var defaultRoleList = await (await identityRoles.GetQueryableAsync())
                                        .Where(x => x.IsDefault == true)
                                        .Select(x => x.Name).ToListAsync();

        if (defaultRoleList.Count == 0)
        {
            throw new UserFriendlyException(L["Set a role as default first."]);
        }

        (await identityUserManager.SetRolesAsync(user, defaultRoleList)).CheckErrors();

        user.SetIsActive(true);
        (await identityUserManager.UpdateAsync(user)).CheckErrors();

    }

    [Authorize(CyberjuicePermissions.Employees.Edit)]
    public async Task<bool> UpdateAsync(Guid id, CreateUpdateEmployeeInput input)
    {
        var employee = await (await employeeRepository.GetQueryableAsync())
                            .Include(e => e.Companies)
                            .SingleOrDefaultAsync(e => e.Id == id);

        await employeeManager.UpdateAsync(
            employee,
            input.FirstName,
            input.LastName,
            input.Email,
            input.PhoneNumber,
            input.DateOfBirth,
            input.CompanyIds
        );

        return true;
    }

    [Authorize(CyberjuicePermissions.Employees.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        // CompanyEmployee entities will be automatically deleted due to cascade delete
        await employeeRepository.DeleteAsync(id);
    }

    public async Task<bool> GetEmployeeHasAccessToCompanyAsync(Guid? currentCompanyId)
    {
        var employeeId = CurrentUser.Id;

        if (employeeId is null || currentCompanyId is null)
        {
            return false; // No access if context is incomplete
        }

        var employeeQueryable = await employeeRepository.GetQueryableAsync();

        var hasAccess = await employeeQueryable
            .AsNoTracking()
            .Where(e => e.Id == employeeId)
            .SelectMany(e => e.Companies)
            .AnyAsync(c => c.Id == currentCompanyId.Value);

        return hasAccess;
    }
}
