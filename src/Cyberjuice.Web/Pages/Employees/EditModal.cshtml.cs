using Cyberjuice.Employees;
using Cyberjuice.Employees.Dtos;
using Cyberjuice.Companies;
using Cyberjuice.Companies.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace Cyberjuice.Web.Pages.Employees;

public class EditModalModel : AbpPageModel
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public CreateUpdateEmployeeInput Employee { get; set; }
    
    public List<SelectListItem> Companies { get; set; } = new List<SelectListItem>();

    private readonly IEmployeeAppService _employeeAppService;
    private readonly ICompanyAppService _companyAppService;

    public EditModalModel(IEmployeeAppService employeeAppService, ICompanyAppService companyAppService)
    {
        _employeeAppService = employeeAppService;
        _companyAppService = companyAppService;
    }

    public async Task OnGetAsync()
    {
        var employee = await _employeeAppService.GetAsync(Id);
        
        Employee = new CreateUpdateEmployeeInput
        {
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            DateOfBirth = employee.DateOfBirth,
            CompanyIds = employee.CompanyIds
        };
        
        await LoadCompaniesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _employeeAppService.UpdateAsync(Id, Employee);
        return NoContent();
    }

    private async Task LoadCompaniesAsync()
    {
        var companies = await _companyAppService.GetAllPagedAsync(new PagedAndSortedResultRequestDto { MaxResultCount = 1000 });
        
        Companies = [.. companies.Items.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Name
        })];
    }
} 