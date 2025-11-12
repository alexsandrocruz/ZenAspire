# Fase 5: Segmentation Engine - Planejamento Detalhado

## Overview
**Objetivo:** Implementar motor de segmentação dinâmica com base em regras JSON para agrupamento inteligente de clientes e contatos.

**Período:** Semanas 9-10 (10 dias úteis)
**Status:** 🟡 Planejamento - Pronto para iniciar implementação

---

## Entidades a Serem Implementadas

### 1. Segment
```csharp
public class Segment : BaseAuditableEntity
{
    public Guid Id { get; set; }
    public string TenantId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string DefinitionJson { get; set; } // jsonb
    public bool IsActive { get; set; }
    public DateTime? LastRebuiltAt { get; set; }
    public int? MemberCount { get; set; }
}
```

### 2. SegmentMembership
```csharp
public class SegmentMembership
{
    public Guid SegmentId { get; set; }
    public string TenantId { get; set; }
    public OwnerType OwnerType { get; set; }
    public Guid OwnerId { get; set; }
    public DateTime ComputedAt { get; set; }

    // Chave composta
    // (SegmentId, TenantId, OwnerType, OwnerId)
}
```

---

## JSON DSL para Definição de Segmentos

### Estrutura JSON:
```json
{
  "name": "Clientes Educação - Ativos Recentes",
  "description": "Clientes do setor Educação com interação nos últimos 30 dias",
  "rules": {
    "all": [
      {
        "field": "ClientType",
        "op": "eq",
        "value": "School"
      },
      {
        "field": "LastInteractionDays",
        "op": "<=",
        "value": 30
      },
      {
        "any": [
          {"field": "LifecycleStage", "op": "eq", "value": "Client"},
          {"field": "LifecycleStage", "op": "eq", "value": "Prospect"}
        ]
      }
    ]
  }
}
```

### Operadores Suportados:
- **Comparação:** `eq`, `ne`, `gt`, `gte`, `lt`, `lte`
- **String:** `contains`, `startsWith`, `endsWith`, `in`
- **Data:** `daysAgo`, `withinLast`, `before`, `after`
- **Lógicos:** `all` (AND), `any` (OR), `not` (NOT)
- **Especiais:** `hasTag`, `inSegment`, `hasInteractionOfType`

---

## Status Atual das Atividades

### ✅ Concluído
- [x] Análise de requisitos (Phase 5 no roadmap)
- [x] Definição das entidades e estrutura JSON
- [x] Planejamento de APIs e serviços
- [x] Decisão por Elsa Workflows (substituindo Hangfire)

### 🔄 Em Andamento
- [ ] Configuração do ambiente de desenvolvimento
- [ ] Preparação da estrutura de projetos

### ⏳ Pendentes

#### 1. Camada Domain (1 dia)
- [ ] Criar entidade `Segment`
- [ ] Criar classe `SegmentMembership`
- [ ] Definir enums `OwnerType` e `SegmentOperator`
- [ ] Implementar validações de negócio

#### 2. Camada Infrastructure (1 dia)
- [ ] Configurar EF Core mappings para Segment
- [ ] Configurar EF Core mappings para SegmentMembership
- [ ] Implementar parser JSON DSL
- [ ] Criar índices otimizados:
  ```sql
  CREATE INDEX ix_segment_memberships_composite
  ON SegmentMemberships (SegmentId, TenantId, OwnerType, OwnerId);
  ```

#### 3. Camada Application (3 dias)
- [ ] Commands:
  - [ ] `CreateSegmentCommand`
  - [ ] `UpdateSegmentCommand`
  - [ ] `DeleteSegmentCommand`
  - [ ] `RebuildSegmentCommand`
- [ ] Queries:
  - [ ] `GetSegmentsQuery`
  - [ ] `GetSegmentByIdQuery`
  - [ ] `GetSegmentMembersQuery`
  - [ ] `GetSegmentStatsQuery`
- [ ] Validators:
  - [ ] `CreateSegmentValidator`
  - [ ] `UpdateSegmentValidator`
  - [ ] `SegmentDefinitionValidator`
- [ ] DTOs:
  - [ ] `SegmentDto`
  - [ ] `SegmentMembershipDto`
  - [ ] `SegmentDefinitionDto`

#### 4. Motor de Segmentação (2 dias)
- [ ] Implementar `SegmentRuleEngine`
- [ ] Implementar `ExpressionBuilder` (LINQKit)
- [ ] Implementar `SegmentRebuilderService`
- [ ] Implementar testes unitários para o motor
- [ ] Implementar caching de resultados

#### 5. APIs REST (1 dia)
- [ ] `GET /api/crm/segments` - Listar segmentos
- [ ] `POST /api/crm/segments` - Criar segmento
- [ ] `GET /api/crm/segments/{id}` - Detalhe do segmento
- [ ] `PUT /api/crm/segments/{id}` - Atualizar segmento
- [ ] `DELETE /api/crm/segments/{id}` - Excluir segmento
- [ ] `POST /api/crm/segments/{id}/rebuild` - Reconstruir membros
- [ ] `GET /api/crm/segments/{id}/members` - Listar membros
- [ ] `GET /api/crm/segments/{id}/stats` - Estatísticas do segmento

#### 6. Elsa Workflows Integration (1.5 dias)
- [ ] Configurar Elsa Workflows no AppHost
- [ ] Criar workflow definition para `SegmentRebuildWorkflow`
- [ ] Implementar atividades customizadas:
  - [ ] `EvaluateSegmentRulesActivity`
  - [ ] `BuildSegmentMembershipActivity`
  - [ ] `NotifySegmentCompleteActivity`
- [ ] Criar `SegmentAutoRebuildWorkflow` (agendamento diário)
- [ ] Configurar dashboard de monitoramento no Elsa Studio
- [ ] Implementar triggers manuais via API
- [ ] Configurar persistência com PostgreSQL

#### 7. Testes (0.5 dia)
- [ ] Testes unitários:
  - [ ] Parser JSON DSL
  - [ ] Rule Engine
  - [ ] Command handlers
  - [ ] Query handlers
- [ ] Testes de integração:
  - [ ] APIs completas
  - [ ] Elsa workflows
  - [ ] Performance com grandes volumes

---

## Dependencies

### Dependencies Técnicas
- [ ] LINQKit (para dynamic LINQ)
- [ ] Elsa Workflows (recomendado v3+)
- [ ] Elsa.Workflows.Activities (atividades padrão)
- [ ] Elsa.Persistence.EntityFramework (integração com EF Core)
- [ ] Npgsql.EntityFrameworkCore.PostgreSQL (jsonb support)
- [ ] FluentValidation

### Dependencies do Projeto
- [ ] Phase 0-4 concluídas ✅
- [ ] Infraestrutura .NET Aspire configurada
- [ ] Database com suporte a jsonb (PostgreSQL recomendado)

---

## Arquitetura Elsa Workflows

### Workflow: Segment Rebuild
```csharp
public class SegmentRebuildWorkflow : WorkflowBase
{
    protected override void Build(IWorkflowBuilder builder)
    {
        builder.Name = "Segment Rebuild Workflow";

        builder.StartWith<ReceiveSegmentIdActivity>()
            .Then<ValidateSegmentExistsActivity>()
            .Then<EvaluateSegmentRulesActivity>()
            .Then<BuildSegmentMembershipActivity>()
            .Then<UpdateSegmentStatsActivity>()
            .Then<NotifySegmentCompleteActivity>()
            .Then<FinishWorkflowActivity>();
    }
}
```

### Atividades Customizadas
1. **EvaluateSegmentRulesActivity**
   - Parse JSON DSL
   - Executa regras contra database
   - Retorna lista de IDs

2. **BuildSegmentMembershipActivity**
   - Limpa memberships existentes
   - Insere novos membros em lote
   - Atualiza timestamp

3. **NotifySegmentCompleteActivity**
   - Publica evento `SegmentRebuiltEvent`
   - Envia notificações (opcional)
   - Registra métricas

### Benefits do Elsa vs Hangfire
- ✅ Interface visual para design/editação de workflows
- � Suporte nativo a branching e conditionals complexos
- ✅ Reusabilidade de atividades entre workflows
- ✅ Dashboard integrado para monitoramento
- ✅ Versionamento de workflows
- ✅ Persistência e recovery automáticos

---

## Riscos e Mitigações

### Risco 1: Curva de Aprendizado do Elsa
- **Impacto:** Médio
- **Probabilidade:** Alta
- **Mitigação:**
  - Iniciar com workflows simples
  - Usar atividades padrão quando possível
  - Documentar atividades customizadas

### Risco 2: Performance com Grandes Volumes
- **Impacto:** Alto
- **Probabilidade:** Média
- **Mitigação:**
  - Implementar paginação eficiente
  - Usar índices compostos
  - Cache de segmentos frequentes
  - Processamento em lotes dentro do workflow

### Risco 3: Concorrência em Workflows
- **Impacto:** Médio
- **Probabilidade:** Média
- **Mitigação:**
  - Implementar locking no nível de atividade
  - Usar correlation ID por segmento
  - Configurar políticas de retry

---

## Deliverables

### Código
- [ ] Entidades Domain
- [ ] Mapeamentos EF Core
- [ ] Application layer completa
- [ ] APIs REST
- [ ] Elsa workflows e atividades
- [ ] Testes automatizados

### Workflows
- [ ] `SegmentRebuildWorkflow` (manual trigger)
- [ ] `SegmentAutoRebuildWorkflow` (scheduled)
- [ ] Template para futuros workflows de CRM

### Documentação
- [ ] API docs (OpenAPI)
- [ ] JSON DSL reference
- [ ] Elsa workflows documentation
- [ ] Guia de implementação

### Performance
- [ ] < 3s para rebuild de segmento com 10K registros
- [ ] < 500ms para query de membros paginada
- [ ] Suporte a 100+ segmentos por tenant

---

## Próximos Passos Imediatos

1. **Hoje:**
   - [ ] Setup do projeto e dependencies
   - [ ] Configurar Elsa Workflows no AppHost
   - [ ] Criar entidades básicas no Domain

2. **Esta semana:**
   - [ ] Implementar Application layer
   - [ ] Desenvolver atividades customizadas Elsa
   - [ ] Criar primeiro workflow

3. **Próxima semana:**
   - [ ] APIs e workflows completos
   - [ ] Testes e performance tuning
   - [ ] Dashboard e monitoramento

---

## Status Checklist

### Fase 5 - Segmentation Engine

- [ ] **Task 1:** Domain Entities (1 dia)
- [ ] **Task 2:** Infrastructure Setup (1 dia)
- [ ] **Task 3:** Application Layer (3 dias)
- [ ] **Task 4:** Rule Engine (2 dias)
- [ ] **Task 5:** APIs (1 dia)
- [ ] **Task 6:** Elsa Workflows (1.5 dias)
- [ ] **Task 7:** Testing (0.5 dia)

**Progresso Geral:** 0/10 dias (0%)

---

## Notas

- Elsa Workflows oferece melhor visibilidade e manutenibilidade que Hangfire
- Possibilidade de expor interface visual para usuários avançados criarem workflows customizados
- Preparar integração com Phase 12 (Segment UI) usando dashboard do Elsa
- Considerar migração de outros background jobs futuros para Elsa

---

*Última atualização: 2025-11-05*
*Status: 🟡 Planejamento Concluído - Pronto para Implementação*