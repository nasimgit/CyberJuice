using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace Cyberjuice.Web.Components.Company;

public class WorkspaceSelectorViewComponent : AbpViewComponent
{
    public virtual IViewComponentResult Invoke()
    {
        return View("~/Components/Company/Default.cshtml");
    }
} 
