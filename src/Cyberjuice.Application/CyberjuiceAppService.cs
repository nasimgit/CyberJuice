using Cyberjuice.Localization;
using Volo.Abp.Application.Services;

namespace Cyberjuice;

/* Inherit your application services from this class.
 */
public abstract class CyberjuiceAppService : ApplicationService
{
    protected CyberjuiceAppService()
    {
        LocalizationResource = typeof(CyberjuiceResource);
    }
}
