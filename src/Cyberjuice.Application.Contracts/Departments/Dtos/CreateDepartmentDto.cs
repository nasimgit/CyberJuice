using System;
using System.ComponentModel.DataAnnotations;

namespace Cyberjuice.Departments.Dtos;

public class CreateDepartmentDto
{

    [Required] public string Name { get; set; } = string.Empty;

    [Required] public string Description { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int EmployeeCount { get; set; }
}
