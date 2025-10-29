# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

CleanAspire is a .NET 10 Clean Architecture template featuring Minimal APIs, Blazor WebAssembly PWA, and .NET Aspire integration. The project follows Clean Architecture principles with clear separation of concerns across multiple layers.

## Common Development Commands

### Running the Application
```bash
# Run the entire solution (uses Aspire orchestration)
dotnet run

# Run specific projects
dotnet run --project src/CleanAspire.Api
dotnet run --project src/CleanAspire.WebApp
dotnet run --project src/CleanAspire.AppHost
```

### Building and Testing
```bash
# Build the entire solution
dotnet build

# Run tests (uses NUnit framework)
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run tests for specific project
dotnet test tests/CleanAspire.Tests/
```

### Database Migrations
```bash
# Set startup project to CleanAspire.AppHost or CleanAspire.Api first

# Add migration (replace <MigrationName> with your migration name)
dotnet ef migrations add <MigrationName> -p src/Migrators/Migrators.SQLite/

# Update database
dotnet ef database update -p src/Migrators/Migrators.SQLite/

# For other database providers, use corresponding project:
# - src/Migrators/Migrators.MSSQL/
# - src/Migrators/Migrators.PostgreSQL/
```

## Architecture Overview

### Solution Structure
The solution follows Clean Architecture with these main layers:

- **CleanAspire.Domain**: Core entities, domain interfaces, and business logic
- **CleanAspire.Application**: Application services, DTOs, validators, and business use cases
- **CleanAspire.Infrastructure**: EF Core implementations, external services, and persistence
- **CleanAspire.Api**: Minimal API endpoints, authentication, and HTTP pipeline configuration
- **CleanAspire.WebApp**: Blazor WebAssembly PWA client application
- **CleanAspire.AppHost**: .NET Aspire orchestration and service configuration
- **CleanAspire.ClientApp**: Additional client application (optional)
- **Migrators**: Database migration projects for different providers (SQLite, SQL Server, PostgreSQL)

### Key Architectural Patterns

#### Entity Base Classes
- `BaseEntity`: Base entity with Id property
- `BaseAuditableEntity`: Adds audit fields (Created, CreatedBy, LastModified, LastModifiedBy)
- `BaseAuditableSoftDeleteEntity`: Adds soft delete capability

#### Database Context
- `IApplicationDbContext`: Interface defining DbSets for all entities
- `ApplicationDbContext`: EF Core implementation with Identity integration
- Entities: Client, Contact, Product, Stock, Tenant, AuditTrail

#### API Architecture
- Minimal APIs with endpoint registration pattern using `IEndpointRegistrar`
- Automatic endpoint discovery with `MapEndpointDefinitions()`
- Scalar for OpenAPI documentation (replaces Swagger)
- Integrated authentication with Google and Microsoft providers
- CORS configuration for Blazor client applications

#### Current Entity Status
Based on git status, the project is transitioning from Customer-based entities to Client/Contact entities:
- **Removed**: Customer entity and all related CRUD operations
- **Added**: Client and Contact entities with their respective features
- **Active**: Product, Stock, Tenant, and AuditTrail entities

### Configuration
The application uses multiple `appsettings.json` files for different environments:
- Database provider selection (SQLite, SQL Server, PostgreSQL)
- Authentication configuration (Google, Microsoft)
- CORS origins configuration
- External services (SendGrid, Webpushr, MinIO)
- Serilog logging configuration

### Testing Framework
- **NUnit**: Primary testing framework
- **Aspire.Hosting.Testing**: For integration testing with Aspire
- Test project: `tests/CleanAspire.Tests/`

## Development Notes

### Adding New Entities
When adding new entities, follow this pattern:
1. Create entity class inheriting from `BaseAuditableEntity` in `Domain/Entities/`
2. Add DbSet to `IApplicationDbContext` interface
3. Implement DbSet in `ApplicationDbContext`
4. Create entity configuration in `Infrastructure/Persistence/Configurations/`
5. Generate and apply migrations
6. Create Application layer features (Commands, Queries, DTOs, Validators)
7. Add API endpoints using endpoint registrar pattern

### Endpoint Registration
API endpoints are automatically discovered and registered using the `IEndpointRegistrar` interface. Classes implementing this interface are automatically registered with scoped lifetime.

### Authentication & Authorization
- Uses ASP.NET Core Identity with Entity Framework
- Supports external authentication (Google, Microsoft)
- Email confirmation required for sign-in
- Antiforgery protection enabled

### Database Provider Support
The application supports multiple database providers through configuration:
- SQLite (default for development)
- SQL Server
- PostgreSQL
Each provider has its own migration project in the `Migrators/` folder.