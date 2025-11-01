using CleanAspire.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CleanAspire.Infrastructure.Services;

/// <summary>
/// Implementation of ICurrentUserService that reads from HTTP context claims.
/// No fallback tenant - requires authentication.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?
        .FindFirstValue(ClaimTypes.NameIdentifier);

    public string? TenantId => _httpContextAccessor.HttpContext?.User?
        .FindFirstValue("tenant"); // Custom claim for tenant

    public string? UserName => _httpContextAccessor.HttpContext?.User?
        .FindFirstValue(ClaimTypes.Name);
}
