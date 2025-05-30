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
