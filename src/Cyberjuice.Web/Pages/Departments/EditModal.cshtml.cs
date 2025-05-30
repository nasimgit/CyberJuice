using Cyberjuice.Departments;
using Cyberjuice.Departments.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace Cyberjuice.Web.Pages.Departments;

public class EditModalModel : AbpPageModel
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public UpdateDepartmentDto Department { get; set; }

    private readonly IDepartmentAppService _departmentAppService;

    public EditModalModel(IDepartmentAppService departmentAppService)
    {
        _departmentAppService = departmentAppService;
    }

    public async Task OnGetAsync()
    {
        var department = await _departmentAppService.GetAsync(Id);
        
        Department = new UpdateDepartmentDto
        {
            Name = department.Name,
            Description = department.Description,
            EmployeeCount = department.EmployeeCount
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _departmentAppService.UpdateAsync(Id, Department);
        return NoContent();
    }
} 