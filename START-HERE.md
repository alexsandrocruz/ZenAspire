# 🚀 GenZ CRM Implementation - START HERE

## 📌 Quick Overview

Este projeto implementa um **CRM completo multi-tenant** dentro do CleanAspire, seguindo a especificação do documento **"Gen Z Crm (CleanAspire) — DDD Spec, Data Model, APIs & UI.pdf"**.

---

## 📚 Documentação Principal

### 1. **CRM-ROADMAP.md** - Visão Estratégica Completa
   - 📊 Roadmap de 14 fases (22 semanas)
   - 🎯 Análise: O que temos vs. O que falta
   - 📈 KPIs e métricas de sucesso
   - ⚠️ Gestão de riscos e mitigações
   - 🧪 Estratégia de testes

### 2. **PHASE-0-1-CLEANASPIRE-ALIGNED.md** - Guia Prático Step-by-Step
   - ✅ **Phase 0**: Multi-Tenancy Foundation (Week 1-2)
   - ✅ **Phase 1**: Tags, Notes, Attachments (Week 3-4)
   - 💻 Código completo seguindo padrões do CleanAspire
   - 📝 Checklist detalhado para validação
   - ⏱️ Estimativas de tempo por task

### 3. **CRUD-IMPLEMENTATION-GUIDE.md** (Existing)
   - 📖 Convenções e padrões estabelecidos do CleanAspire
   - 🛠️ Template para criar novos CRUDs
   - 🐛 Problemas comuns e soluções

---

## 🎯 Estado Atual vs. Target

### ✅ O Que Já Temos:
- **Entities**: Client, Contact (com CRUD completo)
- **Architecture**: Clean Architecture + DDD
- **API**: Minimal APIs com Endpoint Registrars
- **UI**: Blazor WASM + MudBlazor
- **Cache**: FusionCache
- **Database**: EF Core + Migrations
- **Multi-project**: Domain, Application, Infrastructure, API, ClientApp

### ❌ O Que Falta para o CRM Completo:
1. **Multi-tenancy** - TenantId não está nas entidades (Phase 0)
2. **Tags/Notes normalizadas** - Atualmente são strings inline (Phase 1)
3. **Timeline** - Activity, Interaction entities (Phase 3)
4. **LGPD** - Consent, data export/erasure (Phase 4)
5. **Segmentation** - Segment engine com JSON DSL (Phase 5)
6. **Advanced Features** - Custom fields, relationships, etc. (Phase 6+)

---

## 🚀 Como Começar

### Opção 1: Implementação Manual (Recomendado para aprendizado)

**Siga este fluxo:**

```bash
1. Leia: CRM-ROADMAP.md (visão geral)
2. Leia: PHASE-0-1-CLEANASPIRE-ALIGNED.md (guia prático)
3. Implemente: Phase 0 - Task 0.1 (Add TenantId to Client)
4. Continue seguindo os tasks em ordem
```

**Início imediato:**
```bash
# Task 0.1: Update Client entity
code src/CleanAspire.Domain/Entities/Client.cs

# Adicione:
[Required]
[MaxLength(450)]
public string TenantId { get; set; } = string.Empty;

[MaxLength(200)]
public string? LegalName { get; set; }

[MaxLength(32)]
public string? TaxId { get; set; }

[MaxLength(32)]
public string LifecycleStage { get; set; } = "Lead";

[MaxLength(450)]
public string? OwnerUserId { get; set; }
```

### Opção 2: Pedir Assistência do Claude Code

```bash
# No Claude Code, diga:
"Comece a implementar Phase 0, Task 0.1 do CRM roadmap"

# Ou especificamente:
"Adicione TenantId à entidade Client seguindo o guia PHASE-0-1-CLEANASPIRE-ALIGNED.md"
```

---

## 📋 Checklist de Validação (Phase 0)

Antes de considerar Phase 0 completo, valide:

### Entities:
- [ ] Client tem `TenantId`, `LegalName`, `TaxId`, `LifecycleStage`, `OwnerUserId`
- [ ] Contact tem `TenantId`, `Mobile`, `LifecycleStage`, `OwnerUserId`
- [ ] ClientType enum tem `School = 5`

### EF Configurations:
- [ ] Unique index: `(TenantId, TaxId)` on Client
- [ ] Unique index: `(TenantId, Email)` on Contact
- [ ] Tenant indexes para performance

### Services:
- [ ] `ICurrentUserService` criado e registrado no DI
- [ ] `CurrentUserService` implementado com HttpContext
- [ ] Global query filters adicionados ao DbContext

### Domain Events:
- [ ] `ClientCreatedEvent(Client Item)` - passa entidade completa
- [ ] `ContactCreatedEvent(Contact Item)` - passa entidade completa

### API:
- [ ] Rotas migradas: `/customers` → `/api/crm/clients`
- [ ] Rotas migradas: `/contacts` → `/api/crm/contacts`
- [ ] Tags OpenAPI: `CRM.Clients`, `CRM.Contacts`

### Commands:
- [ ] `CreateClientCommand` injeta `ICurrentUserService`
- [ ] Handler seta `TenantId` do current user
- [ ] Handler valida `TenantId` não é null
- [ ] `TaxId` é normalizado (uppercase, sem caracteres especiais)

### Tests:
- [ ] `TestCurrentUserService` criado para testes
- [ ] Teste: Client criado com TenantId correto
- [ ] Teste: Query filtra por tenant (não vê outros tenants)
- [ ] Teste: Unique constraint por tenant funciona

### Migration:
- [ ] Migration criada: `AddMultiTenancyToCrm`
- [ ] Migration aplicada: `dotnet ef database update`
- [ ] Dados existentes tem TenantId default

---

## 🎓 Padrões CleanAspire (OBRIGATÓRIO SEGUIR)

### 1. **Command Pattern**
```csharp
// ✅ CORRETO
public record CreateClientCommand : IFusionCacheRefreshRequest<ClientDto>, IRequiresValidation
{
    public string Name { get; init; } = string.Empty;
    // ...
    public string CacheKey => ClientCacheKey.GetAllCacheKey;
    public CancellationToken? SharedExpiryToken => ClientCacheKey.SharedExpiryToken;
}

// Handler retorna DTO, não Result<T>
internal sealed class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, ClientDto>
{
    public async Task<ClientDto> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        // ...
        return _mapper.Map<ClientDto>(entity);
    }
}
```

### 2. **Query Pattern**
```csharp
// ✅ CORRETO
public record GetClientByIdQuery(string Id) : IFusionCacheRequest<ClientDto?>
{
    public string CacheKey => ClientCacheKey.GetByIdCacheKey(Id);
    public TimeSpan? Duration => ClientCacheKey.Duration;
}
```

### 3. **Domain Events**
```csharp
// ✅ CORRETO - Passa entidade completa
entity.AddDomainEvent(new ClientCreatedEvent(entity));

// ❌ ERRADO - Não passar apenas propriedades
// entity.AddDomainEvent(new ClientCreatedEvent(entity.Id, entity.TenantId, entity.Name));
```

### 4. **Endpoint Registrar**
```csharp
// ✅ CORRETO
public class ClientEndpointRegistrar : IEndpointRegistrar
{
    public void RegisterRoutes(IEndpointRouteBuilder routes) // RegisterRoutes, não RegisterEndpoints!
    {
        var group = routes.MapGroup("/api/crm/clients")
            .WithTags("CRM.Clients");
    }
}
```

### 5. **Validators**
```csharp
// ✅ CORRETO - AbstractValidator do FluentValidation
public class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200);
    }
}
```

### 6. **Service Proxy (Frontend)**
```csharp
// ✅ CORRETO - HttpClient com logging
public class ClientServiceProxy
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ClientServiceProxy> _logger;

    public async Task<ClientDto?> CreateAsync(CreateClientCommand command)
    {
        try
        {
            Console.WriteLine($"📝 Creating client: {command.Name}");
            var response = await _httpClient.PostAsJsonAsync("/api/crm/clients", command);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ClientDto>();
            Console.WriteLine($"✅ Client created: {result?.Id}");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating client");
            return null;
        }
    }
}
```

### 7. **Pagination (Zero-Based)**
```csharp
// ✅ CORRETO - API usa zero-based (0 = primeira página)
var request = new
{
    PageNumber = pageNumber - 1, // Se UI passa 1, API recebe 0
    PageSize = pageSize
};
```

---

## 🛠️ Comandos Úteis

```bash
# Compilar
dotnet build

# Rodar aplicação
dotnet run --project src/CleanAspire.AppHost/CleanAspire.AppHost.csproj

# Criar migration
cd src/Migrators/Migrators.SQLite
dotnet ef migrations add MigrationName -s ../../CleanAspire.AppHost

# Aplicar migration
dotnet ef database update -s ../../CleanAspire.AppHost

# Testar
dotnet test

# Acessar Scalar (API docs)
https://localhost:7341/scalar/v1

# Acessar Aspire Dashboard
https://localhost:15000 (ou porta exibida no console)
```

---

## 📊 Timeline Visual

```
📅 Week 1-2:  [==== Phase 0 ====] Multi-Tenancy Foundation
📅 Week 3-4:  [==== Phase 1 ====] Tags, Notes, Attachments
📅 Week 5:    [=== Phase 2 ===]  Address & Channel Normalization
📅 Week 6-7:  [==== Phase 3 ====] Timeline (Activities & Interactions)
📅 Week 8:    [=== Phase 4 ===]  LGPD Compliance
📅 Week 9-10: [==== Phase 5 ====] Segmentation Engine
... (see CRM-ROADMAP.md for phases 6-14)
```

---

## ⚠️ Coisas Importantes para NÃO Esquecer

1. **Sempre injetar `ICurrentUserService`** em commands/queries que precisam de TenantId
2. **Global query filters** garantem isolamento de tenants automaticamente
3. **Normalized TaxId** - sempre uppercase, sem caracteres especiais
4. **Commands retornam DTOs**, não `Result<T>` (padrão CleanAspire)
5. **Domain events passam a entidade completa**, não apenas propriedades
6. **RegisterRoutes**, não `RegisterEndpoints` (método correto do IEndpointRegistrar)
7. **Zero-based pagination** na API (page 0 = primeira página)
8. **DTO ≠ Command** - sempre mapear corretamente no ServiceProxy
9. **Required fields no validator** devem ser `Required="true"` no formulário também
10. **Test multi-tenancy** - sempre criar testes com múltiplos tenants

---

## 🆘 Ajuda e Suporte

### Documentação Interna:
- **CRUD-IMPLEMENTATION-GUIDE.md** - Seção "🐛 Problemas Comuns e Soluções"
- **CRM-ROADMAP.md** - Seção "Risk Management"

### Debug Tips:
1. Console do navegador (F12) - veja erros HTTP
2. Scalar (`https://localhost:7341/scalar/v1`) - teste APIs diretamente
3. Aspire Dashboard - monitore logs em tempo real
4. Adicione `Console.WriteLine` nos ServiceProxies para debug

### Quando Pedir Ajuda:
- Se seguiu o guia exatamente e algo não funciona
- Se encontrou erro de compilação não documentado
- Se não entendeu algum padrão do CleanAspire
- Se precisa de esclarecimento sobre DDD/Clean Architecture

---

## ✅ Pronto para Começar?

**Fluxo recomendado para hoje:**

1. ✅ **[5 min]** Leia este arquivo completo (START-HERE.md)
2. 📖 **[15 min]** Leia CRM-ROADMAP.md (entenda a visão geral)
3. 📝 **[10 min]** Leia PHASE-0-1-CLEANASPIRE-ALIGNED.md (Task 0.1 a 0.3)
4. 💻 **[30 min]** Implemente Task 0.1 (Add TenantId to Client)
5. 💻 **[30 min]** Implemente Task 0.2 (Add TenantId to Contact)
6. 💻 **[20 min]** Implemente Task 0.3 (ICurrentUserService)
7. ☕ **Break!**
8. 💻 **[40 min]** Implemente Task 0.4 (Global Query Filters)
9. 💻 **[30 min]** Implemente Task 0.5 (Update Domain Events)
10. 🧪 **[30 min]** Teste tudo com Scalar

**Tempo total estimado Day 1**: 4 horas

**Resultado esperado ao fim do dia:**
- ✅ Multi-tenancy funcionando
- ✅ Client e Contact com TenantId
- ✅ Testes passando
- ✅ APIs retornando dados filtrados por tenant

---

## 📞 Próximos Passos

Após completar Phase 0:
1. Commitar código: `git commit -m "feat: add multi-tenancy support (Phase 0)"`
2. Criar PR para review (opcional)
3. Começar Phase 1 (Tags, Notes, Attachments)

**Lembre-se:**
- Cada fase entrega valor funcional completo (vertical slice)
- Testar frequentemente (Scalar + UI)
- Seguir os padrões do CleanAspire **religiosamente**
- Adicionar logging detalhado para debug

---

**Boa sorte! 🚀**

**Precisa de ajuda?** Releia a seção de documentação ou use o Claude Code para esclarecer dúvidas.

---

**Criado:** 2025-10-31
**Versão:** 1.0
**Status:** 🟢 Pronto para Uso
