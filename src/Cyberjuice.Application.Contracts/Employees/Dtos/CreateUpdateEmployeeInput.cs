using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cyberjuice.Employees.Dtos;

public class CreateUpdateEmployeeInput
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }


    [Required]
    [MinLength(1, ErrorMessage = "Employee must belong to at least one company")]
    public List<Guid> CompanyIds { get; set; } = new List<Guid>();

    public string UserName { get; set; }
}