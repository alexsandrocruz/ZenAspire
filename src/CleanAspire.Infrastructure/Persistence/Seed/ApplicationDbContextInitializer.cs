using CleanAspire.Domain.Entities;
using CleanAspire.Domain.Identities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanAspire.Infrastructure.Persistence.Seed;

public class ApplicationDbContextInitializer
{
    private readonly ILogger<ApplicationDbContextInitializer> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ApplicationDbContextInitializer(ILogger<ApplicationDbContextInitializer> logger, ApplicationDbContext context,
      UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }
    public async Task InitialiseAsync()
    {
        try
        {
            if (_context.Database.IsRelational())
                await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database");
            throw;
        }
    }
    public async Task SeedAsync()
    {
        try
        {
            await SeedUsersAsync();
            await SeedDataAsync();
            _context.ChangeTracker.Clear();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
    private async Task SeedUsersAsync()
    {
        if (!(await _context.Tenants.AnyAsync()))
        {
            var tenants = new List<Tenant>()
            {
                new()
                {
                    Name = "Host",
                    Description = "Default host tenant for system administration",
                    Id = "host" // Fixed ID for the host tenant
                },
                new()
                {
                    Name = "Org - 1",
                    Description = "Organization 1",
                    Id = Guid.CreateVersion7().ToString()
                },
                new()
                {
                    Name = "Org - 2",
                    Description = "Organization 2",
                    Id = Guid.CreateVersion7().ToString()
                }
            };
            _context.Tenants.AddRange(tenants);
            await _context.SaveChangesAsync();
        }

        if (await _userManager.Users.AnyAsync()) return;

        // Get the host tenant for the admin user
        var hostTenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == "host");
        if (hostTenant == null)
        {
            _logger.LogWarning("Host tenant not found. Skipping user seeding.");
            return;
        }

        var defaultPassword = "P@ssw0rd!";
        _logger.LogInformation("Seeding users...");
        var adminUser = new ApplicationUser
        {
            UserName = "Administrator",
            Provider = "Local",
            TenantId = hostTenant.Id, // Admin belongs to host tenant
            Nickname = "Administrator",
            Email = "admin@example.com",
            EmailConfirmed = true,
            LanguageCode = "en-US",
            TimeZoneId = "Asia/Shanghai",
            TwoFactorEnabled = false
        };

        var demoUser = new ApplicationUser
        {
            UserName = "Demo",
            Provider = "Local",
            TenantId = hostTenant.Id, // Demo user also belongs to host tenant
            Nickname = "Demo",
            Email = "Demo@example.com",
            EmailConfirmed = true,
            LanguageCode = "en-US",
            TimeZoneId = "Asia/Shanghai",
            TwoFactorEnabled = false,
            SuperiorId = adminUser.Id

        };

        await _userManager.CreateAsync(adminUser, defaultPassword);
        await _userManager.CreateAsync(demoUser, defaultPassword);
    }
    private async Task SeedDataAsync()
    {
        if (await _context.Products.AnyAsync()) return;
        _logger.LogInformation("Seeding data...");

        // Seed Clients
        await SeedClientsAsync();

        var products = new List<Product>
        {
            new Product
            {
                Name = "Ikea LACK Coffee Table",
                Description = "Simple and stylish coffee table from Ikea, featuring a modern design and durable surface. Perfect for living rooms or offices.",
                Price = 25,
                SKU = "LACK-COFFEE-TABLE",
                UOM = "PCS",
                Currency = "USD",
                Category = ProductCategory.Furniture
            },
            new Product
            {
                Name = "Nike Air Zoom Pegasus 40",
                Description = "Lightweight and responsive running shoes with advanced cushioning and a breathable mesh upper. Ideal for athletes and daily runners.",
                Price = 130,
                SKU = "NIKE-PEGASUS-40",
                UOM = "PCS",
                Currency = "USD",
                Category = ProductCategory.Sports
            },
            new Product
            {
                Name = "Adidas Yoga Mat",
                Description = "Non-slip yoga mat with a 6mm thickness for optimal cushioning and support during workouts. Suitable for yoga, pilates, or general exercises.",
                Price = 45,
                SKU = "ADIDAS-YOGA-MAT",
                UOM = "PCS",
                Currency = "USD",
                Category = ProductCategory.Sports
            },
            new Product
            {
                Name = "Ikea HEMNES Bed Frame",
                Description = "Solid wood bed frame with a classic design. Offers excellent durability and comfort. Compatible with standard-size mattresses.",
                Price = 199,
                SKU = "HEMNES-BED-FRAME",
                UOM = "PCS",
                Currency = "USD",
                Category = ProductCategory.Furniture
            },
            new Product
            {
                Name = "Under Armour Men's HeatGear Compression Shirt",
                Description = "High-performance compression shirt designed to keep you cool and dry during intense workouts. Made from moisture-wicking fabric.",
                Price = 35,
                SKU = "UA-HEATGEAR-SHIRT",
                UOM = "PCS",
                Currency = "USD",
                Category = ProductCategory.Sports
            },
            new Product
            {
                Name = "Apple iPhone 15 Pro",
                Description = "Apple's latest flagship smartphone featuring a 6.1-inch Super Retina XDR display, A17 Pro chip, titanium frame, and advanced camera system with 5x telephoto lens. Ideal for tech enthusiasts and professional users.",
                Price = 1199,
                SKU = "IP15PRO",
                UOM = "PCS",
                Currency = "USD",
                Category = ProductCategory.Electronics
            }
        };

        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();
        var stocks = new List<Stock>
        {
            new Stock
            {
                ProductId = products.FirstOrDefault(p => p.Name == "Ikea LACK Coffee Table")?.Id,
                Product = products.FirstOrDefault(p => p.Name == "Ikea LACK Coffee Table"),
                Quantity = 50,
                Location = "FU-WH-0001"
            },
            new Stock
            {
                ProductId = products.FirstOrDefault(p => p.Name == "Nike Air Zoom Pegasus 40")?.Id,
                Product = products.FirstOrDefault(p => p.Name == "Nike Air Zoom Pegasus 40"),
                Quantity = 100,
                Location = "SP-WH-0001"
            },
            new Stock
            {
                ProductId = products.FirstOrDefault(p => p.Name == "Apple iPhone 15 Pro")?.Id,
                Product = products.FirstOrDefault(p => p.Name == "Apple iPhone 15 Pro"),
                Quantity = 200,
                Location = "EL-WH-0001"
            }
        };

        await _context.Stocks.AddRangeAsync(stocks);
        await _context.SaveChangesAsync();
    }

    private async Task SeedClientsAsync()
    {
        if (await _context.Clients.AnyAsync()) return;

        _logger.LogInformation("Seeding clients and contacts...");

        // Get the host tenant to associate with clients
        var hostTenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == "host");
        if (hostTenant == null)
        {
            _logger.LogWarning("Host tenant not found. Skipping client seeding.");
            return;
        }

        var clients = new List<Client>
        {
            new Client
            {
                TenantId = hostTenant.Id,
                Name = "Acme Corporation",
                Type = ClientType.Company,
                Status = ClientStatus.Active,
                Industry = "Technology",
                Email = "contact@acme.com",
                Phone = "+1-555-0100",
                Website = "https://acme.com",
                Notes = "Leading technology solutions provider",
                Priority = ClientPriority.High,
                LifecycleStage = "Client"
            },
            new Client
            {
                TenantId = hostTenant.Id,
                Name = "TechStart Innovations",
                Type = ClientType.Company,
                Status = ClientStatus.Active,
                Industry = "Software Development",
                Email = "info@techstart.com",
                Phone = "+1-555-0200",
                Website = "https://techstart.com",
                Notes = "Innovative software development company",
                Priority = ClientPriority.Medium,
                LifecycleStage = "Client"
            },
            new Client
            {
                TenantId = hostTenant.Id,
                Name = "Global Retail Inc",
                Type = ClientType.Company,
                Status = ClientStatus.Active,
                Industry = "Retail",
                Email = "sales@globalretail.com",
                Phone = "+1-555-0300",
                Website = "https://globalretail.com",
                Notes = "International retail chain",
                Priority = ClientPriority.High,
                LifecycleStage = "Client"
            }
        };

        await _context.Clients.AddRangeAsync(clients);
        await _context.SaveChangesAsync();

        // Seed Contacts for each Client
        var contacts = new List<Contact>
        {
            new Contact
            {
                TenantId = hostTenant.Id,
                FirstName = "John",
                LastName = "Smith",
                Email = "john.smith@acme.com",
                Phone = "+1-555-0101",
                JobTitle = "CEO",
                Department = "Executive",
                ClientId = clients[0].Id,
                Status = ContactStatus.Active,
                IsMainContact = true,
                IsDecisionMaker = true
            },
            new Contact
            {
                TenantId = hostTenant.Id,
                FirstName = "Sarah",
                LastName = "Johnson",
                Email = "sarah.johnson@acme.com",
                Phone = "+1-555-0102",
                JobTitle = "CTO",
                Department = "Technology",
                ClientId = clients[0].Id,
                Status = ContactStatus.Active,
                IsDecisionMaker = true
            },
            new Contact
            {
                TenantId = hostTenant.Id,
                FirstName = "Michael",
                LastName = "Chen",
                Email = "michael.chen@techstart.com",
                Phone = "+1-555-0201",
                JobTitle = "Founder & CEO",
                Department = "Executive",
                ClientId = clients[1].Id,
                Status = ContactStatus.Active,
                IsMainContact = true,
                IsDecisionMaker = true
            },
            new Contact
            {
                TenantId = hostTenant.Id,
                FirstName = "Emily",
                LastName = "Davis",
                Email = "emily.davis@globalretail.com",
                Phone = "+1-555-0301",
                JobTitle = "VP of Operations",
                Department = "Operations",
                ClientId = clients[2].Id,
                Status = ContactStatus.Active,
                IsMainContact = true,
                IsDecisionMaker = true
            }
        };

        await _context.Contacts.AddRangeAsync(contacts);
        await _context.SaveChangesAsync();
    }
}

