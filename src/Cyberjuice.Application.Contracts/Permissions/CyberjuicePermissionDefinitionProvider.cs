using Cyberjuice.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace Cyberjuice.Permissions;

public class CyberjuicePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(CyberjuicePermissions.GroupName);

        //Define your own permissions here. Example:
        //myGroup.AddPermission(CyberjuicePermissions.MyPermission1, L("Permission:MyPermission1"));

        // Employees permissions
        var employeesPermission = myGroup.AddPermission(CyberjuicePermissions.Employees.Default, L("Permission:Employees"));
        employeesPermission.AddChild(CyberjuicePermissions.Employees.Create, L("Permission:Employees.Create"));
        employeesPermission.AddChild(CyberjuicePermissions.Employees.Edit, L("Permission:Employees.Edit"));
        employeesPermission.AddChild(CyberjuicePermissions.Employees.Delete, L("Permission:Employees.Delete"));

        // Departments permissions
        var departmentsPermission = myGroup.AddPermission(CyberjuicePermissions.Departments.Default, L("Permission:Departments"));
        departmentsPermission.AddChild(CyberjuicePermissions.Departments.Create, L("Permission:Departments.Create"));
        departmentsPermission.AddChild(CyberjuicePermissions.Departments.Edit, L("Permission:Departments.Edit"));
        departmentsPermission.AddChild(CyberjuicePermissions.Departments.Delete, L("Permission:Departments.Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<CyberjuiceResource>(name);
    }
}
