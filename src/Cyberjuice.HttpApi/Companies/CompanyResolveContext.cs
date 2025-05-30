using Microsoft.AspNetCore.Http;
using System;

namespace Cyberjuice.Companies;

public class CompanyResolveContext(HttpContext httpContext) : ICompanyResolveContext
{
    public Guid? CompanyId { get; set; }
    public string CompanyName { get; set; }

    public IServiceProvider ServiceProvider => httpContext.RequestServices;

    public HttpContext GetHttpContext()
    {
        return httpContext;
    }
}
