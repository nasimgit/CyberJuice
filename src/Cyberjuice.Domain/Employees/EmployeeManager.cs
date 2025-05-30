using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp;
using Cyberjuice.Companies;

namespace Cyberjuice.Employees;

public class EmployeeManager(
    IRepository<Employee, Guid> employeeRepository,
    IRepository<Company, Guid> companyRepository) : DomainService
{
    public async Task<Employee> CreateAsync(
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        DateTime dateOfBirth,
        IEnumerable<Guid> companyIds)
    {
        await CheckEmailNotExistsAsync(email);
        await ValidateCompaniesExistAsync(companyIds);

        var employee = new Employee(
            GuidGenerator.Create(),
            firstName,
            lastName,
            email,
            phoneNumber,
            dateOfBirth);

        // Add companies to employee
        var companies = await companyRepository.GetListAsync(c => companyIds.Contains(c.Id));
        employee.UpdateCompanies(companies);

        return employee;
    }

    public async Task<Employee> UpdateAsync(
        Employee employee,
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        DateTime dateOfBirth,
        IEnumerable<Guid> companyIds)
    {
        Check.NotNull(employee, nameof(employee));

        if (employee.Email != email)
        {
            await CheckEmailNotExistsAsync(email, employee.Id);
        }

        await ValidateCompaniesExistAsync(companyIds);

        employee
            .SetFirstName(firstName)
            .SetLastName(lastName)
            .SetEmail(email)
            .SetPhoneNumber(phoneNumber);

        employee.DateOfBirth = dateOfBirth;

        // Update company assignments
        var companies = await companyRepository.GetListAsync(c => companyIds.Contains(c.Id));
        employee.UpdateCompanies(companies);

        return employee;
    }

    private async Task CheckEmailNotExistsAsync(string email, Guid? excludeId = null)
    {
        var existingEmployee = await employeeRepository.FindAsync(x => 
            x.Email == email && 
            (excludeId == null || x.Id != excludeId));

        if (existingEmployee != null)
        {
            throw new BusinessException(CyberjuiceDomainErrorCodes.EmployeeEmailAlreadyExists)
                .WithData("Email", email);
        }
    }

    private async Task ValidateCompaniesExistAsync(IEnumerable<Guid> companyIds)
    {
        if (!companyIds.Any())
        {
            throw new BusinessException(CyberjuiceDomainErrorCodes.EmployeeMustBelongToAtLeastOneCompany);
        }

        foreach (var companyId in companyIds)
        {
            var companyExists = await companyRepository.AnyAsync(c => c.Id == companyId);
            if (!companyExists)
            {
                throw new BusinessException(CyberjuiceDomainErrorCodes.CompanyNotFound)
                    .WithData("CompanyId", companyId);
            }
        }
    }
}
