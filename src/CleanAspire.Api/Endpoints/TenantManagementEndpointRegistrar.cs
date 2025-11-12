using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CleanAspire.Domain.Identities;
using System.Security.Claims;

namespace CleanAspire.Api.Endpoints;

/// <summary>
/// Endpoint registrar for tenant management utilities.
/// Used for setup and maintenance of tenant claims.
/// </summary>
public class TenantManagementEndpointRegistrar : IEndpointRegistrar
{
    public void RegisterRoutes(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/admin/tenants").WithTags("Admin.Tenants").RequireAuthorization();

        /// <summary>
        /// Migrate existing users to have tenant claims.
        /// This is a temporary endpoint for migration purposes.
        /// </summary>
        group.MapPost("/migrate-claims", async ([FromServices] UserManager<ApplicationUser> userManager) =>
        {
            var users = userManager.Users.ToList();
            var migrated = 0;
            var errors = new List<string>();

            foreach (var user in users)
            {
                try
                {
                    // Check if user already has tenant claim
                    var existingClaims = await userManager.GetClaimsAsync(user);
                    var hasTenantClaim = existingClaims.Any(c => c.Type == "tenant");

                    if (!hasTenantClaim)
                    {
                        var tenantId = user.TenantId ?? "host";
                        await userManager.AddClaimAsync(user, new Claim("tenant", tenantId));
                        migrated++;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Failed to migrate user {user.UserName}: {ex.Message}");
                }
            }

            return Results.Ok(new
            {
                message = "Tenant claims migration completed",
                usersChecked = users.Count,
                usersMigrated = migrated,
                errors = errors
            });
        })
        .Produces(StatusCodes.Status200OK)
        .WithSummary("Migrate existing users to have tenant claims")
        .WithDescription("Adds tenant claims to existing users who don't have them. For migration purposes only.");
    }
}