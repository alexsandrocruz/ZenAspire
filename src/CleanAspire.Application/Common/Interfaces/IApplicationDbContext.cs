using CleanAspire.Domain.Entities;

namespace CleanAspire.Application.Common.Interfaces;

/// <summary>
/// Represents the application database context interface.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Gets or sets the Clients DbSet.
    /// </summary>
    DbSet<Client> Clients { get; set; }

    /// <summary>
    /// Gets or sets the Contacts DbSet.
    /// </summary>
    DbSet<Contact> Contacts { get; set; }

    /// <summary>
    /// Gets or sets the Products DbSet.
    /// </summary>
    DbSet<Product> Products { get; set; }

    /// <summary>
    /// Gets or sets the AuditTrails DbSet.
    /// </summary>
    DbSet<AuditTrail> AuditTrails { get; set; }

    /// <summary>
    /// Gets or sets the Tenants DbSet.
    /// </summary>
    DbSet<Tenant> Tenants { get; set; }

    /// <summary>
    /// Gets or sets the Stocks DbSet.
    /// </summary>
    DbSet<Stock> Stocks { get; set; }

    /// <summary>
    /// Gets or sets the Tags DbSet.
    /// </summary>
    DbSet<Tag> Tags { get; set; }

    /// <summary>
    /// Gets or sets the TagLinks DbSet.
    /// </summary>
    DbSet<TagLink> TagLinks { get; set; }

    /// <summary>
    /// Gets or sets the Notes DbSet.
    /// </summary>
    DbSet<Note> Notes { get; set; }

    /// <summary>
    /// Gets or sets the Attachments DbSet.
    /// </summary>
    DbSet<Attachment> Attachments { get; set; }

    /// <summary>
    /// Gets or sets the Addresses DbSet.
    /// </summary>
    DbSet<Address> Addresses { get; set; }

    /// <summary>
    /// Gets or sets the ChannelIdentities DbSet.
    /// </summary>
    DbSet<ChannelIdentity> ChannelIdentities { get; set; }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

