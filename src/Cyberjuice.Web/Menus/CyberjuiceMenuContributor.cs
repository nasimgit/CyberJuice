using System.Threading.Tasks;
using Cyberjuice.Localization;
using Cyberjuice.Permissions;
using Cyberjuice.MultiTenancy;
using Volo.Abp.SettingManagement.Web.Navigation;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Identity.Web.Navigation;
using Volo.Abp.UI.Navigation;

namespace Cyberjuice.Web.Menus;

public class CyberjuiceMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
    }

    private static Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<CyberjuiceResource>();

        //Home
        context.Menu.AddItem(
            new ApplicationMenuItem(
                CyberjuiceMenus.Home,
                l["Menu:Home"],
                "~/",
                icon: "fa fa-home",
                order: 1
            )
        );

        //Employees
        context.Menu.AddItem(
            new ApplicationMenuItem(
                CyberjuiceMenus.Employees,
                l["Employees"],
                url: "/Employees",
                icon: "fa fa-users",
                order: 2
            ).RequirePermissions(CyberjuicePermissions.Employees.Default)
        );

        context.Menu.AddItem(
        new ApplicationMenuItem(
            CyberjuiceMenus.Departments,
            l["Departments"],
            url: "/Departments",
            icon: "fa fa-users",
            order: 2
        ).RequirePermissions(CyberjuicePermissions.Departments.Default)
    );


        //Administration
        var administration = context.Menu.GetAdministration();
        administration.Order = 6;

        //Administration->Identity
        administration.SetSubItemOrder(IdentityMenuNames.GroupName, 1);
        
        administration.SetSubItemOrder(SettingManagementMenuNames.GroupName, 3);

        //Administration->Settings
        administration.SetSubItemOrder(SettingManagementMenuNames.GroupName, 7);
        
        return Task.CompletedTask;
    }
}
