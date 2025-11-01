using CleanAspire.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanAspire.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for the Client entity.
/// </summary>
public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        // Configurações da tabela
        builder.ToTable("Clients");

        // Chave primária
        builder.HasKey(x => x.Id);

        // ✅ Multi-tenancy: TenantId required
        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(450);

        // ✅ Composite unique index per tenant: (TenantId, TaxId)
        builder.HasIndex(x => new { x.TenantId, x.TaxId })
            .IsUnique()
            .HasFilter($"\"{nameof(Client.TaxId)}\" IS NOT NULL");

        // ✅ Index for tenant queries (performance)
        builder.HasIndex(x => x.TenantId);

        // ✅ Index for lookup queries
        builder.HasIndex(x => new { x.TenantId, x.Name });
        builder.HasIndex(x => new { x.TenantId, x.Email });

        // Propriedades obrigatórias
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.LifecycleStage)
            .IsRequired()
            .HasMaxLength(32)
            .HasDefaultValue("Lead");

        // ✅ New CRM fields
        builder.Property(x => x.LegalName)
            .HasMaxLength(200);

        builder.Property(x => x.TaxId)
            .HasMaxLength(32);

        builder.Property(x => x.OwnerUserId)
            .HasMaxLength(450);

        // Propriedades opcionais
        builder.Property(x => x.TradeName)
            .HasMaxLength(50);
            
        builder.Property(x => x.Email)
            .HasMaxLength(100);
            
        builder.Property(x => x.Phone)
            .HasMaxLength(20);
            
        builder.Property(x => x.Website)
            .HasMaxLength(250);
            
        // Endereço
        builder.Property(x => x.Address)
            .HasMaxLength(200);
            
        builder.Property(x => x.City)
            .HasMaxLength(100);
            
        builder.Property(x => x.State)
            .HasMaxLength(50);
            
        builder.Property(x => x.PostalCode)
            .HasMaxLength(20);
            
        builder.Property(x => x.Country)
            .HasMaxLength(100);
            
        // Informações de negócio
        builder.Property(x => x.Industry)
            .HasMaxLength(100);
            
        builder.Property(x => x.Size)
            .HasMaxLength(50);
            
        builder.Property(x => x.AnnualRevenue)
            .HasColumnType("decimal(18,2)");
            
        builder.Property(x => x.Notes)
            .HasMaxLength(1000);
            
        builder.Property(x => x.Tags)
            .HasMaxLength(500);
        
        // Enums
        builder.Property(x => x.Type)
            .HasConversion<int>();
            
        builder.Property(x => x.Status)
            .HasConversion<int>();
            
        builder.Property(x => x.Priority)
            .HasConversion<int>();
        
        // Relacionamentos
        builder.HasMany(x => x.Contacts)
            .WithOne(x => x.Client)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Índices adicionais
        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.LifecycleStage);

        // Propriedades ignoradas (calculadas)
        builder.Ignore(x => x.DisplayName);
        builder.Ignore(x => x.IsCompany);
        builder.Ignore(x => x.IsPublicFigure);
        builder.Ignore(x => x.DocumentNumber); // ✅ Obsolete alias
        
        // Ignorar eventos de domínio
        builder.Ignore(e => e.DomainEvents);
    }
}