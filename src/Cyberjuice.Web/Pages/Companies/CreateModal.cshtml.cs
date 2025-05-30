using System.Threading.Tasks;
using Cyberjuice.Companies;
using Cyberjuice.Companies.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Cyberjuice.Web.Pages.Companies;

public class CreateModalModel : CyberjuicePageModel
{
    [BindProperty]
    public CompanyDto Company { get; set; }

    private readonly ICompanyAppService _workspaceAppService;

    public CreateModalModel(ICompanyAppService workspaceAppService)
    {
        _workspaceAppService = workspaceAppService;
    }

    public void OnGet()
    {
        Company = new CompanyDto();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _workspaceAppService.CreateAsync(Company.Name);
        return NoContent();
    }
} 