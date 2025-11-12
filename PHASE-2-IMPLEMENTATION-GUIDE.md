# Phase 2: Address & Channel Normalization - Implementation Guide

## 📋 Overview

**Goal**: Separate addresses and communication channels for reusability and multi-channel support

**Duration**: Week 5 (estimated 20-30 hours)

**Status**: 🟡 **Foundation Complete - Commands/Queries/APIs In Progress**

---

## ✅ Completed Tasks

### 1. Domain Layer - Entities ✅
- **Address Entity**: `src/CleanAspire.Domain/Entities/Address.cs`
  - Polymorphic owner relationship (OwnerType + OwnerId)
  - Fields: Line1, Line2, District, City, State, Zip, Country
  - Geocoding support (GeoLat, GeoLng)
  - Primary flag support
  - Label support (e.g., "Home", "Office", "Billing")

- **ChannelIdentity Entity**: `src/CleanAspire.Domain/Entities/ChannelIdentity.cs`
  - Polymorphic owner relationship
  - ChannelType enum: Email, Phone, Mobile, WhatsApp, Website, Instagram, LinkedIn, Facebook, Twitter, Telegram, Skype, Other
  - Verification support (VerifiedAt)
  - Opt-in/Opt-out support for LGPD compliance
  - Primary flag support

- **ChannelType Enum**: `src/CleanAspire.Domain/Enums/ChannelType.cs`
  - 12 channel types defined

### 2. Infrastructure Layer - EF Core ✅
- **AddressConfiguration**: `src/CleanAspire.Infrastructure/Persistence/Configurations/AddressConfiguration.cs`
  - Indexes: TenantId, (TenantId, OwnerType, OwnerId), (TenantId, OwnerType, OwnerId, IsPrimary)
  - Geocoding index with filter
  - Decimal precision for coordinates (10,7)

- **ChannelIdentityConfiguration**: `src/CleanAspire.Infrastructure/Persistence/Configurations/ChannelIdentityConfiguration.cs`
  - Indexes: TenantId, (TenantId, OwnerType, OwnerId), (TenantId, Type, Value)
  - Unique constraint: (TenantId, OwnerType, OwnerId, Type, Value)
  - Verified channels index

- **ApplicationDbContext Updates**: `src/CleanAspire.Infrastructure/Persistence/ApplicationDbContext.cs`
  - DbSet<Address> Addresses
  - DbSet<ChannelIdentity> ChannelIdentities
  - Query filters for multi-tenancy

- **IApplicationDbContext Updates**: `src/CleanAspire.Application/Common/Interfaces/IApplicationDbContext.cs`
  - Interface updated with new DbSets

### 3. Client & Contact Entities - Obsolete Markers ✅
- **Client.cs** and **Contact.cs**:
  - Email, Phone, Website, Mobile marked as `[Obsolete]`
  - Address, City, State, PostalCode, Country marked as `[Obsolete]`
  - Backward compatibility maintained for existing code

### 4. Application Layer - DTOs ✅
- **AddressDto**: `src/CleanAspire.Application/Features/Addresses/DTOs/AddressDto.cs`
- **ChannelIdentityDto**: `src/CleanAspire.Application/Features/Channels/DTOs/ChannelIdentityDto.cs`

### 5. Domain Events ✅
- **AddressEvents**: `src/CleanAspire.Domain/Events/AddressEvents.cs`
  - AddressCreatedEvent, AddressUpdatedEvent, AddressDeletedEvent, AddressPrimaryChangedEvent

- **ChannelEvents**: `src/CleanAspire.Domain/Events/ChannelEvents.cs`
  - ChannelIdentityCreatedEvent, ChannelIdentityUpdatedEvent, ChannelIdentityDeletedEvent
  - ChannelIdentityVerifiedEvent, ChannelOptInChangedEvent

### 6. Cache Keys ✅
- **AddressCacheKey**: `src/CleanAspire.Application/Features/Addresses/Caching/AddressCacheKey.cs`
- **ChannelCacheKey**: `src/CleanAspire.Application/Features/Channels/Caching/ChannelCacheKey.cs`

---

## 🚧 Remaining Tasks

### 7. Commands (To Be Created)

#### Address Commands:
```
src/CleanAspire.Application/Features/Addresses/Commands/
├── CreateAddressCommand.cs
├── UpdateAddressCommand.cs
├── DeleteAddressCommand.cs
└── SetPrimaryAddressCommand.cs
```

**Template for CreateAddressCommand.cs**:
```csharp
using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Application.Features.Addresses.Caching;
using CleanAspire.Application.Features.Addresses.DTOs;
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Enums;
using CleanAspire.Domain.Events;
using MediatR;

namespace CleanAspire.Application.Features.Addresses.Commands;

public record CreateAddressCommand : IFusionCacheRefreshRequest<AddressDto>, IRequiresValidation
{
    public OwnerType OwnerType { get; init; }
    public string OwnerId { get; init; } = string.Empty;
    public string Line1 { get; init; } = string.Empty;
    public string? Line2 { get; init; }
    public string? District { get; init; }
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string Zip { get; init; } = string.Empty;
    public string Country { get; init; } = "BR";
    public decimal? GeoLat { get; init; }
    public decimal? GeoLng { get; init; }
    public bool IsPrimary { get; init; }
    public string? Label { get; init; }

    public string CacheKey => AddressCacheKey.GetByOwnerCacheKey(OwnerType, OwnerId);
    public CancellationToken? SharedExpiryToken => AddressCacheKey.SharedExpiryToken;
}

internal sealed class CreateAddressCommandHandler : IRequestHandler<CreateAddressCommand, AddressDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public CreateAddressCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ICurrentUserService currentUser)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<AddressDto> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUser.TenantId))
            throw new UnauthorizedAccessException("TenantId is required");

        // If setting as primary, unset other primary addresses for this owner
        if (request.IsPrimary)
        {
            var existingPrimary = await _context.Addresses
                .Where(a => a.OwnerType == request.OwnerType && a.OwnerId == request.OwnerId && a.IsPrimary)
                .ToListAsync(cancellationToken);

            foreach (var addr in existingPrimary)
            {
                addr.IsPrimary = false;
            }
        }

        var entity = new Address
        {
            TenantId = _currentUser.TenantId,
            OwnerType = request.OwnerType,
            OwnerId = request.OwnerId,
            Line1 = request.Line1,
            Line2 = request.Line2,
            District = request.District,
            City = request.City,
            State = request.State,
            Zip = request.Zip,
            Country = request.Country,
            GeoLat = request.GeoLat,
            GeoLng = request.GeoLng,
            IsPrimary = request.IsPrimary,
            Label = request.Label
        };

        entity.AddDomainEvent(new AddressCreatedEvent(entity));

        _context.Addresses.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AddressDto>(entity);
    }
}
```

#### ChannelIdentity Commands:
```
src/CleanAspire.Application/Features/Channels/Commands/
├── CreateChannelIdentityCommand.cs
├── UpdateChannelIdentityCommand.cs
├── DeleteChannelIdentityCommand.cs
├── SetPrimaryChannelCommand.cs
├── VerifyChannelCommand.cs
└── UpdateOptInCommand.cs
```

### 8. Queries (To Be Created)

#### Address Queries:
```
src/CleanAspire.Application/Features/Addresses/Queries/
├── GetAddressByIdQuery.cs
├── GetAddressesByOwnerQuery.cs
└── GetPrimaryAddressQuery.cs
```

**Template for GetAddressesByOwnerQuery.cs**:
```csharp
using CleanAspire.Application.Common.Interfaces;
using CleanAspire.Application.Features.Addresses.Caching;
using CleanAspire.Application.Features.Addresses.DTOs;
using CleanAspire.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanAspire.Application.Features.Addresses.Queries;

public record GetAddressesByOwnerQuery : IFusionCacheRequest<List<AddressDto>>
{
    public OwnerType OwnerType { get; init; }
    public string OwnerId { get; init; } = string.Empty;

    public string CacheKey => AddressCacheKey.GetByOwnerCacheKey(OwnerType, OwnerId);
    public TimeSpan? Duration => AddressCacheKey.Duration;
}

internal sealed class GetAddressesByOwnerQueryHandler : IRequestHandler<GetAddressesByOwnerQuery, List<AddressDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAddressesByOwnerQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<AddressDto>> Handle(GetAddressesByOwnerQuery request, CancellationToken cancellationToken)
    {
        var addresses = await _context.Addresses
            .Where(a => a.OwnerType == request.OwnerType && a.OwnerId == request.OwnerId)
            .OrderByDescending(a => a.IsPrimary)
            .ThenBy(a => a.Label)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<AddressDto>>(addresses);
    }
}
```

#### ChannelIdentity Queries:
```
src/CleanAspire.Application/Features/Channels/Queries/
├── GetChannelByIdQuery.cs
├── GetChannelsByOwnerQuery.cs
├── GetChannelsByTypeQuery.cs
└── GetPrimaryChannelsQuery.cs
```

### 9. Validators (To Be Created)

```
src/CleanAspire.Application/Features/Addresses/Commands/
└── CreateAddressCommandValidator.cs

src/CleanAspire.Application/Features/Channels/Commands/
└── CreateChannelIdentityCommandValidator.cs
```

**Template for CreateAddressCommandValidator.cs**:
```csharp
using FluentValidation;

namespace CleanAspire.Application.Features.Addresses.Commands;

public class CreateAddressCommandValidator : AbstractValidator<CreateAddressCommand>
{
    public CreateAddressCommandValidator()
    {
        RuleFor(v => v.OwnerId)
            .NotEmpty().WithMessage("Owner ID is required");

        RuleFor(v => v.Line1)
            .NotEmpty().WithMessage("Address Line 1 is required")
            .MaximumLength(200);

        RuleFor(v => v.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100);

        RuleFor(v => v.State)
            .NotEmpty().WithMessage("State is required")
            .MaximumLength(50);

        RuleFor(v => v.Zip)
            .NotEmpty().WithMessage("ZIP/Postal code is required")
            .MaximumLength(20);

        RuleFor(v => v.Country)
            .NotEmpty().WithMessage("Country is required")
            .Length(2).WithMessage("Country code must be 2 characters (ISO 3166-1 alpha-2)");

        RuleFor(v => v.GeoLat)
            .InclusiveBetween(-90, 90).When(v => v.GeoLat.HasValue)
            .WithMessage("Latitude must be between -90 and +90");

        RuleFor(v => v.GeoLng)
            .InclusiveBetween(-180, 180).When(v => v.GeoLng.HasValue)
            .WithMessage("Longitude must be between -180 and +180");
    }
}
```

### 10. API Endpoints (To Be Created)

#### Address Endpoints:
```
src/CleanAspire.Api/Endpoints/
└── AddressEndpointRegistrar.cs
```

**Template**:
```csharp
using CleanAspire.Application.Features.Addresses.Commands;
using CleanAspire.Application.Features.Addresses.DTOs;
using CleanAspire.Application.Features.Addresses.Queries;
using CleanAspire.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanAspire.Api.Endpoints;

public class AddressEndpointRegistrar : IEndpointRegistrar
{
    public void RegisterRoutes(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/crm/addresses")
            .WithTags("CRM.Addresses");

        // GET /api/crm/addresses/{id}
        group.MapGet("/{id}", GetAddressById)
            .WithName("GetAddressById")
            .WithSummary("Get address by ID")
            .Produces<AddressDto>()
            .Produces(404);

        // GET /api/crm/addresses/owner/{ownerType}/{ownerId}
        group.MapGet("/owner/{ownerType}/{ownerId}", GetAddressesByOwner)
            .WithName("GetAddressesByOwner")
            .WithSummary("Get all addresses for an owner")
            .Produces<List<AddressDto>>();

        // POST /api/crm/addresses
        group.MapPost("/", CreateAddress)
            .WithName("CreateAddress")
            .WithSummary("Create a new address")
            .Produces<AddressDto>(201)
            .ProducesValidationProblem();

        // PUT /api/crm/addresses
        group.MapPut("/", UpdateAddress)
            .WithName("UpdateAddress")
            .WithSummary("Update an existing address")
            .Produces(204)
            .Produces(404)
            .ProducesValidationProblem();

        // DELETE /api/crm/addresses/{id}
        group.MapDelete("/{id}", DeleteAddress)
            .WithName("DeleteAddress")
            .WithSummary("Delete an address")
            .Produces(204)
            .Produces(404);

        // POST /api/crm/addresses/{id}/set-primary
        group.MapPost("/{id}/set-primary", SetPrimaryAddress)
            .WithName("SetPrimaryAddress")
            .WithSummary("Set an address as primary")
            .Produces(204)
            .Produces(404);
    }

    private static async Task<IResult> GetAddressById(string id, ISender sender)
    {
        var query = new GetAddressByIdQuery(id);
        var result = await sender.Send(query);
        return result is not null ? Results.Ok(result) : Results.NotFound();
    }

    private static async Task<IResult> GetAddressesByOwner(
        OwnerType ownerType,
        string ownerId,
        ISender sender)
    {
        var query = new GetAddressesByOwnerQuery
        {
            OwnerType = ownerType,
            OwnerId = ownerId
        };
        var result = await sender.Send(query);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateAddress(
        CreateAddressCommand command,
        ISender sender)
    {
        var result = await sender.Send(command);
        return Results.Created($"/api/crm/addresses/{result.Id}", result);
    }

    private static async Task<IResult> UpdateAddress(
        UpdateAddressCommand command,
        ISender sender)
    {
        await sender.Send(command);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteAddress(string id, ISender sender)
    {
        await sender.Send(new DeleteAddressCommand { Id = id });
        return Results.NoContent();
    }

    private static async Task<IResult> SetPrimaryAddress(string id, ISender sender)
    {
        await sender.Send(new SetPrimaryAddressCommand { Id = id });
        return Results.NoContent();
    }
}
```

#### ChannelIdentity Endpoints:
```
src/CleanAspire.Api/Endpoints/
└── ChannelEndpointRegistrar.cs
```

### 11. AutoMapper Profiles (To Be Created)

```
src/CleanAspire.Application/Features/Addresses/DTOs/
└── AddressMappingProfile.cs

src/CleanAspire.Application/Features/Channels/DTOs/
└── ChannelIdentityMappingProfile.cs
```

**Template for AddressMappingProfile.cs**:
```csharp
using AutoMapper;
using CleanAspire.Domain.Entities;

namespace CleanAspire.Application.Features.Addresses.DTOs;

public class AddressMappingProfile : Profile
{
    public AddressMappingProfile()
    {
        CreateMap<Address, AddressDto>()
            .ForMember(dest => dest.FullAddress, opt => opt.MapFrom(src => src.FullAddress));

        CreateMap<AddressDto, Address>()
            .ForMember(dest => dest.DomainEvents, opt => opt.Ignore());
    }
}
```

### 12. Database Migration

```bash
# Navigate to migrators
cd src/Migrators/Migrators.SQLite

# Create migration
dotnet ef migrations add AddAddressAndChannelNormalization -s ../../CleanAspire.AppHost

# Review the generated migration file
# It should include:
# - Create Addresses table
# - Create ChannelIdentities table
# - Indexes and constraints

# Apply migration
dotnet ef database update -s ../../CleanAspire.AppHost
```

### 13. Data Migration Script

After the schema migration, you need to migrate existing inline data to the new tables:

```
src/Migrators/Migrators.SQLite/DataMigrations/
└── MigrateInlineAddressesAndChannels.cs
```

**Template**:
```csharp
using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Enums;
using CleanAspire.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Migrators.SQLite.DataMigrations;

/// <summary>
/// Migrates inline address and channel data to normalized tables
/// </summary>
public static class MigrateInlineAddressesAndChannels
{
    public static async Task ExecuteAsync(ApplicationDbContext context)
    {
        Console.WriteLine("Starting data migration: Inline Addresses and Channels...");

        await MigrateClientAddressesAsync(context);
        await MigrateClientChannelsAsync(context);
        await MigrateContactAddressesAsync(context);
        await MigrateContactChannelsAsync(context);

        await context.SaveChangesAsync();
        Console.WriteLine("Data migration completed successfully!");
    }

    private static async Task MigrateClientAddressesAsync(ApplicationDbContext context)
    {
        var clients = await context.Clients
            .Where(c => !string.IsNullOrEmpty(c.Address) || !string.IsNullOrEmpty(c.City))
            .ToListAsync();

        foreach (var client in clients)
        {
            if (string.IsNullOrEmpty(client.Address) && string.IsNullOrEmpty(client.City))
                continue;

            var address = new Address
            {
                TenantId = client.TenantId,
                OwnerType = OwnerType.Client,
                OwnerId = client.Id,
                Line1 = client.Address ?? "",
                City = client.City ?? "",
                State = client.State ?? "",
                Zip = client.PostalCode ?? "",
                Country = client.Country ?? "BR",
                IsPrimary = true,
                Label = "Primary"
            };

            context.Addresses.Add(address);
        }

        Console.WriteLine($"Migrated {clients.Count} client addresses");
    }

    private static async Task MigrateClientChannelsAsync(ApplicationDbContext context)
    {
        var clients = await context.Clients
            .Where(c => !string.IsNullOrEmpty(c.Email) ||
                       !string.IsNullOrEmpty(c.Phone) ||
                       !string.IsNullOrEmpty(c.Website))
            .ToListAsync();

        foreach (var client in clients)
        {
            if (!string.IsNullOrEmpty(client.Email))
            {
                context.ChannelIdentities.Add(new ChannelIdentity
                {
                    TenantId = client.TenantId,
                    OwnerType = OwnerType.Client,
                    OwnerId = client.Id,
                    Type = ChannelType.Email,
                    Value = client.Email,
                    IsPrimary = true,
                    Label = "Primary"
                });
            }

            if (!string.IsNullOrEmpty(client.Phone))
            {
                context.ChannelIdentities.Add(new ChannelIdentity
                {
                    TenantId = client.TenantId,
                    OwnerType = OwnerType.Client,
                    OwnerId = client.Id,
                    Type = ChannelType.Phone,
                    Value = client.Phone,
                    IsPrimary = true,
                    Label = "Primary"
                });
            }

            if (!string.IsNullOrEmpty(client.Website))
            {
                context.ChannelIdentities.Add(new ChannelIdentity
                {
                    TenantId = client.TenantId,
                    OwnerType = OwnerType.Client,
                    OwnerId = client.Id,
                    Type = ChannelType.Website,
                    Value = client.Website,
                    IsPrimary = true,
                    Label = "Primary"
                });
            }
        }

        Console.WriteLine($"Migrated channels for {clients.Count} clients");
    }

    private static async Task MigrateContactAddressesAsync(ApplicationDbContext context)
    {
        var contacts = await context.Contacts
            .Where(c => !string.IsNullOrEmpty(c.Address) || !string.IsNullOrEmpty(c.City))
            .ToListAsync();

        foreach (var contact in contacts)
        {
            if (string.IsNullOrEmpty(contact.Address) && string.IsNullOrEmpty(contact.City))
                continue;

            var address = new Address
            {
                TenantId = contact.TenantId,
                OwnerType = OwnerType.Contact,
                OwnerId = contact.Id,
                Line1 = contact.Address ?? "",
                City = contact.City ?? "",
                State = contact.State ?? "",
                Zip = contact.PostalCode ?? "",
                Country = "BR",
                IsPrimary = true,
                Label = "Primary"
            };

            context.Addresses.Add(address);
        }

        Console.WriteLine($"Migrated {contacts.Count} contact addresses");
    }

    private static async Task MigrateContactChannelsAsync(ApplicationDbContext context)
    {
        var contacts = await context.Contacts
            .Where(c => !string.IsNullOrEmpty(c.Email) ||
                       !string.IsNullOrEmpty(c.Phone) ||
                       !string.IsNullOrEmpty(c.Mobile))
            .ToListAsync();

        foreach (var contact in contacts)
        {
            if (!string.IsNullOrEmpty(contact.Email))
            {
                context.ChannelIdentities.Add(new ChannelIdentity
                {
                    TenantId = contact.TenantId,
                    OwnerType = OwnerType.Contact,
                    OwnerId = contact.Id,
                    Type = ChannelType.Email,
                    Value = contact.Email,
                    IsPrimary = true,
                    Label = "Primary"
                });
            }

            if (!string.IsNullOrEmpty(contact.Phone))
            {
                context.ChannelIdentities.Add(new ChannelIdentity
                {
                    TenantId = contact.TenantId,
                    OwnerType = OwnerType.Contact,
                    OwnerId = contact.Id,
                    Type = ChannelType.Phone,
                    Value = contact.Phone,
                    IsPrimary = true,
                    Label = "Office"
                });
            }

            if (!string.IsNullOrEmpty(contact.Mobile))
            {
                context.ChannelIdentities.Add(new ChannelIdentity
                {
                    TenantId = contact.TenantId,
                    OwnerType = OwnerType.Contact,
                    OwnerId = contact.Id,
                    Type = ChannelType.Mobile,
                    Value = contact.Mobile,
                    IsPrimary = true,
                    Label = "Personal"
                });
            }
        }

        Console.WriteLine($"Migrated channels for {contacts.Count} contacts");
    }
}
```

---

## 📝 Testing Checklist

### Unit Tests
- [ ] Address CRUD operations
- [ ] ChannelIdentity CRUD operations
- [ ] Primary flag behavior (only one primary per owner)
- [ ] Tenant isolation for addresses
- [ ] Tenant isolation for channels
- [ ] Channel verification logic
- [ ] Opt-in/opt-out logic

### Integration Tests
- [ ] API endpoints return correct data
- [ ] Multi-tenancy filtering works
- [ ] Unique constraints are enforced
- [ ] Data migration completes successfully
- [ ] Obsolete inline fields still work (backward compatibility)

### Manual Testing (via Scalar)
1. Access: `https://localhost:7341/scalar/v1`
2. Test endpoints:
   - `POST /api/crm/addresses` - Create address
   - `GET /api/crm/addresses/owner/{ownerType}/{ownerId}` - List addresses
   - `POST /api/crm/channels` - Create channel
   - `GET /api/crm/channels/owner/{ownerType}/{ownerId}` - List channels

---

## 🚀 Next Steps

### Immediate (Today):
1. Create the remaining Commands (see templates above)
2. Create the remaining Queries (see templates above)
3. Create Validators
4. Create API Endpoints

### After Commands/Queries Complete:
1. Create AutoMapper profiles
2. Run `dotnet build` to ensure everything compiles
3. Create and apply EF migration
4. Create and execute data migration script
5. Test via Scalar

### Optional Enhancements (Future):
1. **Geocoding Service**: Integrate with Google Maps API or OpenCage to auto-populate GeoLat/GeoLng
2. **Channel Verification**: Implement OTP verification for email/phone
3. **Address Validation**: Integrate with postal service APIs to validate addresses
4. **Channel Format Validation**: Add regex validators for phone numbers, emails, etc.

---

## 📚 References

- **CRM Roadmap**: `CRM-ROADMAP.md` - Phase 2 (Week 5)
- **Phase 0-1 Guide**: `PHASE-0-1-CLEANASPIRE-ALIGNED.md`
- **CleanAspire Patterns**: `CRUD-IMPLEMENTATION-GUIDE.md`

---

## 🐛 Common Issues & Solutions

### Issue: Migration fails with "column already exists"
**Solution**: Drop and recreate database, or manually alter the migration file

### Issue: Obsolete warnings everywhere
**Solution**: This is expected! The warnings remind developers to use the new entities. Suppress with `#pragma warning disable CS0618` if needed.

### Issue: Primary flag not working correctly
**Solution**: Ensure you're un-setting IsPrimary on existing records before setting a new one

### Issue: Channels not filtering by tenant
**Solution**: Verify query filters are applied in ApplicationDbContext

---

**Status**: 🟢 Foundation Complete, Ready for Command/Query Implementation

**Last Updated**: 2025-10-31

**Next Milestone**: Complete all Commands and Queries, then proceed to migration
