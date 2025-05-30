using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Cyberjuice.Data;

/* This is used if database provider does't define
 * ICyberjuiceDbSchemaMigrator implementation.
 */
public class NullCyberjuiceDbSchemaMigrator : ICyberjuiceDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
