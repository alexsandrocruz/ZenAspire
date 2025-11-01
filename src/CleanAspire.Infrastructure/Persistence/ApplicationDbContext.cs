using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Domain.Common;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Identities;
using CleanAspire.Infrastructure.Persistence.Extensions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CleanAspire.Infrastructure.Persistence;
/// <summary>
/// Represents the application database context.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    private readonly ICurrentUserService? _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
    /// <param name="currentUserService">The current user service for multi-tenancy.</param>
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService? currentUserService = null)
       : base(options)
    {
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Gets or sets the Clients DbSet.
    /// </summary>
    public DbSet<Client> Clients { get; set; }

    /// <summary>
    /// Gets or sets the Contacts DbSet.
    /// </summary>
    public DbSet<Contact> Contacts { get; set; }

    /// <summary>
    /// Gets or sets the Tenants DbSet.
    /// </summary>
    public DbSet<Tenant> Tenants { get; set; }

    /// <summary>
    /// Gets or sets the AuditTrails DbSet.
    /// </summary>
    public DbSet<AuditTrail> AuditTrails { get; set; }

    /// <summary>
    /// Gets or sets the Products DbSet.
    /// </summary>
    public DbSet<Product> Products { get; set; }

    /// <summary>
    /// Gets or sets the Stocks DbSet.
    /// </summary>
    public DbSet<Stock> Stocks { get; set; }

    /// <summary>
    /// Gets or sets the Tags DbSet.
    /// </summary>
    public DbSet<Tag> Tags { get; set; }

    /// <summary>
    /// Gets or sets the TagLinks DbSet.
    /// </summary>
    public DbSet<TagLink> TagLinks { get; set; }

    /// <summary>
    /// Gets or sets the Notes DbSet.
    /// </summary>
    public DbSet<Note> Notes { get; set; }

    /// <summary>
    /// Gets or sets the Attachments DbSet.
    /// </summary>
    public DbSet<Attachment> Attachments { get; set; }

    /// <summary>
    /// Gets or sets the Addresses DbSet.
    /// </summary>
    public DbSet<Address> Addresses { get; set; }

    /// <summary>
    /// Gets or sets the ChannelIdentities DbSet.
    /// </summary>
    public DbSet<ChannelIdentity> ChannelIdentities { get; set; }

    /// <summary>
    /// Gets or sets the Activities DbSet.
    /// Phase 3: Timeline - Activities & Interactions
    /// </summary>
    public DbSet<Activity> Activities { get; set; }

    /// <summary>
    /// Gets or sets the Interactions DbSet.
    /// Phase 3: Timeline - Activities & Interactions
    /// </summary>
    public DbSet<Interaction> Interactions { get; set; }

    /// <summary>
    /// Configures the schema needed for the identity framework.
    /// </summary>
    /// <param name="builder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // ✅ Global query filters for multi-tenancy
        if (_currentUserService != null)
        {
            builder.Entity<Client>().HasQueryFilter(e =>
                e.TenantId == _currentUserService.TenantId);

            builder.Entity<Contact>().HasQueryFilter(e =>
                e.TenantId == _currentUserService.TenantId);

            // ✅ Phase 1: Tags, Notes, Attachments
            builder.Entity<Tag>().HasQueryFilter(e =>
                e.TenantId == _currentUserService.TenantId);

            builder.Entity<TagLink>().HasQueryFilter(e =>
                e.TenantId == _currentUserService.TenantId);

            builder.Entity<Note>().HasQueryFilter(e =>
                e.TenantId == _currentUserService.TenantId);

            builder.Entity<Attachment>().HasQueryFilter(e =>
                e.TenantId == _currentUserService.TenantId);

            // ✅ Phase 2: Address & Channel Normalization
            builder.Entity<Address>().HasQueryFilter(e =>
                e.TenantId == _currentUserService.TenantId);

            builder.Entity<ChannelIdentity>().HasQueryFilter(e =>
                e.TenantId == _currentUserService.TenantId);

            // ✅ Phase 3: Timeline - Activities & Interactions
            builder.Entity<Activity>().HasQueryFilter(e =>
                e.TenantId == _currentUserService.TenantId);

            builder.Entity<Interaction>().HasQueryFilter(e =>
                e.TenantId == _currentUserService.TenantId);
        }
    }

    /// <summary>
    /// Configures the conventions to be used for this context.
    /// </summary>
    /// <param name="configurationBuilder">The builder being used to configure conventions for this context.</param>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<string>().HaveMaxLength(450);
    }
}

