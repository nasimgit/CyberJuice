using System.Threading.Tasks;
using Cyberjuice.Departments;
using Cyberjuice.Departments.Dtos;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace Cyberjuice.Web.Pages.Departments;

public class CreateModalModel : AbpPageModel
{
    [BindProperty]
    public CreateDepartmentDto Department { get; set; }

    private readonly IDepartmentAppService _departmentAppService;

    public CreateModalModel(IDepartmentAppService departmentAppService)
    {
        _departmentAppService = departmentAppService;
    }

    public void OnGet()
    {
        Department = new CreateDepartmentDto();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _departmentAppService.CreateAsync(Department);
        return NoContent();
    }
} 