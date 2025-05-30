using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;
using Microsoft.Extensions.Localization;
using Cyberjuice.Localization;

namespace Cyberjuice.Web;

[Dependency(ReplaceServices = true)]
public class CyberjuiceBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<CyberjuiceResource> _localizer;

    public CyberjuiceBrandingProvider(IStringLocalizer<CyberjuiceResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
