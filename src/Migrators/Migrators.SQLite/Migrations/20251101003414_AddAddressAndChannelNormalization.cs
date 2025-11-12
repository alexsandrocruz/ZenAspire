using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanAspire.Migrators.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class AddAddressAndChannelNormalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    OwnerType = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnerId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Line1 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Line2 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    District = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Zip = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Country = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false, defaultValue: "BR"),
                    GeoLat = table.Column<decimal>(type: "decimal(10,7)", nullable: true),
                    GeoLng = table.Column<decimal>(type: "decimal(10,7)", nullable: true),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    Label = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChannelIdentities",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    OwnerType = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnerId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    VerifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Label = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    OptedIn = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    OptedInAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelIdentities", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_GeoLat_GeoLng",
                table: "Addresses",
                columns: new[] { "GeoLat", "GeoLng" },
                filter: "\"GeoLat\" IS NOT NULL AND \"GeoLng\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_TenantId",
                table: "Addresses",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_TenantId_OwnerType_OwnerId",
                table: "Addresses",
                columns: new[] { "TenantId", "OwnerType", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_TenantId_OwnerType_OwnerId_IsPrimary",
                table: "Addresses",
                columns: new[] { "TenantId", "OwnerType", "OwnerId", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_ChannelIdentities_TenantId",
                table: "ChannelIdentities",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelIdentities_TenantId_OwnerType_OwnerId",
                table: "ChannelIdentities",
                columns: new[] { "TenantId", "OwnerType", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_ChannelIdentities_TenantId_OwnerType_OwnerId_Type_IsPrimary",
                table: "ChannelIdentities",
                columns: new[] { "TenantId", "OwnerType", "OwnerId", "Type", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_ChannelIdentities_TenantId_OwnerType_OwnerId_Type_Value",
                table: "ChannelIdentities",
                columns: new[] { "TenantId", "OwnerType", "OwnerId", "Type", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChannelIdentities_TenantId_Type_Value",
                table: "ChannelIdentities",
                columns: new[] { "TenantId", "Type", "Value" });

            migrationBuilder.CreateIndex(
                name: "IX_ChannelIdentities_TenantId_VerifiedAt",
                table: "ChannelIdentities",
                columns: new[] { "TenantId", "VerifiedAt" },
                filter: "\"VerifiedAt\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "ChannelIdentities");
        }
    }
}
