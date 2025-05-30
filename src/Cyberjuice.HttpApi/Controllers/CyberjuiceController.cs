using Cyberjuice.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Cyberjuice.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class CyberjuiceController : AbpControllerBase
{
    protected CyberjuiceController()
    {
        LocalizationResource = typeof(CyberjuiceResource);
    }
}
