# GenZ CRM Implementation Roadmap

## Executive Summary

This roadmap outlines the implementation plan for transforming CleanAspire into a full-featured, multi-tenant CRM system aligned with the **GenZ CRM DDD Specification**. The implementation follows a **vertical slice architecture**, delivering complete features incrementally.

**Target Stack:** .NET 10, Minimal APIs, Blazor WebAssembly, EF Core, PostgreSQL/SQLite
**Architecture:** Clean Architecture + DDD, Modular Monolith (microservices-ready)
**Cross-Products:** Sapienza, LicitaTek, EducaTec, Fabio Ribeiro Advogados

---

## Current State Analysis

### ✅ What We Have (MVP Foundation)

**Entities:**
- `Client` - Complete with business info, inline address, tags, notes (D:\@dev\ZenAspire\src\CleanAspire.Domain\Entities\Client.cs:6)
- `Contact` - Complete with personal info, inline address (D:\@dev\ZenAspire\src\CleanAspire.Domain\Entities\Contact.cs:6)
- `Tenant` - Basic multi-tenancy support (D:\@dev\ZenAspire\src\CleanAspire.Domain\Entities\Tenant.cs:14)
- Relationship: Client 1:N Contact (cascade delete)

**Features:**
- CRUD operations for Client and Contact
- Queries: GetAll, GetById, WithPagination, GetContactsByClientId
- Commands: Create, Update, Delete (soft delete ready)
- DTOs, Validators, Cache Keys
- API Endpoints via `IEndpointRegistrar` pattern
- Domain Events (ClientEvents, ContactEvents)
- EF Core configurations with indexes
- Audit trail support via `BaseAuditableEntity`

**Infrastructure:**
- ApplicationDbContext with Identity integration
- Database migrations (SQLite, SQL Server, PostgreSQL)
- Clean Architecture layer separation
- Aspire orchestration

### ❌ What's Missing (Per CRM Spec)

**Core Gaps:**
1. **Multi-tenancy:** TenantId not on Client/Contact entities + no query filters
2. **Unique constraints per tenant:** (TenantId, DocumentNumber), (TenantId, Email)
3. **Normalized entities:** Address, ChannelIdentity, Tag, Note (currently inline strings)
4. **Timeline/Interactions:** No Activity, Interaction, or unified timeline
5. **LGPD Compliance:** No Consent entity, data export/erasure workflows
6. **Segmentation:** No Segment/TagLink for dynamic grouping
7. **Relationships:** No Account-to-Account relationships (Parent/Subsidiary/Partner)
8. **Custom Fields:** No schema/value extensibility
9. **Domain Events:** No Outbox pattern, no MassTransit integration
10. **UI:** No Blazor pages for CRM workflows

**API Gaps:**
- `/api/crm/clients/{id}/timeline`
- `/api/crm/clients/{id}/notes`
- `/api/crm/clients/{id}/activities`
- `/api/crm/clients/{id}/consents`
- `/api/crm/clients/{id}/channels`
- `/api/crm/clients/{id}/addresses`
- `/api/crm/segments`
- `/api/crm/relationships`
- `/api/crm/search/global`

---

## Implementation Phases (Vertical Slices)

### 🎯 Phase 0: Foundation & Multi-Tenancy (Week 1-2)

**Goal:** Add multi-tenancy, normalize current data model, align with CRM spec

#### Tasks:
1. **Add TenantId to Client and Contact**
   - Add `TenantId` property (string, required)
   - Update EF configurations with composite indexes:
     - `(TenantId, DocumentNumber)` on Client (unique, nullable filter)
     - `(TenantId, Email)` on Contact (unique)
   - Add global query filter for `TenantId` in ApplicationDbContext
   - Generate migration: `AddTenantIdToClientContact`

2. **Extend Client Entity (align with Account spec)**
   - Add `LegalName` (string?, 200)
   - Rename `DocumentNumber` → `TaxId` (normalize: uppercase, no special chars)
   - Add `LifecycleStage` (string, 32) - "Lead", "Prospect", "Client", "Former"
   - Add `OwnerUserId` (string?) - assigned internal owner
   - Expand `ClientType` enum: add `School`, align with spec

3. **Extend Contact Entity**
   - Add nullable `AccountId` for future M:N support (keep `ClientId` required for now)
   - Add `LifecycleStage` (string, 32)
   - Add `OwnerUserId` (string?)
   - Update `Mobile` → `MobilePhone` to align with spec (E.164 format validation)

4. **Update Domain Events**
   - Rename events: `CRM.ClientCreated`, `CRM.ContactCreated`
   - Add properties: `{ ClientId, TenantId, Name }`

5. **Update API Routes**
   - Change base route: `/api/clients` → `/api/crm/clients`
   - Change base route: `/api/contacts` → `/api/crm/contacts`
   - Update OpenAPI tags: `CRM.Clients`, `CRM.Contacts`

6. **Testing**
   - Update integration tests for multi-tenancy
   - Test tenant isolation (no cross-tenant queries)
   - Test unique constraints per tenant

**Deliverables:**
- ✅ Multi-tenant Client and Contact entities
- ✅ Migration scripts
- ✅ Updated API routes
- ✅ Passing tests

---

### 🎯 Phase 1: Core CRM Entities - Tags, Notes, Attachments (Week 3-4)

**Goal:** Normalize tags and notes, add file attachments, enable rich annotations

#### New Entities:
1. **Tag**
   ```csharp
   public class Tag : BaseAuditableEntity {
       Guid Id, string TenantId, string Name (unique per tenant)
   }
   ```

2. **TagLink** (M:N polymorphic)
   ```csharp
   public class TagLink {
       Guid TagId, OwnerType OwnerType, Guid OwnerId, string TenantId
   }
   ```
   - `OwnerType`: enum { Client, Contact, Activity, Opportunity, ... }

3. **Note**
   ```csharp
   public class Note : BaseAuditableEntity {
       Guid Id, string TenantId, OwnerType, Guid OwnerId,
       string Title (max 200), string Body (max text)
   }
   ```

4. **Attachment**
   ```csharp
   public class Attachment : BaseAuditableEntity {
       Guid Id, Guid NoteId, string FileName, long Size,
       string BlobKey (MinIO/Azure Blob), string ContentType
   }
   ```

#### Migrations:
- Remove `Tags` and `Notes` string columns from Client and Contact
- Add new tables: `Tags`, `TagLinks`, `Notes`, `Attachments`
- Data migration script: migrate existing inline tags/notes to new tables

#### APIs:
- `GET /api/crm/tags?search=` (autocomplete)
- `POST /api/crm/tags`
- `POST /api/crm/clients/{id}/tags` (bulk add/remove via TagLink)
- `POST /api/crm/contacts/{id}/tags`
- `GET /api/crm/clients/{id}/notes`
- `POST /api/crm/clients/{id}/notes`
- `PUT /api/crm/notes/{id}`
- `DELETE /api/crm/notes/{id}`
- `POST /api/crm/notes/{noteId}/attachments` (multipart upload)

#### Application Layer:
- Commands: `CreateTag`, `LinkTag`, `UnlinkTag`, `CreateNote`, `AddAttachment`
- Queries: `GetTagsQuery`, `GetNotesByOwnerQuery`
- Validators: FluentValidation for file size/type

#### Infrastructure:
- Blob storage service (MinIO or Azure Blob via configuration)
- Tag autocomplete with full-text search

**Deliverables:**
- ✅ Normalized Tag, Note, Attachment entities
- ✅ Migration from inline strings
- ✅ CRUD APIs for tags and notes
- ✅ File upload support

---

### 🎯 Phase 2: Address & Channel Normalization (Week 5)

**Goal:** Separate addresses and communication channels for reusability and multi-channel support

#### New Entities:
1. **Address**
   ```csharp
   public class Address : BaseAuditableEntity {
       Guid Id, string TenantId, OwnerType, Guid OwnerId,
       string Line1, string? Line2, string? District,
       string City, string State, string Zip, string Country,
       decimal? GeoLat, decimal? GeoLng,
       bool IsPrimary
   }
   ```

2. **ChannelIdentity**
   ```csharp
   public class ChannelIdentity : BaseAuditableEntity {
       Guid Id, string TenantId, OwnerType, Guid OwnerId,
       ChannelType Type, string Value (max 256),
       bool IsPrimary, DateTime? VerifiedAt
   }
   ```
   - `ChannelType`: enum { Email, Phone, Mobile, WhatsApp, Website, Instagram, LinkedIn, Other }

#### Migrations:
- Keep inline address fields on Client/Contact for backward compatibility (mark `[Obsolete]`)
- Add new `Addresses` and `ChannelIdentities` tables
- Data migration: copy existing addresses/emails/phones to new tables, set `IsPrimary = true`

#### APIs:
- `GET /api/crm/clients/{id}/addresses`
- `POST /api/crm/clients/{id}/addresses`
- `PUT /api/crm/addresses/{id}`
- `DELETE /api/crm/addresses/{id}`
- `GET /api/crm/clients/{id}/channels`
- `POST /api/crm/clients/{id}/channels`
- `PUT /api/crm/channels/{id}`

#### Features:
- Geocoding service (optional, via external API)
- Channel verification (email/phone OTP)

**Deliverables:**
- ✅ Address and ChannelIdentity entities
- ✅ Migration from inline fields
- ✅ CRUD APIs
- ✅ Primary flag support

---

### 🎯 Phase 3: Timeline - Activities & Interactions (Week 6-7)

**Goal:** Implement activity management and omni-channel interaction tracking

#### New Entities:
1. **Activity**
   ```csharp
   public class Activity : BaseAuditableEntity {
       Guid Id, string TenantId,
       ActivityType Type, ActivityStatus Status,
       DateTime Start, DateTime? Due,
       string Subject (max 160), string? Description,
       RegardingType? RegardingType, Guid? RegardingId,
       string? AssignedToUserId, DateTime? ReminderAt
   }
   ```
   - `ActivityType`: enum { Task, Call, Meeting, FollowUp }
   - `ActivityStatus`: enum { Open, Completed, Canceled }
   - `RegardingType`: enum { Client, Contact, Opportunity, Case, ... }

2. **Interaction** (Timeline)
   ```csharp
   public class Interaction : BaseAuditableEntity {
       Guid Id, string TenantId,
       InteractionType Type, string Direction,
       DateTime At, OwnerType OwnerType, Guid OwnerId,
       string? ChannelRef, string? Subject, string? Snippet,
       string? PayloadJson (jsonb)
   }
   ```
   - `InteractionType`: enum { Email, WhatsApp, Phone, Form, Import }
   - `Direction`: enum { Inbound, Outbound }

#### APIs:
- `GET /api/crm/clients/{id}/timeline` (unified: Interactions + Activities + Notes, sorted by date)
- `POST /api/crm/clients/{id}/activities`
- `PUT /api/crm/activities/{id}`
- `DELETE /api/crm/activities/{id}`
- `POST /api/crm/clients/{id}/interactions` (manual capture)
- `GET /api/crm/activities?status=Open&assignedTo={userId}`

#### Application Layer:
- Commands: `CreateActivity`, `CompleteActivity`, `CaptureInteraction`
- Queries: `GetTimelineQuery` (merge Activities/Interactions/Notes with pagination)
- Domain Event: `CRM.InteractionCaptured { OwnerType, OwnerId, Type, At }`

#### Features:
- Activity reminders (background job to send notifications)
- Timeline heatmap query (interactions per day/week)

**Deliverables:**
- ✅ Activity and Interaction entities
- ✅ Timeline API (unified feed)
- ✅ Activity CRUD
- ✅ Reminder system (basic)

---

### 🎯 Phase 4: LGPD Compliance - Consent & Data Privacy (Week 8)

**Goal:** Enable LGPD-compliant consent management, data export, and erasure workflows

#### New Entities:
1. **Consent**
   ```csharp
   public class Consent : BaseAuditableEntity {
       Guid Id, string TenantId,
       OwnerType OwnerType, Guid OwnerId,
       ConsentPurpose Purpose, bool OptIn,
       string Channel, DateTime At, string? Source
   }
   ```
   - `ConsentPurpose`: enum { Marketing, Contractual, Support, Legal }

#### APIs:
- `GET /api/crm/clients/{id}/consents`
- `POST /api/crm/clients/{id}/consents`
- `PUT /api/crm/consents/{id}`
- `GET /api/crm/exports/{ownerType}/{id}` (data export - JSON/CSV)
- `POST /api/crm/erasure-requests` (soft-delete + tombstone)

#### Application Layer:
- Commands: `RecordConsent`, `RequestDataExport`, `RequestErasure`
- Queries: `GetConsentsByOwnerQuery`, `ExportPersonalDataQuery`
- Domain Event: `CRM.ConsentChanged { OwnerType, OwnerId, Purpose, OptIn }`

#### Features:
- Data export: generate JSON/CSV with all personal data (Client, Contacts, Notes, Interactions)
- Erasure workflow: soft-delete + anonymize + publish `SubjectErasureRequested` event
- Consent audit trail (all changes logged)

**Deliverables:**
- ✅ Consent entity
- ✅ LGPD-compliant APIs
- ✅ Data export/erasure workflows
- ✅ Audit trail

---

### 🎯 Phase 5: Segmentation Engine (Week 9-10)

**Goal:** Dynamic customer segmentation with JSON-based rule engine

#### New Entities:
1. **Segment**
   ```csharp
   public class Segment : BaseAuditableEntity {
       Guid Id, string TenantId,
       string Name, string DefinitionJson (jsonb)
   }
   ```
   - JSON DSL example:
     ```json
     {
       "any": [
         {"field": "Industry", "op": "eq", "value": "Education"},
         {"field": "LastInteractionDays", "op": ">", "value": 30}
       ]
     }
     ```

2. **SegmentMembership** (materialized)
   ```csharp
   public class SegmentMembership {
       Guid SegmentId, string TenantId,
       OwnerType OwnerType, Guid OwnerId,
       DateTime ComputedAt
   }
   ```

#### APIs:
- `GET /api/crm/segments`
- `POST /api/crm/segments` (create definition)
- `PUT /api/crm/segments/{id}`
- `POST /api/crm/segments/{id}/rebuild` (trigger background job)
- `GET /api/crm/segments/{id}/members?page=&pageSize=`

#### Application Layer:
- Commands: `CreateSegment`, `UpdateSegment`, `RebuildSegment`
- Queries: `GetSegmentsQuery`, `GetSegmentMembersQuery`
- Background Job: `SegmentRebuildJob` (Hangfire/Quartz)

#### Features:
- JSON DSL parser (LINQKit or Expression Trees)
- Segment size tracking over time (for KPIs)
- Auto-rebuild on schedule (e.g., daily)

**Deliverables:**
- ✅ Segment entity
- ✅ JSON DSL engine
- ✅ Rebuild job
- ✅ Segment membership API

---

### 🎯 Phase 6: Relationships & Custom Fields (Week 11)

**Goal:** Model Account-to-Account relationships and extensible custom fields

#### New Entities:
1. **Relationship**
   ```csharp
   public class Relationship : BaseAuditableEntity {
       Guid Id, string TenantId,
       Guid FromId, Guid ToId,
       RelationshipType Type, string? Note
   }
   ```
   - `RelationshipType`: enum { ParentCompany, Subsidiary, Partner, Supplier, StudentOf, ProspectOf, ClientOf }

2. **CustomFieldSchema**
   ```csharp
   public class CustomFieldSchema : BaseAuditableEntity {
       Guid Id, string TenantId,
       string OwnerKind (e.g., "Client", "Contact"),
       string Name, string DataType, bool Required
   }
   ```

3. **CustomFieldValue**
   ```csharp
   public class CustomFieldValue {
       Guid Id, string TenantId,
       Guid OwnerId, string Name, string JsonValue
   }
   ```

#### APIs:
- `POST /api/crm/relationships` (create link)
- `GET /api/crm/clients/{id}/relationships`
- `DELETE /api/crm/relationships/{id}`
- `GET /api/crm/custom-fields?ownerKind=Client`
- `POST /api/crm/custom-fields` (define schema)
- `PUT /api/crm/clients/{id}/custom-fields` (set values)

#### Application Layer:
- Commands: `CreateRelationship`, `DefineCustomField`, `SetCustomFieldValue`
- Queries: `GetRelationshipsQuery`, `GetCustomFieldsQuery`

**Deliverables:**
- ✅ Relationship entity
- ✅ Custom fields schema/value
- ✅ CRUD APIs

---

### 🎯 Phase 7: AccountContact M:N (Week 12)

**Goal:** Support multiple Client-Contact associations with roles

#### New Entities:
1. **AccountContact** (many-to-many)
   ```csharp
   public class AccountContact {
       Guid ClientId, Guid ContactId, string TenantId,
       string Role (e.g., "Decision Maker", "Finance", "Legal Rep.", "Parent")
   }
   ```

#### Migrations:
- Keep existing `Contact.ClientId` for backward compatibility (mark as "Primary Association")
- Add `AccountContact` table with composite key `(ClientId, ContactId)`

#### APIs:
- `POST /api/crm/clients/{clientId}/contacts/{contactId}/roles` (link with role)
- `GET /api/crm/clients/{id}/contacts` (include roles)
- `DELETE /api/crm/clients/{clientId}/contacts/{contactId}`

**Deliverables:**
- ✅ AccountContact M:N entity
- ✅ Role-based associations
- ✅ Updated APIs

---

### 🎯 Phase 8: Import & Deduplication (Week 13-14)

**Goal:** CSV/XLSX import with deduplication and merge workflows

#### Features:
1. **Import Pipeline**
   - Upload CSV/XLSX → detect columns → preview → map fields → validate → import
   - Background job for large imports (Hangfire)

2. **Deduplication Rules**
   - Client: match by `(TaxId)` or `(LegalName ~ 85% similarity)`
   - Contact: match by `(Email)` or `(Mobile)` or `(FirstName+LastName+ClientId)`

3. **Merge Workflow**
   - UI to review duplicates → select survivor → merge (audit trail)

#### APIs:
- `POST /api/crm/imports/clients` (multipart upload)
- `GET /api/crm/imports/{id}/preview` (column mapping)
- `POST /api/crm/imports/{id}/execute` (start import)
- `GET /api/crm/duplicates?type=Client&threshold=0.85`
- `POST /api/crm/merge` (merge two records)

#### Application Layer:
- Commands: `ImportClientsCommand`, `MergeClientsCommand`
- Background Jobs: `ImportJob`, `DedupJob`

**Deliverables:**
- ✅ Import pipeline
- ✅ Dedup detection
- ✅ Merge workflow

---

### 🎯 Phase 9: Domain Events & Outbox Pattern (Week 15)

**Goal:** Enable event-driven architecture for cross-context integration

#### Infrastructure:
1. **Outbox Table**
   ```csharp
   public class OutboxMessage {
       Guid Id, string Type, string Payload,
       DateTime CreatedAt, DateTime? ProcessedAt
   }
   ```

2. **MassTransit Integration**
   - Configure MassTransit with InMemory/RabbitMQ/Azure Service Bus
   - Publish events after commit (Outbox pattern)

#### Events:
- `CRM.ClientCreated { ClientId, TenantId, Name }`
- `CRM.ContactCreated { ContactId, ClientId }`
- `CRM.InteractionCaptured { OwnerType, OwnerId, Type, At }`
- `CRM.ActivityDueSoon { ActivityId, Due }`
- `CRM.ConsentChanged { OwnerType, OwnerId, Purpose, OptIn }`

#### Consumers (Bounded Contexts):
- **Legal Context:** Subscribe to `ClientCreated` → pre-seed client pickers
- **Support Context:** Publish `TicketCreated` → CRM writes Interaction
- **Billing Context:** Publish `InvoiceIssued` → CRM writes Interaction + updates Segments

**Deliverables:**
- ✅ Outbox pattern
- ✅ MassTransit setup
- ✅ Event publishers/consumers
- ✅ Cross-context integration tests

---

### 🎯 Phase 10: Blazor UI - Client/Contact Grids (Week 16-17)

**Goal:** Build core CRM UI pages in Blazor WebAssembly

#### Pages:
1. **`/crm/clients`** (grid)
   - Columns: Name, Type, Stage, Industry, Owner, Tags, Last Activity
   - Filters: search, tag, status, stage
   - Quick actions: create, edit, delete

2. **`/crm/clients/new`** (create form)
   - Inline channels & address
   - Tag autocomplete

3. **`/crm/clients/{id}`** (detail view)
   - Tabs: Overview, Timeline, Contacts, Addresses/Channels, Consents, Files

4. **`/crm/contacts`** (grid)
   - Filters: search, clientId
   - Quick create dialog

5. **`/crm/contacts/{id}`** (detail view)
   - Similar to Client detail

#### Shared Components:
- `TimelineList.razor` (Interactions + Activities + Notes)
- `TagChips.razor` (editable tag chips)
- `ConsentPanel.razor` (LGPD consent display)
- `ChannelBadge.razor` (email/phone/WhatsApp icons)
- `ActivityEditor.razor` (create/edit activity)
- `AddressForm.razor` (address input with autocomplete)

#### State Management:
- Fluxor or MudBlazor-style service layer
- HttpClient + Refit for API calls
- Optimistic updates where safe

**Deliverables:**
- ✅ Client and Contact grids
- ✅ Detail pages with tabs
- ✅ Shared components
- ✅ PWA offline support (cache lists)

---

### 🎯 Phase 11: Timeline UI & Activity Management (Week 18)

**Goal:** Rich timeline and activity UIs

#### Pages:
1. **Timeline Tab** (in Client/Contact detail)
   - Unified feed: Interactions, Activities, Notes
   - Filter by type, date range
   - Infinite scroll

2. **Activity Management**
   - Calendar view (optional, using FullCalendar or similar)
   - Reminders (browser notifications + email)
   - Bulk actions (complete, reassign)

**Deliverables:**
- ✅ Timeline component
- ✅ Activity calendar
- ✅ Reminders

---

### 🎯 Phase 12: Segments & Reports UI (Week 19)

**Goal:** Segment builder and basic reporting

#### Pages:
1. **`/crm/segments`** (list + create)
   - Visual segment builder (drag-and-drop rules)
   - Preview member count

2. **`/crm/reports`** (dashboards)
   - Clients by Stage/Industry (chart)
   - New Contacts per Week (line chart)
   - Activities Open vs Completed (gauge)
   - Segment sizes over time (area chart)
   - Timeline heatmap

**Deliverables:**
- ✅ Segment builder UI
- ✅ KPI dashboards

---

### 🎯 Phase 13: Security & RBAC (Week 20)

**Goal:** Role-based access control for CRM

#### Roles:
- `CRM.Admin` - manage schemas, segments
- `CRM.Manager` - CRUD clients/contacts, assign owners
- `CRM.Agent` - read/write timeline, activities
- `CRM.ReadOnly` - view only

#### Implementation:
- Add `[Authorize(Policy = "CRM.Manager")]` to endpoints
- Policy-based authorization in ASP.NET Core
- Row-level security: filter by `OwnerUserId` for non-admins

**Deliverables:**
- ✅ RBAC policies
- ✅ Row-level security
- ✅ Authorization tests

---

### 🎯 Phase 14: Hardening & Ops (Week 21-22)

**Goal:** Production readiness

#### Features:
1. **OpenTelemetry**
   - Traces for all API calls
   - Structured logs with TenantId, UserId

2. **Rate Limiting**
   - Per IP/JWT (AspNetCore.RateLimiting)

3. **Backups**
   - DB backups (PITR for PostgreSQL)
   - Blob storage backups (MinIO/Azure)

4. **Feature Flags**
   - LaunchDarkly or custom (for Opportunities module later)

5. **Testing**
   - Integration tests for all APIs
   - Load testing (k6 or NBomber)

**Deliverables:**
- ✅ Observability
- ✅ Rate limiting
- ✅ Backup strategy
- ✅ Load tests

---

## Post-MVP Evolution

### Phase 15+: Advanced Features

1. **Opportunities/Pipeline** (Sales CRM)
   - Stages, weighted forecast, products/quotes
   - Sales analytics

2. **360° View** (Federated Queries)
   - Embed Support tickets, Legal cases, Billing invoices in CRM tabs

3. **Omni-channel** (Integrations)
   - WhatsApp Business API
   - Email threading (IMAP/SMTP)
   - LinkedIn/Instagram enrichment

4. **Data Quality**
   - Periodic dedup jobs
   - Bounce handling (email/phone validation)
   - CNPJ lookup (Receita Federal API)

5. **AI/ML**
   - Lead scoring
   - Next-best-action recommendations
   - Sentiment analysis on interactions

---

## Migration Strategy

### Backward Compatibility Plan

1. **Phase 0-2:** Keep inline fields (Address, Tags, Notes) with `[Obsolete]` attribute
2. **Data Migration:** Write scripts to copy inline data to new tables
3. **API Versioning:** Support both `/api/clients` (legacy) and `/api/crm/clients` (new) during transition
4. **Deprecation Timeline:** Remove inline fields after 6 months (Phase 14+)

### Database Migrations

- Use EF Core migrations for schema changes
- Data migration scripts in `Migrators.SQLite/DataMigrations/`
- Test migrations on staging before production
- Rollback plan: keep previous migration backup

---

## Testing Strategy

### Unit Tests
- Domain logic (entities, value objects)
- Application layer (commands, queries, validators)

### Integration Tests
- API endpoints (all CRUD + queries)
- Multi-tenancy isolation
- Event publishing/consuming

### E2E Tests
- Blazor UI workflows (Playwright or Selenium)
- Import/export flows
- Merge workflows

### Load Tests
- 1000 concurrent users
- 10K records in database
- Response time < 200ms (p95)

---

## Success Metrics (KPIs)

### Technical
- API response time: p95 < 200ms
- Test coverage: > 80%
- Zero cross-tenant data leaks
- Uptime: 99.9%

### Business
- Clients/Contacts imported: > 10K
- Segments created: > 50
- Timeline interactions captured: > 100K
- User adoption: > 80% of target users

---

## Dependencies & Prerequisites

### Required Skills
- .NET 10, C#, EF Core
- Blazor WebAssembly
- PostgreSQL / SQL Server
- MassTransit
- Docker (for Aspire)

### External Services
- Blob storage (MinIO or Azure Blob)
- Message broker (RabbitMQ or Azure Service Bus) - optional for Phase 9
- Geocoding API (Google Maps or OpenCage) - optional for Phase 2

### Infrastructure
- PostgreSQL 15+ (preferred) or SQL Server 2022+
- Redis (for caching and rate limiting)
- .NET Aspire AppHost for orchestration

---

## Risk Management

### Risks & Mitigations

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Data loss during migration | High | Low | Backup before each migration, test on staging |
| Performance degradation with large datasets | Medium | Medium | Add indexes, implement pagination, load tests |
| Multi-tenancy bugs (data leaks) | Critical | Low | Global query filters, integration tests, code reviews |
| MassTransit complexity | Medium | Medium | Start with InMemory, defer RabbitMQ to Phase 9 |
| UI/UX delays | Low | High | Use MudBlazor components, defer custom styling |

---

## Team & Resources

### Recommended Team (for 22-week timeline)
- 1x Backend Developer (.NET/C#)
- 1x Frontend Developer (Blazor)
- 1x DevOps Engineer (part-time)
- 1x QA Engineer (integration/E2E tests)
- 1x Product Owner (prioritization)

### Alternative (solo developer)
- Extend timeline to 6-9 months
- Focus on Phases 0-7 first (core CRM)
- Defer UI to Phase 10+ (use Scalar/Swagger for testing)

---

## Documentation Plan

### Living Documentation
1. **Architecture Decision Records (ADRs)** - document key decisions
2. **API Documentation** - Scalar + OpenAPI
3. **Developer Guide** - how to add new entities, endpoints
4. **User Guide** - Blazor UI workflows (screenshots)
5. **Runbooks** - deployment, backup, monitoring

---

## Conclusion

This roadmap transforms CleanAspire into a **production-ready, multi-tenant CRM** following the GenZ CRM specification. The **vertical slice approach** ensures we deliver value incrementally, with each phase producing working software.

**Next Steps:**
1. Review and approve this roadmap with stakeholders
2. Set up project board (GitHub Projects or Jira)
3. Create detailed user stories for Phase 0
4. Begin implementation: **Phase 0, Task 1 - Add TenantId to Client and Contact**

---

**Document Version:** 1.0
**Last Updated:** 2025-10-31
**Author:** Claude Code (with ZenAspire team)
**Status:** 🟢 Ready for Implementation
