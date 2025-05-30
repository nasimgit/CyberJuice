using System.Threading.Tasks;

namespace Cyberjuice.Companies;

public interface ICompanyResolveContributor
{
    string Name { get; }
    Task ResolveAsync(ICompanyResolveContext context);
}
