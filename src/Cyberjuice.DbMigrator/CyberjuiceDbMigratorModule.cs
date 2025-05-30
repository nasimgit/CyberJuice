using Cyberjuice.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Cyberjuice.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(CyberjuiceEntityFrameworkCoreModule),
    typeof(CyberjuiceApplicationContractsModule)
)]
public class CyberjuiceDbMigratorModule : AbpModule
{
}
