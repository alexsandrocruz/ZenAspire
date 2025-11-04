// This static class defines the structure and content of the navigation bar menu for the application.
// It organizes menu items into a hierarchical structure with labels, icons, URLs, statuses, and descriptions.

// Purpose:
// 1. **Navigation Menu Structure**:
//    - Provides a clear and organized layout of menu items for easy access to different sections of the application.
//    - Supports submenus for grouping related functionality (e.g., Products, Orders, Reports, Help).

// 2. **User Experience**:
//    - Enhances the user experience by displaying icons (`StartIcon`, `EndIcon`) and descriptions for each menu item.
//    - Displays the status of each menu item (e.g., `Completed`, `New`, `ComingSoon`), helping users identify available features.

using MudBlazor;

namespace CleanAspire.ClientApp.Services.Navigation;

/// <summary>
/// Represents the default navigation menu configuration for the application.
/// Includes sections like Application, Reports, and Help with nested submenus.
/// </summary>
public static class NavbarMenu
{
    public static List<MenuItem> Default = new List<MenuItem>
    {
        new MenuItem
        {
            Label = "GenZen Aspire",
            StartIcon = Icons.Material.Filled.Dashboard,
            EndIcon = Icons.Material.Filled.KeyboardArrowDown,
            SubItems = new List<MenuItem>
            {
                new MenuItem
                {
                    Label = "Modulos",
                    Href = "/genz/modules",
                    Status = PageStatus.Completed,
                    Description = "View an overview of all modules."
                },
                new MenuItem
                {
                    Label = "Entidades ",
                    Href = "/genz/entities",
                    Status = PageStatus.New,
                    Description = "Ver todas as entidades disponiveis em nosso sistema."
                },
                new MenuItem
                {
                    Label = "Schema Builder",
                    Href = "/genz/schema-builder",
                    Status = PageStatus.Completed,
                    Description = "View the schema builder for database entities."
                }
            }
        },
        new MenuItem
        {
            Label = "Application",
            StartIcon = Icons.Material.Filled.AppRegistration,
            EndIcon = Icons.Material.Filled.KeyboardArrowDown,
            SubItems = new List<MenuItem>
            {
                new MenuItem
                {
                    Label = "Products",
                    SubItems = new List<MenuItem>
                    {
                        new MenuItem
                        {
                            Label = "All Products",
                            Href = "/products/index",
                            Status = PageStatus.Completed,
                            Description = "View all available products in our inventory."
                        },
                        new MenuItem
                        {
                            Label = "Stock Inquiry",
                            Href = "/stocks/index",
                            Status = PageStatus.New,
                            Description = "Check product stock levels."
                        },
                        new MenuItem
                        {
                            Label = "Best Sellers",
                            Href = "",
                            Status = PageStatus.Completed,
                            Description = "See our top-selling products."
                        }
                    }
                },
                new MenuItem
                {
                    Label = "CRM",
                    SubItems = new List<MenuItem>
                    {
                        new MenuItem
                        {
                            Label = "Clients",
                            Href = "/clients/index",
                            Status = PageStatus.New,
                            Description = "Manage clients and their information."
                        },
                        new MenuItem
                        {
                            Label = "Contacts",
                            Href = "/contacts/index",
                            Status = PageStatus.New,
                            Description = "Manage contacts and their relationships with clients."
                        },
                        new MenuItem
                        {
                            Label = "Activities",
                            Href = "/activities/index",
                            Status = PageStatus.New,
                            Description = "Manage activities, tasks, meetings, and follow-ups."
                        },
                        new MenuItem
                        {
                            Label = "Notes",
                            Href = "/notes/index",
                            Status = PageStatus.New,
                            Description = "View and manage notes across all entities."
                        },
                        new MenuItem
                        {
                            Label = "Timeline",
                            Href = "/timeline",
                            Status = PageStatus.New,
                            Description = "Unified timeline view of all activities, interactions, and notes."
                        }
                    }
                },
                new MenuItem
                {
                    Label = "Orders",
                    SubItems = new List<MenuItem>
                    {
                        new MenuItem
                        {
                            Label = "Order Overview",
                            Href = "/orders/overview",
                            Status = PageStatus.ComingSoon,
                            Description = "Overview of all orders."
                        },
                        new MenuItem
                        {
                            Label = "Shipment Details",
                            Href = "/orders/shipments",
                            Status = PageStatus.ComingSoon,
                            Description = "Track the shipment details of orders."
                        }
                    }
                }
            }
        },
        new MenuItem
        {
            Label = "Reports",
            StartIcon = Icons.Material.Filled.Dashboard,
            EndIcon = Icons.Material.Filled.KeyboardArrowDown,
            SubItems = new List<MenuItem>
            {
                new MenuItem
                {
                    Label = "Overview",
                    Href = "/reports/overview",
                    Status = PageStatus.Completed,
                    Description = "View an overview of all reports."
                },
                new MenuItem
                {
                    Label = "Statistics",
                    Href = "/reports/statistics",
                    Status = PageStatus.New,
                    Description = "Analyze detailed statistics for performance tracking."
                },
                new MenuItem
                {
                    Label = "Activity Log",
                    Href = "/reports/activitylog",
                    Status = PageStatus.Completed,
                    Description = "View the activity log for user actions."
                }
            }
        },
        new MenuItem
        {
            Label = "LGPD & Privacy",
            StartIcon = Icons.Material.Filled.Security,
            EndIcon = Icons.Material.Filled.KeyboardArrowDown,
            SubItems = new List<MenuItem>
            {
                new MenuItem
                {
                    Label = "Dashboard",
                    Href = "/lgpd",
                    Status = PageStatus.New,
                    Description = "Overview of all consents and privacy settings."
                },
                new MenuItem
                {
                    Label = "Consent Management",
                    SubItems = new List<MenuItem>
                    {
                        new MenuItem
                        {
                            Label = "All Consents",
                            Href = "/lgpd/consents",
                            Status = PageStatus.New,
                            Description = "View and manage user consents."
                        },
                        new MenuItem
                        {
                            Label = "Record Consent",
                            Href = "/lgpd/consents/new",
                            Status = PageStatus.New,
                            Description = "Record a new consent for data processing."
                        }
                    }
                },
                new MenuItem
                {
                    Label = "Data Rights",
                    SubItems = new List<MenuItem>
                    {
                        new MenuItem
                        {
                            Label = "Export My Data",
                            Href = "/lgpd/export",
                            Status = PageStatus.New,
                            Description = "Download your personal data in JSON or CSV format."
                        },
                        new MenuItem
                        {
                            Label = "Request Deletion",
                            Href = "/lgpd/erasure",
                            Status = PageStatus.ComingSoon,
                            Description = "Request deletion of your personal data."
                        }
                    }
                }
            }
        },
        new MenuItem
        {
            Label = "Help",
            StartIcon = Icons.Material.Filled.Help,
            EndIcon = Icons.Material.Filled.KeyboardArrowDown,
            SubItems = new List<MenuItem>
            {
                new MenuItem
                {
                    Label = "Documentation",
                    Href = "/help/documentation",
                    Status = PageStatus.Completed,
                    Description = "Access the user and developer documentation."
                },
                new MenuItem
                {
                    Label = "GitHub",
                    Href = "https://github.com/neozhu/cleanaspire/",
                    Status = PageStatus.Completed,
                    Description = "Visit our GitHub repository."
                }
            }
        }
    };
}
