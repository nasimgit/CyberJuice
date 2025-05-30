using System.Collections.Generic;

namespace Cyberjuice.Companies;

public class CompanyResolveOptions
{
    public List<ICompanyResolveContributor> CompanyResolvers { get; }

    public CompanyResolveOptions()
    {
        CompanyResolvers = [];
    }

    public void AddResolver(ICompanyResolveContributor resolver)
    {
        CompanyResolvers.Add(resolver);
    }
}
