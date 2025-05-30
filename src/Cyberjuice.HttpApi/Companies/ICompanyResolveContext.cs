using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Cyberjuice.Companies;

public interface ICompanyResolveContext
{
    Guid? CompanyId { get; set; }

    string CompanyName { get; set; }

    IServiceProvider ServiceProvider { get; }

    HttpContext GetHttpContext();
}
