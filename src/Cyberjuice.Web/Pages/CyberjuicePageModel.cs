using Cyberjuice.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace Cyberjuice.Web.Pages;

public abstract class CyberjuicePageModel : AbpPageModel
{
    protected CyberjuicePageModel()
    {
        LocalizationResourceType = typeof(CyberjuiceResource);
    }
}
