using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Cyberjuice.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class CyberjuiceDbContextFactory : IDesignTimeDbContextFactory<CyberjuiceDbContext>
{
    public CyberjuiceDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        CyberjuiceEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<CyberjuiceDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new CyberjuiceDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Cyberjuice.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
