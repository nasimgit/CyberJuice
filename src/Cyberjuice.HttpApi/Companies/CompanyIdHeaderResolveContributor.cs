using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Cyberjuice.Companies;

/// <summary>
/// Resolves Company from the X-Company-Id header in HTTP requests
/// </summary>
public class CompanyIdHeaderResolveContributor : ICompanyResolveContributor, ITransientDependency
{
    /// <summary>
    /// Default header name: X-Company-ID.
    /// </summary>
    public const string HeaderName = "x-company-id";

    /// <summary>
    /// Default contributor name: CompanyIdHeader.
    /// </summary>
    public const string ContributorName = "CompanyIdHeader";
    /// <summary>
    /// Name of the contributor.
    /// </summary>
    public string Name => ContributorName;
    private readonly ILogger<CompanyIdHeaderResolveContributor> _logger;
    public CompanyIdHeaderResolveContributor(ILogger<CompanyIdHeaderResolveContributor> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Tries to resolve current Company from HTTP header.
    /// </summary>
    public Task ResolveAsync(ICompanyResolveContext context)
    {
        var httpContext = context.GetHttpContext();
        if (httpContext == null)
        {
            return Task.CompletedTask;
        }
        var WorkspaceIdHeader = httpContext.Request.Headers[HeaderName];
        if (WorkspaceIdHeader.Count == 0 || string.IsNullOrWhiteSpace(WorkspaceIdHeader[0]))
        {
            return Task.CompletedTask;
        }
        if (Guid.TryParse(WorkspaceIdHeader[0], out var workspaceId))
        {
            _logger.LogDebug($"Company Id found in request header: {workspaceId}");
            context.CompanyId = workspaceId;
        }
        else
        {
            _logger.LogDebug($"Invalid Company Id format in request header: {WorkspaceIdHeader[0]}");
        }
        return Task.CompletedTask;
    }
}
