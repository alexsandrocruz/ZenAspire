using System.ComponentModel.DataAnnotations;
using CleanAspire.Domain.Common;

namespace CleanAspire.Domain.Entities
{
    public class Contact : BaseAuditableEntity, IAuditTrial
    {
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
        
        [MaxLength(20)]
        public string? MobilePhone { get; set; }
        
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