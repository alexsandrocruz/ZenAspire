using System.ComponentModel.DataAnnotations;
using CleanAspire.Domain.Common;

namespace CleanAspire.Domain.Entities
{
    public class Contact : BaseAuditableEntity, IAuditTrial
    {
        // ✅ Multi-tenancy support
        [Required]
        [MaxLength(450)]
        public string TenantId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        // ✅ Phase 2: Inline channels marked as obsolete - use ChannelIdentity entity instead
        [Obsolete("Use ChannelIdentity entity instead. Will be removed in Phase 2+ after data migration.")]
        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Obsolete("Use ChannelIdentity entity instead. Will be removed in Phase 2+ after data migration.")]
        [MaxLength(20)]
        public string? Phone { get; set; }

        // ✅ Mobile (E.164 format: +55 11 99999-9999)
        [Obsolete("Use ChannelIdentity entity instead. Will be removed in Phase 2+ after data migration.")]
        [MaxLength(40)]
        public string? Mobile { get; set; }

        // ✅ Backward compatibility alias
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

        // ✅ Phase 2: Inline address fields marked as obsolete - use Address entity instead
        [Obsolete("Use Address entity instead. Will be removed in Phase 2+ after data migration.")]
        [MaxLength(200)]
        public string? Address { get; set; }

        [Obsolete("Use Address entity instead. Will be removed in Phase 2+ after data migration.")]
        [MaxLength(100)]
        public string? City { get; set; }

        [Obsolete("Use Address entity instead. Will be removed in Phase 2+ after data migration.")]
        [MaxLength(50)]
        public string? State { get; set; }

        [Obsolete("Use Address entity instead. Will be removed in Phase 2+ after data migration.")]
        [MaxLength(20)]
        public string? PostalCode { get; set; }
        
        [MaxLength(1000)]
        public string? Notes { get; set; }
        
        [MaxLength(500)]
        public string? Tags { get; set; }
        
        // ✅ CRM lifecycle management
        [Required]
        [MaxLength(32)]
        public string LifecycleStage { get; set; } = "Lead"; // Lead, Prospect, Client, Former

        // ✅ Owner assignment
        [MaxLength(450)]
        public string? OwnerUserId { get; set; }

        // Informações de relacionamento
        public ContactType Type { get; set; }
        public ContactStatus Status { get; set; }

        public bool IsMainContact { get; set; } = false; // Contato principal do cliente
        public bool IsDecisionMaker { get; set; } = false; // Tomador de decisão
        
        // Datas importantes
        public DateTime? BirthDate { get; set; }
        public DateTime? LastContactDate { get; set; }
        
        // Relacionamentos
        [Required]
        public string ClientId { get; set; } = string.Empty;
        public virtual Client Client { get; set; } = null!;

        // ✅ For future M:N support (Phase 7 - AccountContact)
        public string? AccountId { get; set; }

        // Propriedades calculadas
        public string FullName => $"{FirstName} {LastName}".Trim();

        public string CompanyName => Client?.DisplayName ?? string.Empty;
    }

    public enum ContactType
    {
        Lead = 1,
        Prospect = 2,
        Client = 3,        // Mudado de Customer para Client
        Partner = 4,
        Vendor = 5,        // Fornecedor
        Employee = 6       // Funcionário
    }

    public enum ContactStatus
    {
        Active = 1,
        Inactive = 2,
        Qualified = 3,
        Unqualified = 4
    }
}