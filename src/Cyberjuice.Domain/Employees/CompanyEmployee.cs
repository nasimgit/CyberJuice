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
