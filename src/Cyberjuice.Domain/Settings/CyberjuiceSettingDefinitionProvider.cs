using Volo.Abp.Settings;

namespace Cyberjuice.Settings;

public class CyberjuiceSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(CyberjuiceSettings.MySetting1));
    }
}
