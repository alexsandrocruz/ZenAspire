using CleanAspire.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanAspire.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the Contact entity.
/// </summary>
public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        // Configurações da tabela
        builder.ToTable("Contacts");
        
        // Chave primária
        builder.HasKey(x => x.Id);
        
        // Propriedades obrigatórias
        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(150);
            
        builder.Property(x => x.ClientId)
            .IsRequired();
            
        // Propriedades opcionais
        builder.Property(x => x.Phone)
            .HasMaxLength(20);
            
        builder.Property(x => x.MobilePhone)
            .HasMaxLength(20);
            
        builder.Property(x => x.JobTitle)
            .HasMaxLength(100);
            
        builder.Property(x => x.Department)
            .HasMaxLength(100);
            
        // Endereço
        builder.Property(x => x.Address)
            .HasMaxLength(200);
            
        builder.Property(x => x.City)
            .HasMaxLength(100);
            
        builder.Property(x => x.State)
            .HasMaxLength(50);
            
        builder.Property(x => x.PostalCode)
            .HasMaxLength(20);
            
        builder.Property(x => x.Notes)
            .HasMaxLength(1000);
            
        builder.Property(x => x.Tags)
            .HasMaxLength(500);
        
        // Enums
        builder.Property(x => x.Type)
            .HasConversion<int>();
            
        builder.Property(x => x.Status)
            .HasConversion<int>();
        
        // Relacionamentos
        builder.HasOne(x => x.Client)
            .WithMany(x => x.Contacts)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Índices
        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => x.ClientId);
        builder.HasIndex(x => new { x.FirstName, x.LastName });
        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.IsMainContact);
        builder.HasIndex(x => x.IsDecisionMaker);
        
        // Propriedades ignoradas (calculadas)
        builder.Ignore(x => x.FullName);
        builder.Ignore(x => x.CompanyName);
        
        // Ignorar eventos de domínio
        builder.Ignore(e => e.DomainEvents);
    }
}