# Phase 0-1 Implementation Guide

## Quick Start: First 4 Weeks

This guide provides detailed, step-by-step implementation instructions for **Phase 0** (Foundation & Multi-Tenancy) and **Phase 1** (Tags, Notes, Attachments). These are the critical foundation layers for the GenZ CRM.

---

## 🎯 Phase 0: Foundation & Multi-Tenancy (Week 1-2)

### Task 0.1: Add TenantId to Client Entity

**Duration:** 2-3 hours

#### Step 1: Update Client Entity
**File:** `src/CleanAspire.Domain/Entities/Client.cs`

```csharp
// Add after existing properties:
[Required]
public string TenantId { get; set; } = string.Empty;

// Add or update for spec alignment:
[MaxLength(200)]
public string? LegalName { get; set; }  // New field

// Rename DocumentNumber to TaxId (keep DocumentNumber as alias for now)
[MaxLength(32)]
public string? TaxId { get; set; }  // CNPJ/CPF normalized (uppercase, no special chars)

[MaxLength(32)]
public string LifecycleStage { get; set; } = "Lead";  // Lead, Prospect, Client, Former

public string? OwnerUserId { get; set; }  // Assigned internal owner

// Update ClientType enum (add School):
public enum ClientType
{
    Company = 1,
    PublicFigure = 2,
    Government = 3,
    NonProfit = 4,
    School = 5  // NEW
}
```

#### Step 2: Update Client EF Configuration
**File:** `src/CleanAspire.Infrastructure/Persistence/Configurations/ClientConfiguration.cs`

```csharp
public void Configure(EntityTypeBuilder<Client> builder)
{
    builder.ToTable("Clients");
    builder.HasKey(x => x.Id);

    // TenantId - Required
    builder.Property(x => x.TenantId)
        .IsRequired()
        .HasMaxLength(450);

    // Composite unique index: (TenantId, TaxId) - nullable filter
    builder.HasIndex(x => new { x.TenantId, x.TaxId })
        .IsUnique()
        .HasFilter($"\"{nameof(Client.TaxId)}\" IS NOT NULL");

    // Index TenantId for query performance
    builder.HasIndex(x => x.TenantId);

    // New fields
    builder.Property(x => x.LegalName)
        .HasMaxLength(200);

    builder.Property(x => x.TaxId)
        .HasMaxLength(32);

    builder.Property(x => x.LifecycleStage)
        .IsRequired()
        .HasMaxLength(32)
        .HasDefaultValue("Lead");

    builder.Property(x => x.OwnerUserId)
        .HasMaxLength(450);

    // ... rest of existing configuration
}
```

#### Step 3: Update Contact Entity
**File:** `src/CleanAspire.Domain/Entities/Contact.cs`

```csharp
// Add after existing properties:
[Required]
public string TenantId { get; set; } = string.Empty;

[MaxLength(32)]
public string LifecycleStage { get; set; } = "Lead";

public string? OwnerUserId { get; set; }

// Rename MobilePhone to Mobile for spec alignment (keep MobilePhone as alias)
[MaxLength(40)]  // E.164 format: +countrycode + number
public string? Mobile { get; set; }

// For future M:N support (keep ClientId required for now)
public Guid? AccountId { get; set; }
```

#### Step 4: Update Contact EF Configuration
**File:** `src/CleanAspire.Infrastructure/Persistence/Configurations/ContactConfiguration.cs`

```csharp
public void Configure(EntityTypeBuilder<Contact> builder)
{
    builder.ToTable("Contacts");
    builder.HasKey(x => x.Id);

    // TenantId - Required
    builder.Property(x => x.TenantId)
        .IsRequired()
        .HasMaxLength(450);

    // Composite unique index: (TenantId, Email)
    builder.HasIndex(x => new { x.TenantId, x.Email })
        .IsUnique();

    // Index TenantId for query performance
    builder.HasIndex(x => x.TenantId);

    // New fields
    builder.Property(x => x.LifecycleStage)
        .IsRequired()
        .HasMaxLength(32)
        .HasDefaultValue("Lead");

    builder.Property(x => x.OwnerUserId)
        .HasMaxLength(450);

    builder.Property(x => x.Mobile)
        .HasMaxLength(40);

    // ... rest of existing configuration
}
```

#### Step 5: Add Global Query Filter
**File:** `src/CleanAspire.Infrastructure/Persistence/ApplicationDbContext.cs`

```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    private readonly ICurrentUserService? _currentUserService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService? currentUserService = null)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    // ... DbSets ...

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Global query filter for multi-tenancy
        if (_currentUserService != null)
        {
            builder.Entity<Client>().HasQueryFilter(e =>
                e.TenantId == _currentUserService.TenantId);

            builder.Entity<Contact>().HasQueryFilter(e =>
                e.TenantId == _currentUserService.TenantId);
        }
    }
}
```

#### Step 6: Create ICurrentUserService
**File:** `src/CleanAspire.Application/Common/Interfaces/ICurrentUserService.cs`

```csharp
namespace CleanAspire.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? TenantId { get; }
    string? UserName { get; }
}
```

**File:** `src/CleanAspire.Infrastructure/Services/CurrentUserService.cs`

```csharp
using CleanAspire.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CleanAspire.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? TenantId => _httpContextAccessor.HttpContext?.User?.FindFirstValue("tenant");

    public string? UserName => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
}
```

**Register in DI:** `src/CleanAspire.Infrastructure/DependencyInjection.cs`

```csharp
services.AddScoped<ICurrentUserService, CurrentUserService>();
services.AddHttpContextAccessor();
```

#### Step 7: Create Migration
```bash
cd src/Migrators/Migrators.SQLite
dotnet ef migrations add AddTenantIdAndAlignWithCrmSpec -s ../../CleanAspire.AppHost
```

**Review migration file** - ensure:
- TenantId column added (non-nullable, with default value for existing rows)
- LegalName, TaxId, LifecycleStage, OwnerUserId added
- Unique indexes created
- Data migration for existing records (set TenantId to a default tenant)

#### Step 8: Update Domain Events
**File:** `src/CleanAspire.Domain/Events/ClientEvents.cs`

```csharp
namespace CleanAspire.Domain.Events;

public record ClientCreatedEvent(Guid ClientId, string TenantId, string Name) : IDomainEvent;

public record ClientUpdatedEvent(Guid ClientId, string TenantId) : IDomainEvent;

public record ClientDeletedEvent(Guid ClientId, string TenantId) : IDomainEvent;
```

**File:** `src/CleanAspire.Domain/Events/ContactEvents.cs`

```csharp
namespace CleanAspire.Domain.Events;

public record ContactCreatedEvent(Guid ContactId, Guid ClientId, string TenantId) : IDomainEvent;

public record ContactUpdatedEvent(Guid ContactId, string TenantId) : IDomainEvent;

public record ContactDeletedEvent(Guid ContactId, string TenantId) : IDomainEvent;
```

---

### Task 0.2: Update API Routes to `/api/crm/*`

**Duration:** 1 hour

#### Update Client Endpoint Registrar
**File:** `src/CleanAspire.Api/Endpoints/ClientEndpointRegistrar.cs`

```csharp
public void RegisterEndpoints(IEndpointRouteBuilder app)
{
    var group = app.MapGroup("/api/crm/clients")
        .WithTags("CRM.Clients")
        .RequireAuthorization();

    // GET /api/crm/clients?search=&tag=&status=&stage=&page=&pageSize=
    group.MapGet("/", GetClients)
        .WithName("GetClients")
        .Produces<PaginatedList<ClientDto>>();

    // GET /api/crm/clients/{id}
    group.MapGet("/{id:guid}", GetClientById)
        .WithName("GetClientById")
        .Produces<ClientDto>()
        .ProducesNotFound();

    // POST /api/crm/clients
    group.MapPost("/", CreateClient)
        .WithName("CreateClient")
        .Produces<ClientDto>(StatusCodes.Status201Created)
        .ProducesValidationProblem();

    // PUT /api/crm/clients/{id}
    group.MapPut("/{id:guid}", UpdateClient)
        .WithName("UpdateClient")
        .Produces<ClientDto>()
        .ProducesValidationProblem()
        .ProducesNotFound();

    // DELETE /api/crm/clients/{id} (soft delete)
    group.MapDelete("/{id:guid}", DeleteClient)
        .WithName("DeleteClient")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesNotFound();

    // GET /api/crm/clients/{id}/contacts
    group.MapGet("/{id:guid}/contacts", GetClientContacts)
        .WithName("GetClientContacts")
        .Produces<List<ContactDto>>();
}
```

#### Update Contact Endpoint Registrar
**File:** `src/CleanAspire.Api/Endpoints/ContactEndpointRegistrar.cs`

```csharp
public void RegisterEndpoints(IEndpointRouteBuilder app)
{
    var group = app.MapGroup("/api/crm/contacts")
        .WithTags("CRM.Contacts")
        .RequireAuthorization();

    // Similar pattern...
}
```

---

### Task 0.3: Update Commands & Queries for TenantId

**Duration:** 2-3 hours

#### Update CreateClientCommand
**File:** `src/CleanAspire.Application/Features/Clients/Commands/CreateClientCommand.cs`

```csharp
public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateClientCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            return Result<Guid>.Failure("TenantId is required");

        var client = new Client
        {
            TenantId = _currentUser.TenantId,
            Name = request.Name,
            LegalName = request.LegalName,
            TaxId = NormalizeTaxId(request.TaxId),
            // ... other fields
        };

        // Raise domain event
        client.AddDomainEvent(new ClientCreatedEvent(client.Id, client.TenantId, client.Name));

        _context.Clients.Add(client);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(client.Id);
    }

    private static string? NormalizeTaxId(string? taxId)
    {
        if (string.IsNullOrWhiteSpace(taxId))
            return null;

        // Remove special chars, uppercase
        return new string(taxId.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
    }
}
```

#### Similar updates for:
- `UpdateClientCommand`
- `DeleteClientCommand`
- `CreateContactCommand`
- `UpdateContactCommand`
- `DeleteContactCommand`

**All queries automatically filtered by TenantId via global query filter!**

---

### Task 0.4: Update Tests

**Duration:** 2 hours

#### Update Integration Tests
**File:** `tests/CleanAspire.Tests/Features/Clients/CreateClientCommandTests.cs`

```csharp
public class CreateClientCommandTests : TestBase
{
    [Test]
    public async Task CreateClient_WithValidData_ShouldSucceed()
    {
        // Arrange
        var command = new CreateClientCommand
        {
            Name = "Test Client",
            TaxId = "12.345.678/0001-90",
            // ...
        };

        // Act
        var result = await SendAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var client = await FindAsync<Client>(result.Value);
        client.Should().NotBeNull();
        client!.TenantId.Should().Be(TestCurrentUserService.DefaultTenantId);
        client.TaxId.Should().Be("12345678000190"); // Normalized
    }

    [Test]
    public async Task CreateClient_WithDuplicateTaxId_InSameTenant_ShouldFail()
    {
        // Arrange
        var client1 = await AddAsync(new Client { TenantId = "tenant1", TaxId = "12345678000190" });

        var command = new CreateClientCommand { TaxId = "12.345.678/0001-90" };

        // Act
        Func<Task> act = async () => await SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Test]
    public async Task CreateClient_WithDuplicateTaxId_InDifferentTenant_ShouldSucceed()
    {
        // Arrange
        var client1 = await AddAsync(new Client { TenantId = "tenant1", TaxId = "12345678000190" });

        // Switch to tenant2
        TestCurrentUserService.SetTenantId("tenant2");

        var command = new CreateClientCommand { TaxId = "12.345.678/0001-90" };

        // Act
        var result = await SendAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
```

#### Add TestCurrentUserService
**File:** `tests/CleanAspire.Tests/TestCurrentUserService.cs`

```csharp
public class TestCurrentUserService : ICurrentUserService
{
    public const string DefaultTenantId = "test-tenant-id";
    public const string DefaultUserId = "test-user-id";

    private string _tenantId = DefaultTenantId;

    public string? UserId => DefaultUserId;
    public string? TenantId => _tenantId;
    public string? UserName => "test@example.com";

    public void SetTenantId(string tenantId) => _tenantId = tenantId;
}
```

---

### Task 0.5: Run Migration & Test

**Duration:** 1 hour

```bash
# Apply migration
dotnet ef database update -p src/Migrators/Migrators.SQLite/ -s src/CleanAspire.AppHost/

# Run tests
dotnet test

# Run API and test with Scalar
dotnet run --project src/CleanAspire.AppHost
```

**Manual API Test:**
```bash
# Create a client (should auto-assign TenantId from JWT claim)
POST /api/crm/clients
Authorization: Bearer {token-with-tenant-claim}
{
  "name": "Acme Corp",
  "legalName": "Acme Corporation Ltd",
  "taxId": "12.345.678/0001-90",
  "lifecycleStage": "Prospect"
}
```

---

## 🎯 Phase 1: Tags, Notes, Attachments (Week 3-4)

### Task 1.1: Create Tag Entities

**Duration:** 3 hours

#### Create Tag Entity
**File:** `src/CleanAspire.Domain/Entities/Tag.cs`

```csharp
using CleanAspire.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace CleanAspire.Domain.Entities;

public class Tag : BaseAuditableEntity
{
    [Required]
    public string TenantId { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Description { get; set; }

    [MaxLength(20)]
    public string? Color { get; set; } // Hex color code for UI

    public virtual ICollection<TagLink> TagLinks { get; set; } = new List<TagLink>();
}
```

#### Create TagLink Entity (M:N polymorphic)
**File:** `src/CleanAspire.Domain/Entities/TagLink.cs`

```csharp
using CleanAspire.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace CleanAspire.Domain.Entities;

public class TagLink : BaseEntity
{
    [Required]
    public string TenantId { get; set; } = string.Empty;

    [Required]
    public Guid TagId { get; set; }
    public virtual Tag Tag { get; set; } = null!;

    [Required]
    public OwnerType OwnerType { get; set; }

    [Required]
    public Guid OwnerId { get; set; }

    public DateTime LinkedAt { get; set; } = DateTime.UtcNow;
}

public enum OwnerType
{
    Client = 1,
    Contact = 2,
    Activity = 3,
    Opportunity = 4,
    Case = 5,
    Ticket = 6
}
```

#### Create EF Configurations
**File:** `src/CleanAspire.Infrastructure/Persistence/Configurations/TagConfiguration.cs`

```csharp
using CleanAspire.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanAspire.Infrastructure.Persistence.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).IsRequired().HasMaxLength(450);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Description).HasMaxLength(100);
        builder.Property(x => x.Color).HasMaxLength(20);

        // Unique constraint: (TenantId, Name)
        builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();

        builder.HasMany(x => x.TagLinks)
            .WithOne(x => x.Tag)
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

**File:** `src/CleanAspire.Infrastructure/Persistence/Configurations/TagLinkConfiguration.cs`

```csharp
public class TagLinkConfiguration : IEntityTypeConfiguration<TagLink>
{
    public void Configure(EntityTypeBuilder<TagLink> builder)
    {
        builder.ToTable("TagLinks");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).IsRequired().HasMaxLength(450);
        builder.Property(x => x.OwnerType).HasConversion<int>().IsRequired();
        builder.Property(x => x.OwnerId).IsRequired();

        // Unique constraint: (TenantId, TagId, OwnerType, OwnerId)
        builder.HasIndex(x => new { x.TenantId, x.TagId, x.OwnerType, x.OwnerId }).IsUnique();

        // Index for querying by owner
        builder.HasIndex(x => new { x.TenantId, x.OwnerType, x.OwnerId });
    }
}
```

#### Add to DbContext
**File:** `src/CleanAspire.Infrastructure/Persistence/ApplicationDbContext.cs`

```csharp
public DbSet<Tag> Tags { get; set; }
public DbSet<TagLink> TagLinks { get; set; }

// In OnModelCreating, add global query filter:
builder.Entity<Tag>().HasQueryFilter(e => e.TenantId == _currentUserService.TenantId);
builder.Entity<TagLink>().HasQueryFilter(e => e.TenantId == _currentUserService.TenantId);
```

---

### Task 1.2: Create Note & Attachment Entities

**Duration:** 3 hours

#### Create Note Entity
**File:** `src/CleanAspire.Domain/Entities/Note.cs`

```csharp
using CleanAspire.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace CleanAspire.Domain.Entities;

public class Note : BaseAuditableEntity
{
    [Required]
    public string TenantId { get; set; } = string.Empty;

    [Required]
    public OwnerType OwnerType { get; set; }

    [Required]
    public Guid OwnerId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty; // Max length handled in config

    public bool IsPinned { get; set; } = false;

    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}
```

#### Create Attachment Entity
**File:** `src/CleanAspire.Domain/Entities/Attachment.cs`

```csharp
using CleanAspire.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace CleanAspire.Domain.Entities;

public class Attachment : BaseAuditableEntity
{
    [Required]
    public Guid NoteId { get; set; }
    public virtual Note Note { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public long Size { get; set; } // Bytes

    [Required]
    [MaxLength(500)]
    public string BlobKey { get; set; } = string.Empty; // Path in MinIO/Azure Blob

    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty; // MIME type
}
```

#### Create EF Configurations
```csharp
// Similar pattern as Tag/TagLink
// Add to DbContext, add query filters
```

---

### Task 1.3: Data Migration from Inline Fields

**Duration:** 2 hours

#### Create Data Migration Script
**File:** `src/Migrators/Migrators.SQLite/DataMigrations/MigrateInlineTagsAndNotes.cs`

```csharp
public static class MigrateInlineTagsAndNotes
{
    public static async Task ExecuteAsync(ApplicationDbContext context)
    {
        // Migrate Client.Tags (comma-separated) to Tag + TagLink
        var clients = await context.Clients
            .Where(c => !string.IsNullOrEmpty(c.Tags))
            .ToListAsync();

        foreach (var client in clients)
        {
            var tagNames = client.Tags!.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var tagName in tagNames)
            {
                var tag = await context.Tags.FirstOrDefaultAsync(t =>
                    t.TenantId == client.TenantId && t.Name == tagName.Trim());

                if (tag == null)
                {
                    tag = new Tag { TenantId = client.TenantId, Name = tagName.Trim() };
                    context.Tags.Add(tag);
                    await context.SaveChangesAsync();
                }

                var link = new TagLink
                {
                    TenantId = client.TenantId,
                    TagId = tag.Id,
                    OwnerType = OwnerType.Client,
                    OwnerId = client.Id
                };
                context.TagLinks.Add(link);
            }
        }

        // Migrate Client.Notes to Note entity
        var clientsWithNotes = await context.Clients
            .Where(c => !string.IsNullOrEmpty(c.Notes))
            .ToListAsync();

        foreach (var client in clientsWithNotes)
        {
            var note = new Note
            {
                TenantId = client.TenantId,
                OwnerType = OwnerType.Client,
                OwnerId = client.Id,
                Title = "Migrated Note",
                Body = client.Notes!,
                Created = client.Created
            };
            context.Notes.Add(note);
        }

        await context.SaveChangesAsync();

        // Similar for Contacts...
    }
}
```

#### Run Migration
```bash
dotnet ef migrations add AddTagsNotesAttachments -p src/Migrators/Migrators.SQLite/ -s src/CleanAspire.AppHost/
dotnet ef database update -p src/Migrators/Migrators.SQLite/ -s src/CleanAspire.AppHost/

# Run data migration
dotnet run --project src/CleanAspire.Infrastructure/DataMigrationRunner.csproj
```

---

### Task 1.4: Create Tag APIs

**Duration:** 4 hours

#### Commands
**File:** `src/CleanAspire.Application/Features/Tags/Commands/CreateTagCommand.cs`

```csharp
public record CreateTagCommand(string Name, string? Description, string? Color) : IRequest<Result<Guid>>;

public class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public async Task<Result<Guid>> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            return Result<Guid>.Failure("TenantId required");

        // Check if tag already exists
        var exists = await _context.Tags.AnyAsync(t =>
            t.TenantId == _currentUser.TenantId && t.Name == request.Name, cancellationToken);

        if (exists)
            return Result<Guid>.Failure("Tag already exists");

        var tag = new Tag
        {
            TenantId = _currentUser.TenantId,
            Name = request.Name,
            Description = request.Description,
            Color = request.Color
        };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(tag.Id);
    }
}
```

**File:** `src/CleanAspire.Application/Features/Tags/Commands/LinkTagCommand.cs`

```csharp
public record LinkTagCommand(Guid TagId, OwnerType OwnerType, Guid OwnerId) : IRequest<Result>;

public class LinkTagCommandHandler : IRequestHandler<LinkTagCommand, Result>
{
    // Similar pattern...
}
```

#### Queries
**File:** `src/CleanAspire.Application/Features/Tags/Queries/GetTagsQuery.cs`

```csharp
public record GetTagsQuery(string? Search) : IRequest<Result<List<TagDto>>>;

public class GetTagsQueryHandler : IRequestHandler<GetTagsQuery, Result<List<TagDto>>>
{
    public async Task<Result<List<TagDto>>> Handle(GetTagsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Tags.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(t => t.Name.Contains(request.Search));
        }

        var tags = await query
            .OrderBy(t => t.Name)
            .Take(50) // Autocomplete limit
            .Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Color = t.Color
            })
            .ToListAsync(cancellationToken);

        return Result<List<TagDto>>.Success(tags);
    }
}
```

#### Endpoints
**File:** `src/CleanAspire.Api/Endpoints/TagEndpointRegistrar.cs`

```csharp
public class TagEndpointRegistrar : IEndpointRegistrar
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/crm/tags")
            .WithTags("CRM.Tags")
            .RequireAuthorization();

        group.MapGet("/", GetTags);
        group.MapPost("/", CreateTag);
        group.MapPost("/link", LinkTag);
        group.MapDelete("/link", UnlinkTag);
    }

    private static async Task<IResult> GetTags(
        [FromQuery] string? search,
        ISender sender)
    {
        var result = await sender.Send(new GetTagsQuery(search));
        return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
    }

    // ... other handlers
}
```

---

### Task 1.5: Create Note & Attachment APIs

**Duration:** 4 hours

#### Similar pattern as Tags:
- `CreateNoteCommand`, `UpdateNoteCommand`, `DeleteNoteCommand`
- `GetNotesByOwnerQuery`
- `AddAttachmentCommand` (with multipart file upload)
- `NoteEndpointRegistrar`

#### Blob Storage Service
**File:** `src/CleanAspire.Infrastructure/Services/BlobStorageService.cs`

```csharp
public interface IBlobStorageService
{
    Task<string> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken);
    Task<Stream> DownloadAsync(string blobKey, CancellationToken cancellationToken);
    Task DeleteAsync(string blobKey, CancellationToken cancellationToken);
}

public class MinioBlobStorageService : IBlobStorageService
{
    private readonly IMinioClient _minioClient;

    // Implementation using MinIO SDK
}
```

---

### Task 1.6: Update Client/Contact Endpoints

**Duration:** 2 hours

#### Add Tag Endpoints to ClientEndpointRegistrar
```csharp
// POST /api/crm/clients/{id}/tags
group.MapPost("/{id:guid}/tags", LinkTagsToClient);

// DELETE /api/crm/clients/{id}/tags/{tagId}
group.MapDelete("/{id:guid}/tags/{tagId:guid}", UnlinkTagFromClient);

// GET /api/crm/clients/{id}/notes
group.MapGet("/{id:guid}/notes", GetClientNotes);

// POST /api/crm/clients/{id}/notes
group.MapPost("/{id:guid}/notes", CreateClientNote);
```

---

## Testing Checklist (Phase 0-1)

### Phase 0 Tests
- [ ] Client creation with TenantId
- [ ] Contact creation with TenantId
- [ ] Tenant isolation (no cross-tenant queries)
- [ ] Unique constraint: (TenantId, TaxId) on Client
- [ ] Unique constraint: (TenantId, Email) on Contact
- [ ] TaxId normalization
- [ ] Domain events include TenantId
- [ ] API routes use `/api/crm/*`

### Phase 1 Tests
- [ ] Tag creation
- [ ] Tag linking to Client/Contact
- [ ] Tag unlinking
- [ ] Tag autocomplete (search)
- [ ] Note creation with owner
- [ ] Attachment upload
- [ ] Attachment download
- [ ] Data migration from inline tags/notes
- [ ] Tenant isolation for tags/notes

---

## Next Steps After Phase 1

Once Phase 0-1 is complete, proceed to:
- **Phase 2:** Address & Channel Normalization
- **Phase 3:** Timeline (Activities & Interactions)
- **Phase 4:** LGPD Compliance (Consent & Data Privacy)

Refer to `CRM-ROADMAP.md` for full implementation plan.

---

**Document Version:** 1.0
**Last Updated:** 2025-10-31
**Status:** 🟢 Ready to Start
