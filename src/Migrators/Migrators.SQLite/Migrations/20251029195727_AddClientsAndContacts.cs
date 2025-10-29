using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanAspire.Migrators.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class AddClientsAndContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TradeName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DocumentNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Website = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    PostalCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Country = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Industry = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Size = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    AnnualRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EmployeeCount = table.Column<int>(type: "INTEGER", nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    FirstContactDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastInteractionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    MobilePhone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    JobTitle = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Department = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    PostalCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    IsMainContact = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDecisionMaker = table.Column<bool>(type: "INTEGER", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastContactDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ClientId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contacts_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_DocumentNumber",
                table: "Clients",
                column: "DocumentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Email",
                table: "Clients",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Name",
                table: "Clients",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Status",
                table: "Clients",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Type",
                table: "Clients",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_ClientId",
                table: "Contacts",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_Email",
                table: "Contacts",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_FirstName_LastName",
                table: "Contacts",
                columns: new[] { "FirstName", "LastName" });

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_IsDecisionMaker",
                table: "Contacts",
                column: "IsDecisionMaker");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_IsMainContact",
                table: "Contacts",
                column: "IsMainContact");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_Status",
                table: "Contacts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_Type",
                table: "Contacts",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                column: "Email",
                unique: true);
        }
    }
}
