using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanAspire.Migrators.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class AddConsentEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ReminderSent",
                table: "Activities",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Consents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    OwnerType = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Purpose = table.Column<int>(type: "INTEGER", nullable: false),
                    OptIn = table.Column<bool>(type: "INTEGER", nullable: false),
                    Channel = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    At = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    IpAddress = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ClientId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    ContactId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consents_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Consents_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Consents_At",
                table: "Consents",
                column: "At");

            migrationBuilder.CreateIndex(
                name: "IX_Consents_ClientId",
                table: "Consents",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Consents_ContactId",
                table: "Consents",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Consents_TenantId",
                table: "Consents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Consents_TenantId_OptIn",
                table: "Consents",
                columns: new[] { "TenantId", "OptIn" });

            migrationBuilder.CreateIndex(
                name: "IX_Consents_TenantId_OwnerType_OwnerId",
                table: "Consents",
                columns: new[] { "TenantId", "OwnerType", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_Consents_TenantId_OwnerType_OwnerId_Purpose",
                table: "Consents",
                columns: new[] { "TenantId", "OwnerType", "OwnerId", "Purpose" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Consents_TenantId_Purpose",
                table: "Consents",
                columns: new[] { "TenantId", "Purpose" });

            migrationBuilder.CreateIndex(
                name: "IX_Consents_TenantId_ValidUntil",
                table: "Consents",
                columns: new[] { "TenantId", "ValidUntil" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Consents");

            migrationBuilder.DropColumn(
                name: "ReminderSent",
                table: "Activities");
        }
    }
}
