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

        // ✅ Multi-tenancy: TenantId required
        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(450);

        // ✅ Composite unique index per tenant: (TenantId, Email)
        builder.HasIndex(x => new { x.TenantId, x.Email })
            .IsUnique();

        // ✅ Index for tenant queries (performance)
        builder.HasIndex(x => x.TenantId);

        // ✅ Index for lookup queries
        builder.HasIndex(x => new { x.TenantId, x.FirstName, x.LastName });
        builder.HasIndex(x => new { x.TenantId, x.ClientId });

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

        builder.Property(x => x.LifecycleStage)
            .IsRequired()
            .HasMaxLength(32)
            .HasDefaultValue("Lead");

        // ✅ New CRM fields
        builder.Property(x => x.Mobile)
            .HasMaxLength(40);

        builder.Property(x => x.OwnerUserId)
            .HasMaxLength(450);

        // Propriedades opcionais
        builder.Property(x => x.Phone)
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
        
        // Índices adicionais
        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.IsMainContact);
        builder.HasIndex(x => x.IsDecisionMaker);
        builder.HasIndex(x => x.LifecycleStage);

        // Propriedades ignoradas (calculadas)
        builder.Ignore(x => x.FullName);
        builder.Ignore(x => x.CompanyName);
        builder.Ignore(x => x.MobilePhone); // ✅ Obsolete alias
        
        // Ignorar eventos de domínio
        builder.Ignore(e => e.DomainEvents);
    }
}