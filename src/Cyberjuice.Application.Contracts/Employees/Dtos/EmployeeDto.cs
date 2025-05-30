using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Cyberjuice.Employees.Dtos;

public class EmployeeDto : FullAuditedEntityDto<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public List<Guid> CompanyIds { get; set; } = [];
}
