using System.ComponentModel.DataAnnotations;
using CleanAspire.Domain.Common;

namespace CleanAspire.Domain.Entities;

public class Client : BaseAuditableEntity, IAuditTrial
{
    // ✅ Multi-tenancy support
    [Required]
    [MaxLength(450)]
    public string TenantId { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    // ✅ CRM spec alignment
    [MaxLength(200)]
    public string? LegalName { get; set; }

    [MaxLength(50)]
    public string? TradeName { get; set; }

    // ✅ TaxId (normalized: uppercase, no special chars)
    [MaxLength(32)]
    public string? TaxId { get; set; } // CNPJ, CPF (normalized)

    // ✅ Backward compatibility alias
    [Obsolete("Use TaxId instead")]
    [MaxLength(20)]
    public string? DocumentNumber
    {
        get => TaxId;
        set => TaxId = value;
    }
    
    // ✅ Phase 2: Inline channels marked as obsolete - use ChannelIdentity entity instead
    [Obsolete("Use ChannelIdentity entity instead. Will be removed in Phase 2+ after data migration.")]
    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }

    [Obsolete("Use ChannelIdentity entity instead. Will be removed in Phase 2+ after data migration.")]
    [MaxLength(20)]
    public string? Phone { get; set; }

    [Obsolete("Use ChannelIdentity entity instead. Will be removed in Phase 2+ after data migration.")]
    [MaxLength(250)]
    public string? Website { get; set; }

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

    [Obsolete("Use Address entity instead. Will be removed in Phase 2+ after data migration.")]
    [MaxLength(100)]
    public string? Country { get; set; }
    
    // Informações de negócio
    [MaxLength(100)]
    public string? Industry { get; set; } // Setor/Ramo de atividade
    
    [MaxLength(50)]
    public string? Size { get; set; } // Pequena, Média, Grande empresa
    
    public decimal? AnnualRevenue { get; set; }
    
    public int? EmployeeCount { get; set; }
    
    // ✅ CRM lifecycle management
    [Required]
    [MaxLength(32)]
    public string LifecycleStage { get; set; } = "Lead"; // Lead, Prospect, Client, Former

    // ✅ Owner assignment
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
    
    // Data de primeiro contato
    public DateTime? FirstContactDate { get; set; }
    
    // Data de última interação
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
    Company = 1,        // Empresa/Organização
    PublicFigure = 2,   // Pessoa pública (palestrante, político, artista, etc.)
    Government = 3,     // Órgão governamental
    NonProfit = 4,      // Organização sem fins lucrativos
    School = 5          // ✅ CRM spec: Escola/Instituição de ensino
}

public enum ClientStatus
{
    Prospect = 1,       // Potencial cliente
    Active = 2,         // Cliente ativo
    Inactive = 3,       // Cliente inativo
    Churned = 4,        // Cliente perdido
    Blocked = 5         // Cliente bloqueado
}

public enum ClientPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    VIP = 4
}