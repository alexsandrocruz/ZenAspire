namespace CleanAspire.Application.Common.Interfaces;

/// <summary>
/// Provides access to the current authenticated user's information.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's ID.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets the current user's tenant ID (for multi-tenancy).
    /// </summary>
    string? TenantId { get; }

    /// <summary>
    /// Gets the current user's username/email.
    /// </summary>
    string? UserName { get; }
}
