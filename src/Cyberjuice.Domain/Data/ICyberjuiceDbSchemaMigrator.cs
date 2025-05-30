using System.Threading.Tasks;

namespace Cyberjuice.Data;

public interface ICyberjuiceDbSchemaMigrator
{
    Task MigrateAsync();
}
