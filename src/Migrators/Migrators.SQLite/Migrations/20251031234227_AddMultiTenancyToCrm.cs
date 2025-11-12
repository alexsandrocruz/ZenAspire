using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanAspire.Migrators.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenancyToCrm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Contacts_Email",
                table: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_FirstName_LastName",
                table: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Clients_DocumentNumber",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_Email",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_Name",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "MobilePhone",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "DocumentNumber",
                table: "Clients");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountId",
                table: "Contacts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LifecycleStage",
                table: "Contacts",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "Lead");

            migrationBuilder.AddColumn<string>(
                name: "Mobile",
                table: "Contacts",
                type: "TEXT",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "Contacts",
                type: "TEXT",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Contacts",
                type: "TEXT",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LegalName",
                table: "Clients",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LifecycleStage",
                table: "Clients",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "Lead");

            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "Clients",
                type: "TEXT",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxId",
                table: "Clients",
                type: "TEXT",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Clients",
                type: "TEXT",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_LifecycleStage",
                table: "Contacts",
                column: "LifecycleStage");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_TenantId",
                table: "Contacts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_TenantId_ClientId",
                table: "Contacts",
                columns: new[] { "TenantId", "ClientId" });

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_TenantId_Email",
                table: "Contacts",
                columns: new[] { "TenantId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_TenantId_FirstName_LastName",
                table: "Contacts",
                columns: new[] { "TenantId", "FirstName", "LastName" });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_LifecycleStage",
                table: "Clients",
                column: "LifecycleStage");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_TenantId",
                table: "Clients",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_TenantId_Email",
                table: "Clients",
                columns: new[] { "TenantId", "Email" });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_TenantId_Name",
                table: "Clients",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_TenantId_TaxId",
                table: "Clients",
                columns: new[] { "TenantId", "TaxId" },
                unique: true,
                filter: "\"TaxId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Contacts_LifecycleStage",
                table: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_TenantId",
                table: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_TenantId_ClientId",
                table: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_TenantId_Email",
                table: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_TenantId_FirstName_LastName",
                table: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Clients_LifecycleStage",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_TenantId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_TenantId_Email",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_TenantId_Name",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_TenantId_TaxId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "LifecycleStage",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "Mobile",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "LegalName",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "LifecycleStage",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "TaxId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Clients");

            migrationBuilder.AddColumn<string>(
                name: "MobilePhone",
                table: "Contacts",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentNumber",
                table: "Clients",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_Email",
                table: "Contacts",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_FirstName_LastName",
                table: "Contacts",
                columns: new[] { "FirstName", "LastName" });

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
        }
    }
}
