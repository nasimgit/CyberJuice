using System;
using Volo.Abp.Application.Dtos;

namespace Cyberjuice.Departments.Dtos;

public class DepartmentDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
}
