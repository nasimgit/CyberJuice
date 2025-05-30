using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Cyberjuice.Data;
using Volo.Abp.DependencyInjection;

namespace Cyberjuice.EntityFrameworkCore;

public class EntityFrameworkCoreCyberjuiceDbSchemaMigrator
    : ICyberjuiceDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreCyberjuiceDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the CyberjuiceDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<CyberjuiceDbContext>()
            .Database
            .MigrateAsync();
    }
}
