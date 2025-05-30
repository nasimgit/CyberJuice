using AutoMapper;
using Cyberjuice.Companies;
using Cyberjuice.Employees;
using Cyberjuice.Employees.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;
using Volo.Abp.Timing;

namespace Cyberjuice.Web.Pages.Employees;

public class CreateModalModel : AbpPageModel
{
    [BindProperty]
    public CreateEmployeeModel ViewModel { get; set; }

    public List<SelectListItem> Companies { get; set; } = new List<SelectListItem>();

    private readonly IEmployeeAppService _employeeAppService;
    private readonly ICompanyAppService _companyAppService;

    public CreateModalModel(IEmployeeAppService employeeAppService, ICompanyAppService companyAppService)
    {
        _employeeAppService = employeeAppService;
        _companyAppService = companyAppService;
    }

    public async Task OnGetAsync()
    {
        ViewModel = new();
        await LoadCompaniesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var createInput = ObjectMapper.Map<CreateEmployeeModel, CreateUpdateEmployeeInput>(ViewModel);

        await _employeeAppService.CreateAsync(createInput);
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

    [AutoMap(typeof(CreateUpdateEmployeeInput), ReverseMap = true)]
    public class CreateEmployeeModel
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
        public DateTime DateOfBirth { get; set; } = DateTime.Now;

        [Required]
        [MinLength(1, ErrorMessage = "Employee must belong to at least one company")]
        public List<Guid> CompanyIds { get; set; } = [];

        [Required(ErrorMessage = "UserName is required.")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "UserName can only contain letters and digits.")]
        public string UserName { get; set; }
    }
}