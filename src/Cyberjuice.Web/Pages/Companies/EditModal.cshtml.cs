using System;
using System.Threading.Tasks;
using Cyberjuice.Companies;
using Cyberjuice.Companies.Dtos;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace Cyberjuice.Web.Pages.Companies;

public class EditModalModel : AbpPageModel
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public CompanyDto Company { get; set; }

    private readonly ICompanyAppService _workspaceAppService;

    public EditModalModel(ICompanyAppService workspaceAppService)
    {
        _workspaceAppService = workspaceAppService;
    }

    public async Task OnGetAsync()
    {
        Company = await _workspaceAppService.GetAsync(Id);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _workspaceAppService.UpdateAsync(Id, Company.Name);
        return NoContent();
    }
} 