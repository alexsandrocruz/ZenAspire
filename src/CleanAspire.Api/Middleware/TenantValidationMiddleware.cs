using CleanAspire.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using CleanAspire.Domain.Identities;

namespace CleanAspire.Api.Middleware;

/// <summary>
/// Middleware to validate tenant claim for authenticated users.
/// Ensures that authenticated users have a valid tenant ID.
/// </summary>
public class TenantValidationMiddleware
{
    private readonly RequestDelegate _next;

    public TenantValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentUserService currentUserService, UserManager<ApplicationUser> userManager)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

        // Skip tenant validation for authentication-related endpoints
        var authEndpoints = new[] { "/account/", "/login", "/logout", "/manage/" };
        if (authEndpoints.Any(endpoint => path.StartsWith(endpoint)))
        {
            await _next(context);
            return;
        }

        // Skip tenant validation for anonymous endpoints
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<IAuthorizeData>() == null)
        {
            await _next(context);
            return;
        }

        // For authenticated users, validate tenant claim
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantId = currentUserService.TenantId;

            // If no tenant claim found, try to get it from database and add it as a temporary claim
            if (string.IsNullOrEmpty(tenantId))
            {
                var userId = currentUserService.UserId;
                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        tenantId = user.TenantId;
                        // Log for debugging
                        Console.WriteLine($"[DEBUG] No tenant claim found for user {user.UserName}, using TenantId from database: {tenantId}");
                    }
                }
            }

            if (string.IsNullOrEmpty(tenantId))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                    title = "Forbidden",
                    status = 403,
                    detail = "User is authenticated but no tenant information found. Please contact system administrator.",
                    instance = context.Request.Path
                });
                return;
            }
        }

        await _next(context);
    }
}