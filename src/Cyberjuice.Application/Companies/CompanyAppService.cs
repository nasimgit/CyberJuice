using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using System;
using System.Linq;
using Cyberjuice.Companies.Dtos;
using Cyberjuice.Employees;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace Cyberjuice.Companies;

public class CompanyAppService(
    IRepository<Company, Guid> companyRepository,
    IRepository<CompanyEmployee> employeeCompanyRepo)
    : ApplicationService, ICompanyAppService
{
    public async Task<CompanyDto> CreateAsync(string name)
    {
        var company = new Company { Name = name };

        var createdCompany = await companyRepository.InsertAsync(company);

        return new CompanyDto { Id = createdCompany.Id, Name = createdCompany.Name };
    }

    public async Task<CompanyDto> UpdateAsync(Guid id, string name)
    {
        var Company = await companyRepository.GetAsync(id);
        Company.Name = name;

        var updatedWorkspace = await companyRepository.UpdateAsync(Company);
        return new CompanyDto { Id = updatedWorkspace.Id, Name = updatedWorkspace.Name };
    }

    public async Task<CompanyDto> GetAsync(Guid id)
    {
        var Company = await companyRepository.GetAsync(id);
        return new CompanyDto { Id = Company.Id, Name = Company.Name };
    }


    public async Task<PagedResultDto<CompanyDto>> GetAllPagedAsync(PagedAndSortedResultRequestDto filter)
    {
        string sortBy = !string.IsNullOrWhiteSpace(filter.Sorting) ? filter.Sorting : nameof(Company.CreationTime);

        var workspaceQueryable = (await companyRepository.GetQueryableAsync()).AsNoTracking();

        var totalCount = await workspaceQueryable.CountAsync();

        var query = from e in workspaceQueryable
                    select new CompanyDto
                    {
                        Id = e.Id,
                        Name = e.Name,
                        CreationTime = e.CreationTime
                    };

        var result = await query
                        .OrderBy(sortBy)
                        .PageBy(filter)
                        .ToListAsync();

        return new PagedResultDto<CompanyDto>(
            totalCount,
            result
        );
    }

    [Authorize]
    public async Task<List<CompanyDto>> GetAllAsync()
    {
        var workspaceQueryable = (await companyRepository.GetQueryableAsync()).AsNoTracking();
        var employeeCompanyQueryable = (await employeeCompanyRepo.GetQueryableAsync()).AsNoTracking();

        var query = from company in workspaceQueryable
                    join ec in employeeCompanyQueryable
                        on company.Id equals ec.CompanyId
                    where ec.EmployeeId == CurrentUser.Id
                    select new CompanyDto
                    {
                        Id = company.Id,
                        Name = company.Name,
                        CreationTime = company.CreationTime
                    };

        return await query.ToListAsync();
    }

    public async Task DeleteAsync(Guid id) => await companyRepository.DeleteAsync(id);

}
