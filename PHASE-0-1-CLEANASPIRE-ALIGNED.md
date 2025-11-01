# Phase 0-1 Implementation Guide (CleanAspire Aligned)

## 🎯 Overview

This guide provides step-by-step implementation for **Phase 0** (Multi-Tenancy) and **Phase 1** (Tags, Notes, Attachments) following **CleanAspire's exact patterns and conventions** as documented in `CRUD-IMPLEMENTATION-GUIDE.md`.

---

## 📋 CleanAspire Conventions (Must Follow)

### Architecture Layers:
- **Domain**: Entities + Events (`DomainEvent` pattern)
- **Application**: Commands/Queries + DTOs + Validators (FluentValidation)
- **API**: Minimal APIs with `IEndpointRegistrar`
- **ClientApp**: Blazor WASM + MudBlazor

### Key Patterns:
1. **Cache**: `IFusionCacheRequest<T>` for queries, `IFusionCacheRefreshRequest<T>` for commands
2. **Commands**: Return `TDto` or `Unit`, implement `IRequiresValidation`
3. **Queries**: Return `TDto` or `PaginatedResult<TDto>`
4. **Events**: `new EntityCreatedEvent(entity)` - pass full entity
5. **Endpoints**: `IEndpointRegistrar` with `RegisterRoutes(IEndpointRouteBuilder)`
6. **Pagination**: Zero-based indexing (page 0 = first page)
7. **IDs**: `string Id = Guid.CreateVersion7().ToString()`
8. **Validators**: `AbstractValidator<TCommand>`
9. **Service Proxy**: HttpClient with detailed logging
10. **UI**: MudBlazor with MudDataGrid

---

## 🎯 Phase 0: Multi-Tenancy Foundation (Week 1-2)

### Checklist:
- [ ] 0.1 Add TenantId to Client entity
- [ ] 0.2 Add TenantId to Contact entity
- [ ] 0.3 Update EF configurations with tenant indexes
- [ ] 0.4 Create ICurrentUserService
- [ ] 0.5 Add global query filters
- [ ] 0.6 Update domain events (align with CRM spec)
- [ ] 0.7 Update API routes to `/api/crm/*`
- [ ] 0.8 Update commands/queries for TenantId
- [ ] 0.9 Create and run migration
- [ ] 0.10 Update tests for multi-tenancy

---

### Task 0.1: Add TenantId to Client Entity

**Duration:** 1-2 hours

#### Step 1: Update Domain Entity
**File:** `src/CleanAspire.Domain/Entities/Client.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using CleanAspire.Domain.Common;

namespace CleanAspire.Domain.Entities;

public class Client : BaseAuditableEntity, IAuditTrial
{
    // ✅ NEW: Multi-tenancy support
    [Required]
    [MaxLength(450)]
    public string TenantId { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    // ✅ NEW: Align with CRM spec
    [MaxLength(200)]
    public string? LegalName { get; set; }

    [MaxLength(50)]
    public string? TradeName { get; set; }

    // ✅ RENAME: DocumentNumber → TaxId (keep DocumentNumber for backward compat)
    [MaxLength(32)]
    public string? TaxId { get; set; } // CNPJ, CPF (normalized: uppercase, no special chars)

    [Obsolete("Use TaxId instead")]
    [MaxLength(20)]
    public string? DocumentNumber
    {
        get => TaxId;
        set => TaxId = value;
    }

    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(250)]
    public string? Website { get; set; }

    // Endereço (inline for now - will be normalized in Phase 2)
    [MaxLength(200)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(50)]
    public string? State { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    // Informações de negócio
    [MaxLength(100)]
    public string? Industry { get; set; }

    [MaxLength(50)]
    public string? Size { get; set; }

    public decimal? AnnualRevenue { get; set; }

    public int? EmployeeCount { get; set; }

    // ✅ NEW: CRM lifecycle management
    [Required]
    [MaxLength(32)]
    public string LifecycleStage { get; set; } = "Lead"; // Lead, Prospect, Client, Former

    // ✅ NEW: Owner assignment
    [MaxLength(450)]
    public string? OwnerUserId { get; set; }

    // Informações de relacionamento
    public ClientType Type { get; set; }

    public ClientStatus Status { get; set; }

    public ClientPriority Priority { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [MaxLength(500)]
    public string? Tags { get; set; }

    public DateTime? FirstContactDate { get; set; }

    public DateTime? LastInteractionDate { get; set; }

    // Relacionamentos
    public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();

    // Propriedades calculadas
    public string DisplayName => !string.IsNullOrEmpty(TradeName) ? TradeName : Name;

    public bool IsCompany => Type == ClientType.Company;

    public bool IsPublicFigure => Type == ClientType.PublicFigure;
}

public enum ClientType
{
    Company = 1,
    PublicFigure = 2,
    Government = 3,
    NonProfit = 4,
    School = 5  // ✅ NEW: Align with CRM spec
}

public enum ClientStatus
{
    Prospect = 1,
    Active = 2,
    Inactive = 3,
    Churned = 4,
    Blocked = 5
}

public enum ClientPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    VIP = 4
}
```

#### Step 2: Update EF Configuration
**File:** `src/CleanAspire.Infrastructure/Persistence/Configurations/ClientConfiguration.cs`

```csharp
using CleanAspire.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanAspire.Infrastructure.Persistence.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");

        builder.HasKey(x => x.Id);

        // ✅ Multi-tenancy: TenantId required
        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(450);

        // ✅ Composite unique index per tenant: (TenantId, TaxId)
        builder.HasIndex(x => new { x.TenantId, x.TaxId })
            .IsUnique()
            .HasFilter($"\"{nameof(Client.TaxId)}\" IS NOT NULL");

        // ✅ Index for tenant queries
        builder.HasIndex(x => x.TenantId);

        // ✅ Index for lookup queries
        builder.HasIndex(x => new { x.TenantId, x.Name });
        builder.HasIndex(x => new { x.TenantId, x.Email });

        // Required fields
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.LifecycleStage)
            .IsRequired()
            .HasMaxLength(32)
            .HasDefaultValue("Lead");

        // New CRM fields
        builder.Property(x => x.LegalName)
            .HasMaxLength(200);

        builder.Property(x => x.TaxId)
            .HasMaxLength(32);

        builder.Property(x => x.OwnerUserId)
            .HasMaxLength(450);

        // Existing fields (unchanged)
        builder.Property(x => x.TradeName).HasMaxLength(50);
        builder.Property(x => x.Email).HasMaxLength(100);
        builder.Property(x => x.Phone).HasMaxLength(20);
        builder.Property(x => x.Website).HasMaxLength(250);
        builder.Property(x => x.Address).HasMaxLength(200);
        builder.Property(x => x.City).HasMaxLength(100);
        builder.Property(x => x.State).HasMaxLength(50);
        builder.Property(x => x.PostalCode).HasMaxLength(20);
        builder.Property(x => x.Country).HasMaxLength(100);
        builder.Property(x => x.Industry).HasMaxLength(100);
        builder.Property(x => x.Size).HasMaxLength(50);
        builder.Property(x => x.AnnualRevenue).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.Tags).HasMaxLength(500);

        // Enums
        builder.Property(x => x.Type).HasConversion<int>();
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.Priority).HasConversion<int>();

        // Relacionamentos
        builder.HasMany(x => x.Contacts)
            .WithOne(x => x.Client)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices existentes
        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.LifecycleStage);

        // Propriedades ignoradas (calculadas)
        builder.Ignore(x => x.DisplayName);
        builder.Ignore(x => x.IsCompany);
        builder.Ignore(x => x.IsPublicFigure);
        builder.Ignore(x => x.DocumentNumber); // Obsolete alias

        // Ignorar eventos de domínio
        builder.Ignore(e => e.DomainEvents);
    }
}
```

---

### Task 0.2: Update Contact Entity

**File:** `src/CleanAspire.Domain/Entities/Contact.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using CleanAspire.Domain.Common;

namespace CleanAspire.Domain.Entities;

public class Contact : BaseAuditableEntity, IAuditTrial
{
    // ✅ NEW: Multi-tenancy support
    [Required]
    [MaxLength(450)]
    public string TenantId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    // ✅ RENAME: MobilePhone → Mobile (keep MobilePhone for backward compat)
    [MaxLength(40)] // E.164 format: +55 11 99999-9999
    public string? Mobile { get; set; }

    [Obsolete("Use Mobile instead")]
    [MaxLength(20)]
    public string? MobilePhone
    {
        get => Mobile;
        set => Mobile = value;
    }

    [MaxLength(100)]
    public string? JobTitle { get; set; }

    [MaxLength(100)]
    public string? Department { get; set; }

    // Endereço pessoal (opcional, pode ser diferente do Client)
    [MaxLength(200)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(50)]
    public string? State { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [MaxLength(500)]
    public string? Tags { get; set; }

    // ✅ NEW: CRM lifecycle management
    [Required]
    [MaxLength(32)]
    public string LifecycleStage { get; set; } = "Lead"; // Lead, Prospect, Client, Former

    // ✅ NEW: Owner assignment
    [MaxLength(450)]
    public string? OwnerUserId { get; set; }

    // Informações de relacionamento
    public ContactType Type { get; set; }
    public ContactStatus Status { get; set; }

    public bool IsMainContact { get; set; } = false;
    public bool IsDecisionMaker { get; set; } = false;

    // Datas importantes
    public DateTime? BirthDate { get; set; }
    public DateTime? LastContactDate { get; set; }

    // Relacionamentos
    [Required]
    public string ClientId { get; set; } = string.Empty;
    public virtual Client Client { get; set; } = null!;

    // ✅ NEW: For future M:N support (Phase 7)
    public Guid? AccountId { get; set; }

    // Propriedades calculadas
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string CompanyName => Client?.DisplayName ?? string.Empty;
}

public enum ContactType
{
    Lead = 1,
    Prospect = 2,
    Client = 3,
    Partner = 4,
    Vendor = 5,
    Employee = 6
}

public enum ContactStatus
{
    Active = 1,
    Inactive = 2,
    Qualified = 3,
    Unqualified = 4
}
```

**File:** `src/CleanAspire.Infrastructure/Persistence/Configurations/ContactConfiguration.cs`

```csharp
public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("Contacts");
        builder.HasKey(x => x.Id);

        // ✅ Multi-tenancy: TenantId required
        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(450);

        // ✅ Composite unique index per tenant: (TenantId, Email)
        builder.HasIndex(x => new { x.TenantId, x.Email })
            .IsUnique();

        // ✅ Index for tenant queries
        builder.HasIndex(x => x.TenantId);

        // ✅ Index for lookup queries
        builder.HasIndex(x => new { x.TenantId, x.FirstName, x.LastName });
        builder.HasIndex(x => new { x.TenantId, x.ClientId });

        // Required fields
        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(150);
        builder.Property(x => x.ClientId).IsRequired();

        builder.Property(x => x.LifecycleStage)
            .IsRequired()
            .HasMaxLength(32)
            .HasDefaultValue("Lead");

        // New CRM fields
        builder.Property(x => x.Mobile).HasMaxLength(40);
        builder.Property(x => x.OwnerUserId).HasMaxLength(450);

        // Existing fields
        builder.Property(x => x.Phone).HasMaxLength(20);
        builder.Property(x => x.JobTitle).HasMaxLength(100);
        builder.Property(x => x.Department).HasMaxLength(100);
        builder.Property(x => x.Address).HasMaxLength(200);
        builder.Property(x => x.City).HasMaxLength(100);
        builder.Property(x => x.State).HasMaxLength(50);
        builder.Property(x => x.PostalCode).HasMaxLength(20);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.Tags).HasMaxLength(500);

        // Enums
        builder.Property(x => x.Type).HasConversion<int>();
        builder.Property(x => x.Status).HasConversion<int>();

        // Relacionamentos
        builder.HasOne(x => x.Client)
            .WithMany(x => x.Contacts)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.IsMainContact);
        builder.HasIndex(x => x.IsDecisionMaker);
        builder.HasIndex(x => x.LifecycleStage);

        // Propriedades ignoradas (calculadas)
        builder.Ignore(x => x.FullName);
        builder.Ignore(x => x.CompanyName);
        builder.Ignore(x => x.MobilePhone); // Obsolete alias

        // Ignorar eventos de domínio
        builder.Ignore(e => e.DomainEvents);
    }
}
```

---

### Task 0.3: Create ICurrentUserService

**File:** `src/CleanAspire.Application/Common/Interfaces/ICurrentUserService.cs`

```csharp
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
```

**File:** `src/CleanAspire.Infrastructure/Services/CurrentUserService.cs`

```csharp
using CleanAspire.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CleanAspire.Infrastructure.Services;

/// <summary>
/// Implementation of ICurrentUserService that reads from HTTP context claims.
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
```

**Register in DI - File:** `src/CleanAspire.Infrastructure/DependencyInjection.cs`

```csharp
// Add to ConfigureServices method:
services.AddHttpContextAccessor();
services.AddScoped<ICurrentUserService, CurrentUserService>();
```

---

### Task 0.4: Add Global Query Filters

**File:** `src/CleanAspire.Infrastructure/Persistence/ApplicationDbContext.cs`

```csharp
using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Domain.Common;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Identities;
using CleanAspire.Infrastructure.Persistence.Extensions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CleanAspire.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    private readonly ICurrentUserService? _currentUserService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService? currentUserService = null) // ✅ Inject ICurrentUserService
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<AuditTrail> AuditTrails { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Stock> Stocks { get; set; }

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
        }
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<string>().HaveMaxLength(450);
    }
}
```

---

### Task 0.5: Update Domain Events

**File:** `src/CleanAspire.Domain/Events/ClientEvents.cs`

```csharp
using CleanAspire.Domain.Common;
using CleanAspire.Domain.Entities;

namespace CleanAspire.Domain.Events;

// ✅ Follow CleanAspire pattern: pass full entity
public record ClientCreatedEvent(Client Item) : DomainEvent;
public record ClientUpdatedEvent(Client Item) : DomainEvent;
public record ClientDeletedEvent(Client Item) : DomainEvent;
```

**File:** `src/CleanAspire.Domain/Events/ContactEvents.cs`

```csharp
using CleanAspire.Domain.Common;
using CleanAspire.Domain.Entities;

namespace CleanAspire.Domain.Events;

public record ContactCreatedEvent(Contact Item) : DomainEvent;
public record ContactUpdatedEvent(Contact Item) : DomainEvent;
public record ContactDeletedEvent(Contact Item) : DomainEvent;
```

---

### Task 0.6: Update API Routes to `/api/crm/*`

**File:** `src/CleanAspire.Api/Endpoints/ClientEndpointRegistrar.cs`

```csharp
namespace CleanAspire.Api.Endpoints;

public class ClientEndpointRegistrar : IEndpointRegistrar
{
    // ✅ Update route: /customers → /api/crm/clients
    public void RegisterRoutes(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/crm/clients")
            .WithTags("CRM.Clients"); // ✅ Update tag
            // .RequireAuthorization(); // TODO: Re-enable after testing

        group.MapGet("/", GetAllClients)
            .WithName("GetAllClients")
            .WithSummary("Get all clients")
            .Produces<List<ClientDto>>();

        group.MapGet("/{id}", GetClientById)
            .WithName("GetClientById")
            .WithSummary("Get client by ID")
            .Produces<ClientDto>()
            .Produces(404);

        group.MapPost("/", CreateClient)
            .WithName("CreateClient")
            .WithSummary("Create a new client")
            .Produces<ClientDto>(201)
            .ProducesValidationProblem();

        group.MapPut("/", UpdateClient)
            .WithName("UpdateClient")
            .WithSummary("Update an existing client")
            .Produces(204)
            .Produces(404)
            .ProducesValidationProblem();

        group.MapDelete("/{id}", DeleteClient)
            .WithName("DeleteClient")
            .WithSummary("Delete a client")
            .Produces(204)
            .Produces(404);

        group.MapPost("/pagination", GetClientsWithPagination)
            .WithName("GetClientsWithPagination")
            .WithSummary("Get clients with pagination")
            .Produces<PaginatedResult<ClientDto>>();

        // ✅ NEW: CRM-specific endpoints (Phase 1+)
        group.MapGet("/{id}/contacts", GetClientContacts)
            .WithName("GetClientContacts")
            .WithSummary("Get contacts for a client")
            .Produces<List<ContactDto>>();
    }

    // Handler methods (same as before, but with TenantId awareness)
    // ...
}
```

**File:** `src/CleanAspire.Api/Endpoints/ContactEndpointRegistrar.cs`

```csharp
namespace CleanAspire.Api.Endpoints;

public class ContactEndpointRegistrar : IEndpointRegistrar
{
    // ✅ Update route: /contacts → /api/crm/contacts
    public void RegisterRoutes(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/crm/contacts")
            .WithTags("CRM.Contacts"); // ✅ Update tag
            // .RequireAuthorization();

        // Standard CRUD endpoints...
    }
}
```

---

### Task 0.7: Update Commands for TenantId

**File:** `src/CleanAspire.Application/Features/Clients/Commands/CreateClientCommand.cs`

```csharp
using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Application.Features.Clients.Caching;
using CleanAspire.Application.Features.Clients.DTOs;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Events;
using MediatR;
using System.Text.RegularExpressions;

namespace CleanAspire.Application.Features.Clients.Commands;

// ✅ Follow CleanAspire pattern: IFusionCacheRefreshRequest + IRequiresValidation
public record CreateClientCommand : IFusionCacheRefreshRequest<ClientDto>, IRequiresValidation
{
    public string Name { get; init; } = string.Empty;
    public string? LegalName { get; init; }
    public string? TradeName { get; init; }
    public string? TaxId { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Website { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    public string? Industry { get; init; }
    public string? Size { get; init; }
    public decimal? AnnualRevenue { get; init; }
    public int? EmployeeCount { get; init; }
    public ClientType Type { get; init; } = ClientType.Company;
    public ClientStatus Status { get; init; } = ClientStatus.Prospect;
    public ClientPriority Priority { get; init; } = ClientPriority.Medium;
    public string LifecycleStage { get; init; } = "Lead";
    public string? OwnerUserId { get; init; }
    public string? Notes { get; init; }
    public string? Tags { get; init; }

    // ✅ FusionCache integration
    public string CacheKey => ClientCacheKey.GetAllCacheKey;
    public CancellationToken? SharedExpiryToken => ClientCacheKey.SharedExpiryToken;
}

internal sealed class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, ClientDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser; // ✅ Inject current user service

    public CreateClientCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ICurrentUserService currentUser)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<ClientDto> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        // ✅ Validate tenant
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        var entity = new Client
        {
            TenantId = _currentUser.TenantId, // ✅ Set TenantId from current user
            Name = request.Name,
            LegalName = request.LegalName,
            TradeName = request.TradeName,
            TaxId = NormalizeTaxId(request.TaxId), // ✅ Normalize: uppercase, no special chars
            Email = request.Email,
            Phone = request.Phone,
            Website = request.Website,
            Address = request.Address,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country,
            Industry = request.Industry,
            Size = request.Size,
            AnnualRevenue = request.AnnualRevenue,
            EmployeeCount = request.EmployeeCount,
            Type = request.Type,
            Status = request.Status,
            Priority = request.Priority,
            LifecycleStage = request.LifecycleStage,
            OwnerUserId = request.OwnerUserId ?? _currentUser.UserId, // ✅ Default to current user
            Notes = request.Notes,
            Tags = request.Tags,
            FirstContactDate = DateTime.UtcNow
        };

        // ✅ Follow CleanAspire pattern: pass full entity
        entity.AddDomainEvent(new ClientCreatedEvent(entity));

        _context.Clients.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ClientDto>(entity);
    }

    private static string? NormalizeTaxId(string? taxId)
    {
        if (string.IsNullOrWhiteSpace(taxId))
            return null;

        // Remove special chars, uppercase (CNPJ: 12.345.678/0001-90 → 12345678000190)
        return Regex.Replace(taxId, @"[^\w]", "").ToUpperInvariant();
    }
}
```

**Similar updates needed for:**
- `UpdateClientCommand.cs`
- `DeleteClientCommand.cs`
- `CreateContactCommand.cs`
- `UpdateContactCommand.cs`
- `DeleteContactCommand.cs`

All should:
1. Inject `ICurrentUserService`
2. Set `TenantId` from `_currentUser.TenantId`
3. Validate `TenantId` is not null
4. Pass full entity to domain events

---

### Task 0.8: Create Migration

```bash
cd src/Migrators/Migrators.SQLite
dotnet ef migrations add AddMultiTenancyToCrm -s ../../CleanAspire.AppHost

# Review migration file, then apply:
dotnet ef database update -s ../../CleanAspire.AppHost
```

**Migration should include:**
- Add `TenantId` column to Clients (non-nullable, default value for existing rows)
- Add `TenantId` column to Contacts (non-nullable, default value for existing rows)
- Add `LegalName`, `LifecycleStage`, `OwnerUserId` to Clients
- Add `Mobile`, `LifecycleStage`, `OwnerUserId` to Contacts
- Create unique indexes: `(TenantId, TaxId)`, `(TenantId, Email)`
- Create tenant indexes for performance

---

### Task 0.9: Update Tests

**File:** `tests/CleanAspire.Tests/Common/TestCurrentUserService.cs` (NEW)

```csharp
using CleanAspire.Application.Common.Interfaces;

namespace CleanAspire.Tests.Common;

public class TestCurrentUserService : ICurrentUserService
{
    public const string DefaultTenantId = "test-tenant-id";
    public const string DefaultUserId = "test-user-id";

    private string _tenantId = DefaultTenantId;
    private string _userId = DefaultUserId;

    public string? UserId => _userId;
    public string? TenantId => _tenantId;
    public string? UserName => "test@example.com";

    public void SetTenantId(string tenantId) => _tenantId = tenantId;
    public void SetUserId(string userId) => _userId = userId;
    public void Reset()
    {
        _tenantId = DefaultTenantId;
        _userId = DefaultUserId;
    }
}
```

**Update TestBase to use TestCurrentUserService:**

```csharp
// In TestBase.cs ConfigureServices:
services.AddScoped<ICurrentUserService, TestCurrentUserService>();
```

**Example test:**

```csharp
[Test]
public async Task CreateClient_WithValidData_ShouldSetTenantId()
{
    // Arrange
    var command = new CreateClientCommand
    {
        Name = "Test Client",
        TaxId = "12.345.678/0001-90",
        Email = "test@example.com"
    };

    // Act
    var result = await SendAsync(command);

    // Assert
    result.Should().NotBeNull();
    result.Id.Should().NotBeEmpty();

    var client = await FindAsync<Client>(result.Id);
    client.Should().NotBeNull();
    client!.TenantId.Should().Be(TestCurrentUserService.DefaultTenantId);
    client.TaxId.Should().Be("12345678000190"); // Normalized
}

[Test]
public async Task GetClientsQuery_ShouldOnlyReturnClientsFromSameTenant()
{
    // Arrange - Create clients in two different tenants
    var tenant1Client = new Client
    {
        TenantId = "tenant-1",
        Name = "Client 1"
    };
    await AddAsync(tenant1Client);

    // Switch to tenant-2
    var testUserService = GetService<TestCurrentUserService>();
    testUserService.SetTenantId("tenant-2");

    var tenant2Client = new Client
    {
        TenantId = "tenant-2",
        Name = "Client 2"
    };
    await AddAsync(tenant2Client);

    // Act - Query as tenant-2
    var query = new GetAllClientsQuery();
    var result = await SendAsync(query);

    // Assert - Should only see tenant-2 client
    result.Should().HaveCount(1);
    result[0].Name.Should().Be("Client 2");
}
```

---

## 🎯 Phase 1: Tags, Notes, Attachments (Week 3-4)

### Checklist:
- [ ] 1.1 Create Tag + TagLink entities
- [ ] 1.2 Create Note + Attachment entities
- [ ] 1.3 Create EF configurations
- [ ] 1.4 Add to DbContext with query filters
- [ ] 1.5 Create DTOs
- [ ] 1.6 Create Commands (CreateTag, LinkTag, CreateNote, AddAttachment)
- [ ] 1.7 Create Queries (GetTags, GetNotesByOwner)
- [ ] 1.8 Create Validators
- [ ] 1.9 Create Cache Keys
- [ ] 1.10 Create API Endpoints
- [ ] 1.11 Data migration from inline tags/notes
- [ ] 1.12 Create Service Proxies
- [ ] 1.13 Update UI components

---

### Task 1.1: Create Tag Entities

**File:** `src/CleanAspire.Domain/Entities/Tag.cs`

```csharp
using CleanAspire.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace CleanAspire.Domain.Entities;

public class Tag : BaseAuditableEntity, IAuditTrial
{
    [Required]
    [MaxLength(450)]
    public string TenantId { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Description { get; set; }

    [MaxLength(20)]
    public string? Color { get; set; } // Hex color for UI: #FF5733

    public virtual ICollection<TagLink> TagLinks { get; set; } = new List<TagLink>();
}
```

**File:** `src/CleanAspire.Domain/Entities/TagLink.cs`

```csharp
using CleanAspire.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace CleanAspire.Domain.Entities;

/// <summary>
/// Polymorphic many-to-many relationship between Tags and various entities.
/// </summary>
public class TagLink : BaseEntity
{
    [Required]
    [MaxLength(450)]
    public string TenantId { get; set; } = string.Empty;

    [Required]
    public string TagId { get; set; } = string.Empty;
    public virtual Tag Tag { get; set; } = null!;

    [Required]
    public OwnerType OwnerType { get; set; }

    [Required]
    public string OwnerId { get; set; } = string.Empty; // Client.Id, Contact.Id, etc.

    public DateTime LinkedAt { get; set; } = DateTime.UtcNow;

    public string? LinkedBy { get; set; } // UserId who created the link
}

public enum OwnerType
{
    Client = 1,
    Contact = 2,
    Activity = 3,
    Opportunity = 4,
    Case = 5,
    Ticket = 6,
    Note = 7
}
```

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
        builder.HasIndex(x => x.TenantId); // Performance

        builder.HasMany(x => x.TagLinks)
            .WithOne(x => x.Tag)
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(e => e.DomainEvents);
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
        builder.Property(x => x.TagId).IsRequired().HasMaxLength(450);
        builder.Property(x => x.OwnerId).IsRequired().HasMaxLength(450);
        builder.Property(x => x.OwnerType).HasConversion<int>().IsRequired();
        builder.Property(x => x.LinkedBy).HasMaxLength(450);

        // Unique constraint: (TenantId, TagId, OwnerType, OwnerId)
        builder.HasIndex(x => new { x.TenantId, x.TagId, x.OwnerType, x.OwnerId }).IsUnique();

        // Query by owner
        builder.HasIndex(x => new { x.TenantId, x.OwnerType, x.OwnerId });
    }
}
```

---

### Task 1.2: Create Tag Commands/Queries (CleanAspire Pattern)

**File:** `src/CleanAspire.Application/Features/Tags/DTOs/TagDto.cs`

```csharp
namespace CleanAspire.Application.Features.Tags.DTOs;

public class TagDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public int UsageCount { get; set; } // Number of TagLinks
    public DateTime? Created { get; set; }
}
```

**File:** `src/CleanAspire.Application/Features/Tags/Commands/CreateTagCommand.cs`

```csharp
using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Application.Features.Tags.Caching;
using CleanAspire.Application.Features.Tags.DTOs;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Events;
using MediatR;

namespace CleanAspire.Application.Features.Tags.Commands;

// ✅ Follow CleanAspire pattern
public record CreateTagCommand : IFusionCacheRefreshRequest<TagDto>, IRequiresValidation
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Color { get; init; }

    public string CacheKey => TagCacheKey.GetAllCacheKey;
    public CancellationToken? SharedExpiryToken => TagCacheKey.SharedExpiryToken;
}

internal sealed class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, TagDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public CreateTagCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ICurrentUserService currentUser)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<TagDto> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        // Check for duplicate
        var exists = await _context.Tags
            .AnyAsync(t => t.TenantId == _currentUser.TenantId && t.Name == request.Name,
                     cancellationToken);

        if (exists)
            throw new InvalidOperationException($"Tag '{request.Name}' already exists");

        var entity = new Tag
        {
            TenantId = _currentUser.TenantId,
            Name = request.Name,
            Description = request.Description,
            Color = request.Color ?? GenerateRandomColor()
        };

        entity.AddDomainEvent(new TagCreatedEvent(entity));

        _context.Tags.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TagDto>(entity);
    }

    private static string GenerateRandomColor()
    {
        var random = new Random();
        return $"#{random.Next(0x1000000):X6}";
    }
}
```

**File:** `src/CleanAspire.Application/Features/Tags/Commands/LinkTagCommand.cs`

```csharp
public record LinkTagCommand : IFusionCacheRefreshRequest<Unit>, IRequiresValidation
{
    public string TagId { get; init; } = string.Empty;
    public OwnerType OwnerType { get; init; }
    public string OwnerId { get; init; } = string.Empty;

    public string CacheKey => TagCacheKey.GetAllCacheKey;
    public CancellationToken? SharedExpiryToken => TagCacheKey.SharedExpiryToken;
}

internal sealed class LinkTagCommandHandler : IRequestHandler<LinkTagCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public async Task<Unit> Handle(LinkTagCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        // Verify tag exists in tenant
        var tagExists = await _context.Tags
            .AnyAsync(t => t.Id == request.TagId && t.TenantId == _currentUser.TenantId,
                     cancellationToken);

        if (!tagExists)
            throw new InvalidOperationException("Tag not found");

        // Check if link already exists
        var linkExists = await _context.TagLinks
            .AnyAsync(tl => tl.TenantId == _currentUser.TenantId &&
                           tl.TagId == request.TagId &&
                           tl.OwnerType == request.OwnerType &&
                           tl.OwnerId == request.OwnerId,
                     cancellationToken);

        if (linkExists)
            return Unit.Value; // Already linked, idempotent

        var link = new TagLink
        {
            TenantId = _currentUser.TenantId,
            TagId = request.TagId,
            OwnerType = request.OwnerType,
            OwnerId = request.OwnerId,
            LinkedAt = DateTime.UtcNow,
            LinkedBy = _currentUser.UserId
        };

        _context.TagLinks.Add(link);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
```

**File:** `src/CleanAspire.Application/Features/Tags/Queries/GetTagsQuery.cs`

```csharp
using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Application.Features.Tags.Caching;
using CleanAspire.Application.Features.Tags.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanAspire.Application.Features.Tags.Queries;

public record GetTagsQuery : IFusionCacheRequest<List<TagDto>>
{
    public string? Search { get; init; }

    public string CacheKey => TagCacheKey.GetSearchCacheKey(Search ?? "");
    public TimeSpan? Duration => TagCacheKey.Duration;
}

internal sealed class GetTagsQueryHandler : IRequestHandler<GetTagsQuery, List<TagDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public async Task<List<TagDto>> Handle(GetTagsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            return new List<TagDto>();

        var query = _context.Tags.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(t => t.Name.ToLower().Contains(search) ||
                                    (t.Description != null && t.Description.ToLower().Contains(search)));
        }

        var tags = await query
            .OrderBy(t => t.Name)
            .Take(50) // Limit for autocomplete
            .Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Color = t.Color,
                UsageCount = t.TagLinks.Count,
                Created = t.Created
            })
            .ToListAsync(cancellationToken);

        return tags;
    }
}
```

**File:** `src/CleanAspire.Application/Features/Tags/Caching/TagCacheKey.cs`

```csharp
namespace CleanAspire.Application.Features.Tags.Caching;

public static class TagCacheKey
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromHours(1);
    private static readonly TimeSpan DefaultDuration = TimeSpan.FromHours(2);

    public static TimeSpan? Duration => DefaultDuration;
    public static string GetAllCacheKey => "all-tags";
    public static string GetSearchCacheKey(string search) => $"tags-search-{search}";
    public static string GetByIdCacheKey(string id) => $"tag-by-id-{id}";
    public static string GetByOwnerCacheKey(OwnerType type, string id) => $"tags-owner-{type}-{id}";

    private static CancellationTokenSource _tokenSource = new();
    public static CancellationToken SharedExpiryToken => _tokenSource.Token;

    public static void Refresh() => SharedExpiryTokenSource();

    private static void SharedExpiryTokenSource()
    {
        _tokenSource.Cancel();
        _tokenSource = new CancellationTokenSource(RefreshInterval);
    }
}
```

**File:** `src/CleanAspire.Application/Features/Tags/Commands/CreateTagCommandValidator.cs`

```csharp
using FluentValidation;

namespace CleanAspire.Application.Features.Tags.Commands;

public class CreateTagCommandValidator : AbstractValidator<CreateTagCommand>
{
    public CreateTagCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Tag name is required")
            .MaximumLength(50).WithMessage("Tag name must not exceed 50 characters");

        RuleFor(v => v.Description)
            .MaximumLength(100).WithMessage("Description must not exceed 100 characters");

        RuleFor(v => v.Color)
            .Matches(@"^#[0-9A-Fa-f]{6}$").WithMessage("Color must be a valid hex color (e.g. #FF5733)")
            .When(v => !string.IsNullOrEmpty(v.Color));
    }
}
```

**File:** `src/CleanAspire.Domain/Events/TagEvents.cs`

```csharp
using CleanAspire.Domain.Common;
using CleanAspire.Domain.Entities;

namespace CleanAspire.Domain.Events;

public record TagCreatedEvent(Tag Item) : DomainEvent;
public record TagUpdatedEvent(Tag Item) : DomainEvent;
public record TagDeletedEvent(Tag Item) : DomainEvent;
```

---

### Task 1.3: Create Tag API Endpoints

**File:** `src/CleanAspire.Api/Endpoints/TagEndpointRegistrar.cs`

```csharp
using CleanAspire.Application.Features.Tags.Commands;
using CleanAspire.Application.Features.Tags.DTOs;
using CleanAspire.Application.Features.Tags.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CleanAspire.Api.Endpoints;

public class TagEndpointRegistrar : IEndpointRegistrar
{
    public void RegisterRoutes(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/crm/tags")
            .WithTags("CRM.Tags");
            // .RequireAuthorization();

        group.MapGet("/", GetTags)
            .WithName("GetTags")
            .WithSummary("Get tags (with autocomplete search)")
            .Produces<List<TagDto>>();

        group.MapPost("/", CreateTag)
            .WithName("CreateTag")
            .WithSummary("Create a new tag")
            .Produces<TagDto>(201)
            .ProducesValidationProblem();

        group.MapPost("/link", LinkTag)
            .WithName("LinkTag")
            .WithSummary("Link a tag to an entity")
            .Produces(204)
            .ProducesValidationProblem();

        group.MapDelete("/link", UnlinkTag)
            .WithName("UnlinkTag")
            .WithSummary("Unlink a tag from an entity")
            .Produces(204);
    }

    private static async Task<IResult> GetTags(
        [FromQuery] string? search,
        ISender sender)
    {
        var query = new GetTagsQuery { Search = search };
        var result = await sender.Send(query);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateTag(
        CreateTagCommand command,
        ISender sender)
    {
        var result = await sender.Send(command);
        return Results.Created($"/api/crm/tags/{result.Id}", result);
    }

    private static async Task<IResult> LinkTag(
        LinkTagCommand command,
        ISender sender)
    {
        await sender.Send(command);
        return Results.NoContent();
    }

    private static async Task<IResult> UnlinkTag(
        UnlinkTagCommand command,
        ISender sender)
    {
        await sender.Send(command);
        return Results.NoContent();
    }
}
```

---

### Task 1.4: Update DbContext

**File:** `src/CleanAspire.Infrastructure/Persistence/ApplicationDbContext.cs`

```csharp
// Add DbSets:
public DbSet<Tag> Tags { get; set; }
public DbSet<TagLink> TagLinks { get; set; }

// Add query filters in OnModelCreating:
builder.Entity<Tag>().HasQueryFilter(e =>
    e.TenantId == _currentUserService.TenantId);

builder.Entity<TagLink>().HasQueryFilter(e =>
    e.TenantId == _currentUserService.TenantId);
```

---

### Task 1.5: Create Migration for Phase 1

```bash
cd src/Migrators/Migrators.SQLite
dotnet ef migrations add AddTagsNotesAttachments -s ../../CleanAspire.AppHost
dotnet ef database update -s ../../CleanAspire.AppHost
```

---

## 📝 Testing Checklist

### Phase 0 Tests:
- [ ] Client created with correct TenantId
- [ ] Contact created with correct TenantId
- [ ] Query filters work (no cross-tenant data)
- [ ] TaxId normalized correctly
- [ ] Unique constraint per tenant works
- [ ] Domain events include TenantId
- [ ] API routes use `/api/crm/*`

### Phase 1 Tests:
- [ ] Tag creation with tenant isolation
- [ ] Tag autocomplete (search)
- [ ] Tag linking to Client/Contact
- [ ] Tag unlinking
- [ ] Duplicate tag name prevented per tenant
- [ ] Different tenants can have same tag name

---

## 🚀 Next Steps

After completing Phase 0-1:
1. Test thoroughly using Scalar (`https://localhost:7341/scalar/v1`)
2. Update ClientApp Service Proxies to use new `/api/crm/*` routes
3. Proceed to Phase 2 (Address & Channel normalization)
4. Refer to `CRM-ROADMAP.md` for full implementation plan

---

**Document Version:** 2.0 (CleanAspire Aligned)
**Last Updated:** 2025-10-31
**Status:** 🟢 Ready for Implementation
**Follows:** CRUD-IMPLEMENTATION-GUIDE.md patterns
