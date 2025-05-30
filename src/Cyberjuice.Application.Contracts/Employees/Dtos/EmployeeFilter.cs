using Volo.Abp.Application.Dtos;

namespace Cyberjuice.Employees.Dtos;

public class EmployeeFilter : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }
}