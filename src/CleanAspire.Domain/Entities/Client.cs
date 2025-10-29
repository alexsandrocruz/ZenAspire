using System.ComponentModel.DataAnnotations;
using CleanAspire.Domain.Common;

namespace CleanAspire.Domain.Entities;

public class Client : BaseAuditableEntity, IAuditTrial
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? TradeName { get; set; }
    
    [MaxLength(20)]
    public string? DocumentNumber { get; set; } // CNPJ, CPF, ou documento equivalente
    
    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }
    
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    [MaxLength(250)]
    public string? Website { get; set; }
    
    // Endereço
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
    public string? Industry { get; set; } // Setor/Ramo de atividade
    
    [MaxLength(50)]
    public string? Size { get; set; } // Pequena, Média, Grande empresa
    
    public decimal? AnnualRevenue { get; set; }
    
    public int? EmployeeCount { get; set; }
    
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
    NonProfit = 4       // Organização sem fins lucrativos
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