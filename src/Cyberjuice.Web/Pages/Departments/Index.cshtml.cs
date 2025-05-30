using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace Cyberjuice.Web.Pages.Departments;

[Authorize]
public class IndexModel : AbpPageModel
{
    public async Task OnGetAsync()
    {
        await Task.CompletedTask;
    }
} 