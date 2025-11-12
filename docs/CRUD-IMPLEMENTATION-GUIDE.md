# 🚀 CleanAspire - Guia Completo para Implementação de CRUD

Este guia fornece um passo-a-passo detalhado para implementar um sistema CRUD completo para uma nova entidade no projeto **CleanAspire**. Baseado na experiência prática de implementação do CRUD de Customer.

---

## 📋 Pré-requisitos

- Projeto CleanAspire configurado e funcionando
- Entidade de domínio já criada em `Domain/Entities`
- Visual Studio Code com extensões .NET

---

## 🎯 Visão Geral da Arquitetura

O CleanAspire segue a **Clean Architecture** com as seguintes camadas:

- **Domain**: Entidades e eventos de domínio
- **Application**: Lógica de negócio, Commands, Queries e DTOs
- **API**: Endpoints RESTful com Minimal APIs
- **ClientApp**: Interface Blazor WebAssembly com MudBlazor

---

## 📝 Checklist de Implementação

### ✅ Fase 1: Application Layer (Backend)

- [ ] 1.1 Criar DTO
- [ ] 1.2 Implementar Commands (Create, Update, Delete, Import)
- [ ] 1.3 Implementar Queries (GetById, Pagination, GetAll, Export)
- [ ] 1.4 Criar Validators
- [ ] 1.5 Implementar Command/Query Handlers
- [ ] 1.6 Criar Domain Events

### ✅ Fase 2: API Layer

- [ ] 2.1 Criar EndpointRegistrar
- [ ] 2.2 Implementar todos os endpoints RESTful
- [ ] 2.3 Testar compilação da API

### ✅ Fase 3: Client Layer (Frontend)

- [ ] 3.1 Criar ServiceProxy
- [ ] 3.2 Implementar página Index
- [ ] 3.3 Criar dialogs (New, Edit)
- [ ] 3.4 Configurar navegação
- [ ] 3.5 Testar compilação do ClientApp

### ✅ Fase 4: Testes e Validação

- [ ] 4.1 Executar migrations (se necessário)
- [ ] 4.2 Testar endpoints via Scalar
- [ ] 4.3 Testar interface do usuário
- [ ] 4.4 Validar funcionalidade completa

---

## 🔧 Implementação Detalhada

### 1️⃣ **Fase 1: Application Layer**

#### 1.1 Criar DTO

**Local**: `src/CleanAspire.Application/Features/{EntidadePlural}/DTOs/{Entidade}Dto.cs`

```csharp
namespace CleanAspire.Application.Features.Customers.DTOs;

public class CustomerDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime? Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
}
```

#### 1.2 Implementar Commands

**Create Command**:
**Local**: `src/CleanAspire.Application/Features/{EntidadePlural}/Commands/Create{Entidade}Command.cs`

```csharp
namespace CleanAspire.Application.Features.Customers.Commands;

public record CreateCustomerCommand : IFusionCacheRefreshRequest<CustomerDto>, IRequiresValidation
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string CacheKey => CustomerCacheKey.GetAllCacheKey;
    public CancellationToken? SharedExpiryToken => CustomerCacheKey.SharedExpiryToken;
}

internal sealed class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateCustomerCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var entity = new Customer
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address
        };

        entity.AddDomainEvent(new CustomerCreatedEvent(entity));
        _context.Customers.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<CustomerDto>(entity);
    }
}
```

**Update Command**:
**Local**: `src/CleanAspire.Application/Features/{EntidadePlural}/Commands/Update{Entidade}Command.cs`

```csharp
public record UpdateCustomerCommand : IFusionCacheRefreshRequest<Unit>, IRequiresValidation
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string CacheKey => CustomerCacheKey.GetAllCacheKey;
    public CancellationToken? SharedExpiryToken => CustomerCacheKey.SharedExpiryToken;
}

internal sealed class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Unit>
{
    // Implementação similar ao Create, mas com Update
}
```

**Delete Command**:
**Local**: `src/CleanAspire.Application/Features/{EntidadePlural}/Commands/Delete{Entidade}Command.cs`

```csharp
public record DeleteCustomerCommand(string Id) : IFusionCacheRefreshRequest<Unit>, IRequiresValidation
{
    public string CacheKey => CustomerCacheKey.GetAllCacheKey;
    public CancellationToken? SharedExpiryToken => CustomerCacheKey.SharedExpiryToken;
}
```

#### 1.3 Implementar Queries

**GetById Query**:
```csharp
public record GetCustomerByIdQuery(string Id) : IFusionCacheRequest<CustomerDto?>
{
    public string CacheKey => CustomerCacheKey.GetByIdCacheKey(Id);
    public TimeSpan? Duration => CustomerCacheKey.Duration;
}
```

**Pagination Query**:
```csharp
public record CustomersWithPaginationQuery : IFusionCacheRequest<PaginatedResult<CustomerDto>>
{
    public CustomerListView ListView { get; init; } = CustomerListView.All;
    public string Keyword { get; init; } = string.Empty;
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 15;
    public string OrderBy { get; init; } = nameof(CustomerDto.Name);
    public SortDirection SortDirection { get; init; } = SortDirection.Descending;
    public string CacheKey => CustomerCacheKey.GetPaginationCacheKey($"{ListView}-{Keyword}-{Page}-{Size}-{OrderBy}-{SortDirection}");
    public TimeSpan? Duration => CustomerCacheKey.Duration;
}
```

#### 1.4 Criar Validators

**Local**: `src/CleanAspire.Application/Features/{EntidadePlural}/Commands/{Command}Validator.cs`

```csharp
public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(v => v.Name)
            .MaximumLength(256)
            .NotEmpty();
            
        RuleFor(v => v.Email)
            .MaximumLength(256)
            .EmailAddress()
            .NotEmpty();
            
        RuleFor(v => v.Phone)
            .MaximumLength(20);
            
        RuleFor(v => v.Address)
            .MaximumLength(500);
    }
}
```

#### 1.5 Cache Keys

**Local**: `src/CleanAspire.Application/Features/{EntidadePlural}/Caching/{Entidade}CacheKey.cs`

```csharp
public static class CustomerCacheKey
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromHours(1);
    private static readonly TimeSpan DefaultDuration = TimeSpan.FromHours(2);
    public static TimeSpan? Duration => DefaultDuration;
    public static string GetAllCacheKey => "all-customers";
    public static string GetPaginationCacheKey(string parameters) => $"customers-pagination-{parameters}";
    public static string GetByIdCacheKey(string id) => $"customer-by-id-{id}";
    public static string GetExportCacheKey(string parameters) => $"customers-export-{parameters}";
    
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

#### 1.6 Domain Events

**Local**: `src/CleanAspire.Domain/Events/{Entidade}Events.cs`

```csharp
public record CustomerCreatedEvent(Customer Item) : DomainEvent;
public record CustomerUpdatedEvent(Customer Item) : DomainEvent;
public record CustomerDeletedEvent(Customer Item) : DomainEvent;
```

---

### 2️⃣ **Fase 2: API Layer**

#### 2.1 Criar EndpointRegistrar

**Local**: `src/CleanAspire.Api/Endpoints/{Entidade}EndpointRegistrar.cs`

```csharp
namespace CleanAspire.Api.Endpoints;

public class CustomerEndpointRegistrar : IEndpointRegistrar
{
    public void RegisterRoutes(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/customers").WithTags("Customers");

        group.MapGet("/", GetAllCustomers)
            .WithName("GetAllCustomers")
            .WithSummary("Get all customers")
            .Produces<List<CustomerDto>>();

        group.MapGet("/{id}", GetCustomerById)
            .WithName("GetCustomerById")
            .WithSummary("Get customer by ID")
            .Produces<CustomerDto>()
            .Produces(404);

        group.MapPost("/", CreateCustomer)
            .WithName("CreateCustomer")
            .WithSummary("Create a new customer")
            .Produces<CustomerDto>(201)
            .ProducesValidationProblem();

        group.MapPut("/", UpdateCustomer)
            .WithName("UpdateCustomer")
            .WithSummary("Update an existing customer")
            .Produces(204)
            .Produces(404)
            .ProducesValidationProblem();

        group.MapDelete("/{id}", DeleteCustomer)
            .WithName("DeleteCustomer")
            .WithSummary("Delete a customer")
            .Produces(204)
            .Produces(404);

        group.MapPost("/pagination", GetCustomersWithPagination)
            .WithName("GetCustomersWithPagination")
            .WithSummary("Get customers with pagination")
            .Produces<PaginatedResult<CustomerDto>>();

        group.MapGet("/export", ExportCustomers)
            .WithName("ExportCustomers")
            .WithSummary("Export customers to CSV")
            .Produces<byte[]>("text/csv");

        group.MapPost("/import", ImportCustomers)
            .WithName("ImportCustomers")
            .WithSummary("Import customers from CSV")
            .Produces(204)
            .ProducesValidationProblem()
            .DisableAntiforgery();
    }

    private static async Task<IResult> GetAllCustomers(ISender sender)
    {
        var query = new GetAllCustomersQuery();
        var result = await sender.Send(query);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetCustomerById(string id, ISender sender)
    {
        var query = new GetCustomerByIdQuery(id);
        var result = await sender.Send(query);
        return result != null ? Results.Ok(result) : Results.NotFound();
    }

    private static async Task<IResult> CreateCustomer(CreateCustomerCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return Results.Created($"/customers/{result.Id}", result);
    }

    private static async Task<IResult> UpdateCustomer(UpdateCustomerCommand command, ISender sender)
    {
        await sender.Send(command);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteCustomer(string id, ISender sender)
    {
        await sender.Send(new DeleteCustomerCommand(id));
        return Results.NoContent();
    }

    private static async Task<IResult> GetCustomersWithPagination(CustomersWithPaginationQuery query, ISender sender)
    {
        var result = await sender.Send(query);
        return Results.Ok(result);
    }

    private static async Task<IResult> ExportCustomers(ExportCustomersQuery query, ISender sender)
    {
        var result = await sender.Send(query);
        return Results.File(result, "application/octet-stream", "customers.csv");
    }

    private static async Task<IResult> ImportCustomers(IFormFile file, ISender sender)
    {
        await sender.Send(new ImportCustomersCommand(file.OpenReadStream()));
        return Results.NoContent();
    }
}
```

---

### 3️⃣ **Fase 3: Client Layer (Frontend)**

#### 3.1 Criar ServiceProxy

**Local**: `src/CleanAspire.ClientApp/Services/{Entidade}ServiceProxy.cs`

```csharp
namespace CleanAspire.ClientApp.Services;

public class CustomerServiceProxy
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomerServiceProxy> _logger;

    public CustomerServiceProxy(HttpClient httpClient, ILogger<CustomerServiceProxy> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<CustomerDto>> GetAllAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<CustomerDto>>("/customers") ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all customers");
            return new();
        }
    }

    public async Task<CustomerDto?> GetByIdAsync(string id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<CustomerDto>($"/customers/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer {Id}", id);
            return null;
        }
    }

    public async Task<PaginatedResult<CustomerDto>> GetWithPaginationAsync(CustomersWithPaginationQuery query)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/customers/pagination", query);
            return await response.Content.ReadFromJsonAsync<PaginatedResult<CustomerDto>>() ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customers with pagination");
            return new();
        }
    }

    public async Task<CustomerDto?> CreateAsync(CreateCustomerCommand command)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/customers", command);
            return await response.Content.ReadFromJsonAsync<CustomerDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer");
            return null;
        }
    }

    public async Task<bool> UpdateAsync(UpdateCustomerCommand command)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync("/customers", command);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer");
            return false;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/customers/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer {Id}", id);
            return false;
        }
    }
}
```

**Registrar no DI**:
**Local**: `src/CleanAspire.ClientApp/DependencyInjection.cs`

```csharp
// Adicionar na seção de serviços:
services.AddScoped<CustomerServiceProxy>();
```

#### 3.2 Implementar Página Index

**Local**: `src/CleanAspire.ClientApp/Pages/Customers/Index.razor`

```razor
@page "/customers"
@using CleanAspire.Application.Features.Customers.DTOs
@using CleanAspire.Application.Features.Customers.Queries
@using CleanAspire.ClientApp.Services
@inject CustomerServiceProxy CustomerService
@inject IDialogService DialogService
@inject ISnackbar Snackbar

<PageTitle>Customers</PageTitle>

<MudContainer MaxWidth="MaxWidth.ExtraLarge" Class="pt-4">
    <MudText Typo="Typo.h4" Class="mb-4">Customer Management</MudText>
    
    <MudPaper Class="pa-4 mb-4">
        <MudGrid AlignItems="Center">
            <MudItem xs="12" md="8">
                <MudTextField @bind-Value="searchTerm" 
                            Placeholder="Search customers..." 
                            Adornment="Adornment.Start" 
                            AdornmentIcon="Icons.Material.Filled.Search"
                            OnKeyPress="@(async (e) => { if (e.Key == "Enter") await SearchCustomers(); })" />
            </MudItem>
            <MudItem xs="12" md="4" Class="d-flex justify-end">
                <MudButton Variant="Variant.Filled" 
                          Color="Color.Primary" 
                          StartIcon="Icons.Material.Filled.Add"
                          OnClick="OpenNewCustomerDialog">
                    New Customer
                </MudButton>
            </MudItem>
        </MudGrid>
    </MudPaper>

    <MudDataGrid @ref="dataGrid" 
                 T="CustomerDto" 
                 ServerData="LoadServerData" 
                 Hover="true"
                 Striped="true"
                 Dense="true">
        <Columns>
            <PropertyColumn Property="x => x.Name" Title="Name" />
            <PropertyColumn Property="x => x.Email" Title="Email" />
            <PropertyColumn Property="x => x.Phone" Title="Phone" />
            <PropertyColumn Property="x => x.Address" Title="Address" />
            <PropertyColumn Property="x => x.Created" Title="Created" Format="dd/MM/yyyy" />
            <TemplateColumn Title="Actions" Sortable="false">
                <CellTemplate>
                    <MudIconButton Icon="Icons.Material.Filled.Edit" 
                                  Color="Color.Primary" 
                                  Size="Size.Small"
                                  OnClick="@(() => EditCustomer(context.Item))" />
                    <MudIconButton Icon="Icons.Material.Filled.Delete" 
                                  Color="Color.Error" 
                                  Size="Size.Small"
                                  OnClick="@(() => DeleteCustomer(context.Item))" />
                </CellTemplate>
            </TemplateColumn>
        </Columns>
        <PagerContent>
            <MudDataGridPager T="CustomerDto" />
        </PagerContent>
    </MudDataGrid>
</MudContainer>

@code {
    private MudDataGrid<CustomerDto> dataGrid = null!;
    private string searchTerm = string.Empty;

    private async Task<GridData<CustomerDto>> LoadServerData(GridState<CustomerDto> state)
    {
        var query = new CustomersWithPaginationQuery
        {
            Page = state.Page + 1,
            Size = state.PageSize,
            Keyword = searchTerm,
            OrderBy = state.SortDefinitions.FirstOrDefault()?.SortBy ?? nameof(CustomerDto.Name),
            SortDirection = state.SortDefinitions.FirstOrDefault()?.Descending == true ? 
                           SortDirection.Descending : SortDirection.Ascending
        };

        var result = await CustomerService.GetWithPaginationAsync(query);
        
        return new GridData<CustomerDto>
        {
            Items = result.Items,
            TotalItems = result.TotalCount
        };
    }

    private async Task SearchCustomers()
    {
        await dataGrid.ReloadServerData();
    }

    private async Task OpenNewCustomerDialog()
    {
        var dialog = await DialogService.ShowAsync<NewCustomerDialog>("New Customer");
        var result = await dialog.Result;
        
        if (!result.Canceled)
        {
            await dataGrid.ReloadServerData();
            Snackbar.Add("Customer created successfully!", Severity.Success);
        }
    }

    private async Task EditCustomer(CustomerDto customer)
    {
        // Implementar dialog de edição
        Snackbar.Add("Edit functionality coming soon!", Severity.Info);
    }

    private async Task DeleteCustomer(CustomerDto customer)
    {
        var confirmed = await DialogService.ShowMessageBox(
            "Confirm Delete",
            $"Are you sure you want to delete customer '{customer.Name}'?",
            yesText: "Delete",
            cancelText: "Cancel");

        if (confirmed == true)
        {
            var success = await CustomerService.DeleteAsync(customer.Id);
            if (success)
            {
                await dataGrid.ReloadServerData();
                Snackbar.Add("Customer deleted successfully!", Severity.Success);
            }
            else
            {
                Snackbar.Add("Error deleting customer.", Severity.Error);
            }
        }
    }
}
```

#### 3.3 Criar Dialog de Criação

**Local**: `src/CleanAspire.ClientApp/Pages/Customers/Components/NewCustomerDialog.razor`

```razor
@using CleanAspire.Application.Features.Customers.Commands
@using CleanAspire.ClientApp.Services
@inject CustomerServiceProxy CustomerService
@inject ISnackbar Snackbar

<MudDialog>
    <DialogContent>
        <MudContainer Style="max-width: 500px;">
            <MudTextField @bind-Value="command.Name" 
                         Label="Name" 
                         Required="true"
                         Class="mb-3" />
            
            <MudTextField @bind-Value="command.Email" 
                         Label="Email" 
                         Required="true"
                         InputType="InputType.Email"
                         Class="mb-3" />
            
            <MudTextField @bind-Value="command.Phone" 
                         Label="Phone" 
                         Class="mb-3" />
            
            <MudTextField @bind-Value="command.Address" 
                         Label="Address" 
                         Lines="3"
                         Class="mb-3" />
        </MudContainer>
    </DialogContent>
    
    <DialogActions>
        <MudButton OnClick="Cancel">Cancel</MudButton>
        <MudButton Color="Color.Primary" 
                  Variant="Variant.Filled" 
                  OnClick="Save"
                  Disabled="@(!IsValid())">
            Create
        </MudButton>
    </DialogActions>
</MudDialog>

@code {
    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;

    private CreateCustomerCommand command = new()
    {
        Name = string.Empty,
        Email = string.Empty,
        Phone = string.Empty,
        Address = string.Empty
    };

    private bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(command.Name) && 
               !string.IsNullOrWhiteSpace(command.Email);
    }

    private async Task Save()
    {
        var result = await CustomerService.CreateAsync(command);
        if (result != null)
        {
            MudDialog.Close(DialogResult.Ok(result));
        }
        else
        {
            Snackbar.Add("Error creating customer.", Severity.Error);
        }
    }

    private void Cancel() => MudDialog.Cancel();
}
```

#### 3.4 Configurar Navegação

**Local**: `src/CleanAspire.ClientApp/Layout/NavMenu.razor`

```razor
<!-- Adicionar no grupo Catalog: -->
<MudNavLink Href="/customers" Match="NavLinkMatch.Prefix" Icon="@Icons.Material.Filled.People">Customers</MudNavLink>
```

---

### 4️⃣ **Fase 4: Testes e Validação**

#### 4.1 Testar Compilação

```powershell
# Testar Application
dotnet build src/CleanAspire.Application/CleanAspire.Application.csproj

# Testar API
dotnet build src/CleanAspire.Api/CleanAspire.Api.csproj

# Testar ClientApp
dotnet build src/CleanAspire.ClientApp/CleanAspire.ClientApp.csproj
```

#### 4.2 Executar Aplicação

```powershell
# Executar Aspire AppHost
dotnet run --project src/CleanAspire.AppHost/CleanAspire.AppHost.csproj
```

#### 4.3 Testar Endpoints

1. Acesse o Scalar: `https://localhost:7341/scalar/v1`
2. Teste todos os endpoints da entidade
3. Verifique respostas e validações

#### 4.4 Testar Interface

1. Acesse a aplicação cliente
2. Navegue para a página da entidade
3. Teste todas as funcionalidades (CRUD)

---

## 🛠️ Comandos Úteis

```powershell
# Compilar tudo
dotnet build

# Executar testes
dotnet test

# Limpar e recompilar
dotnet clean && dotnet build

# Executar aplicação
dotnet run --project src/CleanAspire.AppHost/CleanAspire.AppHost.csproj

# Verificar portas em uso
netstat -ano | findstr :7341
```

---

## 📁 Estrutura de Arquivos Final

```text
src/
├── CleanAspire.Domain/
│   └── Entities/{Entidade}.cs
│   └── Events/{Entidade}Events.cs
├── CleanAspire.Application/
│   └── Features/{EntidadePlural}/
│       ├── DTOs/{Entidade}Dto.cs
│       ├── Commands/
│       │   ├── Create{Entidade}Command.cs
│       │   ├── Update{Entidade}Command.cs
│       │   ├── Delete{Entidade}Command.cs
│       │   └── Import{EntidadePlural}Command.cs
│       ├── Queries/
│       │   ├── Get{Entidade}ByIdQuery.cs
│       │   ├── {EntidadePlural}WithPaginationQuery.cs
│       │   ├── GetAll{EntidadePlural}Query.cs
│       │   └── Export{EntidadePlural}Query.cs
│       └── Caching/{Entidade}CacheKey.cs
├── CleanAspire.Api/
│   └── Endpoints/{Entidade}EndpointRegistrar.cs
└── CleanAspire.ClientApp/
    ├── Services/{Entidade}ServiceProxy.cs
    └── Pages/{EntidadePlural}/
        ├── Index.razor
        ├── Edit.razor
        └── Components/
            ├── New{Entidade}Dialog.razor
            └── _Imports.razor
```

---

## 🐛 Problemas Comuns e Soluções

Esta seção documenta problemas reais encontrados durante a implementação dos CRUDs de **Client** e **Contact**, com suas causas e soluções.

### ❌ Problema 1: CORS Error - Mixed Content (HTTP/HTTPS)

**Erro Observado**:

```text
Access to fetch at 'http://localhost:5519/account/profile' from origin 'https://localhost:7114'
has been blocked by CORS policy: No 'Access-Control-Allow-Origin' header is present
```

**Causa**:
O ClientApp estava configurado para acessar a API via HTTP (`http://localhost:5519`) mas a API estava rodando em HTTPS (`https://localhost:7341`), causando bloqueio por política de CORS.

**Solução**:
Corrigir a URL base no arquivo `appsettings.Development.json` do ClientApp:

```json
// ❌ ERRADO
{
  "ClientAppSettings": {
    "ServiceBaseUrl": "http://localhost:5519"
  }
}

// ✅ CORRETO
{
  "ClientAppSettings": {
    "ServiceBaseUrl": "https://localhost:7341"
  }
}
```

**Arquivo**: `src/CleanAspire.ClientApp/wwwroot/appsettings.Development.json`

---

### ❌ Problema 2: PageNumber Zero-Based - Paginação Retorna 0 Resultados

**Erro Observado**:
```
✅ Got 0 clients successfully
```
Dados existem no banco, mas a paginação retorna 0 resultados.

**Causa**:
O sistema de paginação usa **zero-based indexing** (página 0 = primeira página), mas o ServiceProxy estava enviando `PageNumber = 1`, causando:
```csharp
Skip(1 * 1000) = Skip(1000) // Pula os primeiros 1000 registros!
```

**Solução**:
Subtrair 1 do pageNumber ao fazer a requisição:

```csharp
// ❌ ERRADO
var request = new
{
    PageNumber = pageNumber, // Se pageNumber=1, pula 1000 registros!
    PageSize = pageSize
};

// ✅ CORRETO
var request = new
{
    PageNumber = pageNumber - 1, // API uses zero-based indexing (0 = first page)
    PageSize = pageSize
};
```

**Arquivos Corrigidos**:
- `src/CleanAspire.ClientApp/Services/Clients/ClientServiceProxy.cs`
- `src/CleanAspire.ClientApp/Services/Contacts/ContactServiceProxy.cs`

---

### ❌ Problema 3: Método HTTP Incorreto - GET vs POST para Paginação

**Erro Observado**:
```
Error 404: Not Found
```

**Causa**:
O ServiceProxy estava usando `GetAsync` com query string, mas o endpoint da API está configurado como `POST`:

```csharp
// Na API (ClientEndpointRegistrar.cs)
group.MapPost("/pagination", ...) // Endpoint é POST!
```

**Solução**:
Usar `PostAsJsonAsync` ao invés de `GetAsync`:

```csharp
// ❌ ERRADO
var queryString = $"pageNumber={pageNumber}&pageSize={pageSize}";
var response = await _httpClient.GetAsync($"/api/clients/pagination?{queryString}");

// ✅ CORRETO
var request = new
{
    Keywords = searchTerm ?? string.Empty,
    PageNumber = pageNumber - 1,
    PageSize = pageSize,
    OrderBy = "Name",
    SortDirection = "Ascending"
};
var response = await _httpClient.PostAsJsonAsync("/api/clients/pagination", request);
```

**Lição Aprendida**: Sempre verificar o verbo HTTP do endpoint na API antes de implementar o ServiceProxy.

---

### ❌ Problema 4: Id.ToString() Desnecessário - String para String

**Erro Observado**:
Compilação funcionando, mas conversão desnecessária que pode causar problemas.

**Causa**:
As entidades usam `Id` do tipo `string`, mas os handlers estavam fazendo `.ToString()`:

```csharp
// Entidade (BaseEntity.cs)
public virtual string Id { get; set; } = Guid.CreateVersion7().ToString();

// Handler - conversão desnecessária
Id = client.Id.ToString(), // ❌ string.ToString() é redundante
```

**Solução**:
Remover todas as chamadas `.ToString()` em propriedades que já são strings:

```csharp
// ❌ ERRADO
return new ClientDto
{
    Id = client.Id.ToString(), // Desnecessário!
    ClientId = contact.ClientId.ToString() // Desnecessário!
};

// ✅ CORRETO
return new ClientDto
{
    Id = client.Id, // Já é string!
    ClientId = contact.ClientId // Já é string!
};
```

**Arquivos Corrigidos** (12 arquivos):
- `CreateClientCommand.cs`
- `CreateContactCommand.cs`
- `GetAllClientsQuery.cs`
- `GetClientByIdQuery.cs`
- `ClientsWithPaginationQuery.cs`
- `GetAllContactsQuery.cs`
- `GetContactByIdQuery.cs`
- `GetContactsByClientIdQuery.cs`
- `ContactsWithPaginationQuery.cs`
- E outros...

---

### ❌ Problema 5: Validator Faltando - CreateContactCommand

**Erro Observado**:
```
Error 422: Unprocessable Entity (Validation Error)
```
Falha silenciosa sem mensagem clara de erro.

**Causa**:
O `CreateContactCommand` não tinha um validator correspondente, causando falhas de validação sem feedback adequado.

**Solução**:
Criar o validator seguindo o padrão FluentValidation:

```csharp
// CreateContactCommandValidator.cs
public class CreateContactCommandValidator : AbstractValidator<CreateContactCommand>
{
    public CreateContactCommandValidator()
    {
        RuleFor(command => command.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

        RuleFor(command => command.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

        RuleFor(command => command.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(150).WithMessage("Email must not exceed 150 characters.");

        RuleFor(command => command.ClientId)
            .NotEmpty().WithMessage("Client ID is required.");
    }
}
```

**Lição Aprendida**: Sempre criar validators para commands que implementam `IRequiresValidation`.

---

### ❌ Problema 6: ClientId Faltando no Formulário de Contact

**Erro Observado**:
```
Error 422: Validation failed - ClientId is required
```

**Causa**:
O formulário `NewContactDialog` não tinha um campo para selecionar o Client, então o `ClientId` era enviado como `null` ou vazio.

**Solução 1**: Adicionar campo de seleção de Client no formulário:

```razor
<!-- NewContactDialog.razor -->
<MudSelect T="string"
            @bind-Value="_contactModel.ClientId"
            Label="@L["Client *"]"
            Required="true"
            RequiredError="@L["Client is required"]">
    @foreach (var client in _clients)
    {
        <MudSelectItem Value="@client.Id">@client.DisplayName</MudSelectItem>
    }
</MudSelect>
```

**Solução 2**: Carregar lista de clientes no OnInitializedAsync:

```csharp
protected override async Task OnInitializedAsync()
{
    await LoadClients();
}

private async Task LoadClients()
{
    var result = await ClientServiceProxy.GetClientsWithPaginationAsync(1, 1000);
    _clients = result?.Items?.ToList() ?? new List<ClientDto>();
}
```

**Solução 3**: Adicionar parâmetro ClientId opcional para pré-selecionar:

```csharp
[Parameter]
public string? ClientId { get; set; }

// No OnParametersSet
if (!string.IsNullOrEmpty(ClientId))
{
    _contactModel.ClientId = ClientId;
}
```

---

### ❌ Problema 7: Campo Email Não Obrigatório no Formulário

**Erro Observado**:
```
Error 422: Email is required
```
Formulário permitia submissão sem email, mas o validator da API exigia.

**Causa**:
O campo Email no formulário não estava marcado como `Required="true"`, permitindo envio de valores vazios.

**Solução**:
Adicionar validação no campo:

```razor
<!-- ❌ ERRADO -->
<MudTextField T="string"
              @bind-Value="_contactModel.Email"
              Label="@L["Email"]"
              Type="InputType.Email" />

<!-- ✅ CORRETO -->
<MudTextField T="string"
              @bind-Value="_contactModel.Email"
              Label="@L["Email *"]"
              Type="InputType.Email"
              Required="true"
              RequiredError="@L["Email is required"]" />
```

**Lição Aprendida**: Campos obrigatórios na API devem ser obrigatórios no formulário também. Use o asterisco (*) no label para indicar.

---

### ❌ Problema 8: DTO vs Command Mismatch - Propriedades Incompatíveis

**Erro Observado**:
```
Error 422: Validation failed
```

**Causa**:
O `ContactDto` estava sendo enviado diretamente para a API, mas ele tinha propriedades diferentes do `CreateContactCommand`:

- ContactDto usa `Tags` → CreateContactCommand usa `ContactTags`
- ContactDto tem propriedades extras (Id, ClientName, ClientDisplayName, Created, etc.) que não existem no Command

**Solução**:
Criar um objeto de request anônimo que mapeia corretamente as propriedades:

```csharp
// ❌ ERRADO - Envia DTO diretamente
var response = await _httpClient.PostAsJsonAsync("/api/contacts", contactDto);

// ✅ CORRETO - Mapeia para estrutura do Command
var request = new
{
    contactDto.FirstName,
    contactDto.LastName,
    contactDto.Email,
    contactDto.Phone,
    contactDto.MobilePhone,
    contactDto.JobTitle,
    contactDto.Department,
    contactDto.Address,
    contactDto.City,
    contactDto.State,
    contactDto.PostalCode,
    contactDto.Notes,
    ContactTags = contactDto.Tags, // Mapear Tags → ContactTags
    Type = contactDto.Type,
    Status = contactDto.Status,
    contactDto.IsMainContact,
    contactDto.IsDecisionMaker,
    contactDto.BirthDate,
    contactDto.ClientId
};
var response = await _httpClient.PostAsJsonAsync("/api/contacts", request);
```

**Lição Aprendida**:
- DTOs são para leitura (responses)
- Commands são para escrita (requests)
- Sempre mapear corretamente entre eles no ServiceProxy

---

### ❌ Problema 9: Autenticação Bloqueando Endpoints em Desenvolvimento

**Erro Observado**:
```
Error 401: Unauthorized
```

**Causa**:
Endpoints estavam configurados com `.RequireAuthorization()` mas durante desenvolvimento inicial não havia autenticação configurada.

**Solução Temporária** (apenas para desenvolvimento):
Comentar temporariamente o RequireAuthorization:

```csharp
// ClientEndpointRegistrar.cs
public void RegisterRoutes(IEndpointRouteBuilder routes)
{
    var group = routes.MapGroup("/api/clients")
        .WithTags("clients");
        // .RequireAuthorization(); // TODO: Re-enable after testing
}
```

**⚠️ IMPORTANTE**: Reativar autenticação antes de deploy em produção!

**Solução Definitiva**:
Implementar autenticação correta com cookies/JWT no cliente.

---

### ✅ Checklist de Validação Pós-Implementação

Use esta checklist para evitar os problemas acima:

- [ ] **URLs configuradas corretamente** (HTTPS, não HTTP)
- [ ] **PageNumber zero-based** nos ServiceProxies (pageNumber - 1)
- [ ] **Método HTTP correto** (POST para paginação, não GET)
- [ ] **Sem `.ToString()` desnecessário** em IDs que já são string
- [ ] **Validator criado** para todos os Commands que requerem validação
- [ ] **Campos obrigatórios na API** também são obrigatórios no formulário
- [ ] **ClientId/Foreign Keys** têm seleção no formulário
- [ ] **Mapeamento correto** entre DTO e Command (propriedades compatíveis)
- [ ] **Email com validação Required** se obrigatório na API
- [ ] **Autenticação configurada** (ou desabilitada temporariamente com TODO)
- [ ] **Logging adequado** no ServiceProxy para debug
- [ ] **Testes manuais** de todos os endpoints antes do frontend

---

### 🔍 Debug Tips

**1. Console do Navegador (F12)**:
Sempre verifique o console para ver:
- Erros de requisição HTTP (status code)
- Logs do ServiceProxy
- Erros de JavaScript/Blazor

**2. Scalar/Swagger** (`https://localhost:7341/scalar/v1`):
Teste endpoints diretamente antes de implementar o frontend.

**3. Logs do Aspire Dashboard**:
Monitore as requisições e respostas HTTP em tempo real.

**4. Adicione Logging Detalhado nos ServiceProxies**:
```csharp
Console.WriteLine($"📝 Creating {entity}: {entity.Name}");
Console.WriteLine($"   Request: {System.Text.Json.JsonSerializer.Serialize(request)}");
// ... fazer requisição ...
Console.WriteLine($"✅ {Entity} created successfully with ID: {result?.Id}");
```

---

## ⚠️ Dicas Importantes

1. **Nomenclatura**: Seja consistente com singular/plural
2. **Cache Keys**: Implemente sempre para performance
3. **Validadores**: Use FluentValidation em todos os commands
4. **Domain Events**: Adicione eventos para auditoria
5. **Error Handling**: Trate erros adequadamente no frontend
6. **Testes**: Compile frequentemente para detectar erros cedo
7. **Scalar**: Use para testar API antes do frontend
8. **PageNumber Zero-Based**: Sempre subtrair 1 no ServiceProxy
9. **HTTP Methods**: POST para paginação, não GET
10. **Required Fields**: Sincronizar entre API validators e formulários
11. **DTO vs Command**: Mapear corretamente as propriedades

---

## 🎉 Conclusão

Seguindo este guia passo-a-passo, você terá um sistema CRUD completo e funcional para qualquer entidade no CleanAspire. O padrão é consistente e escalável, seguindo as melhores práticas da Clean Architecture.

**Tempo estimado**: 2-4 horas para implementação completa de uma nova entidade.

---

## 📅 Histórico de Atualizações

- **Outubro 2024**: Versão inicial baseada na implementação do Customer CRUD
- **Outubro 2024**: Adicionada seção "🐛 Problemas Comuns e Soluções" com 9 problemas documentados e resolvidos durante a implementação dos CRUDs de Client e Contact, incluindo:
  - CORS errors (HTTP/HTTPS mismatch)
  - PageNumber zero-based indexing
  - Método HTTP incorreto (GET vs POST)
  - Conversões desnecessárias (Id.ToString())
  - Validators faltando
  - Foreign keys não mapeadas no frontend
  - DTO vs Command mismatch
  - Checklist de validação pós-implementação
  - Debug tips e logging

---

**Mantido por**: Equipe de Desenvolvimento ZenAspire
**Última atualização**: 31 de Outubro de 2025
