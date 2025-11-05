using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanAspire.Migrators.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class AddSegmentEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Segments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    DefinitionJson = table.Column<string>(type: "jsonb", maxLength: 450, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastRebuiltAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MemberCount = table.Column<int>(type: "INTEGER", nullable: true),
                    TargetOwnerTypes = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Segments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SegmentMemberships",
                columns: table => new
                {
                    SegmentId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    OwnerType = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnerId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    ComputedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentMemberships", x => new { x.SegmentId, x.TenantId, x.OwnerType, x.OwnerId });
                    table.ForeignKey(
                        name: "FK_SegmentMemberships_Segments_SegmentId",
                        column: x => x.SegmentId,
                        principalTable: "Segments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SegmentMemberships_ComputedAt",
                table: "SegmentMemberships",
                column: "ComputedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentMemberships_Segment_ComputedAt",
                table: "SegmentMemberships",
                columns: new[] { "SegmentId", "ComputedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SegmentMemberships_Segment_Tenant_Type",
                table: "SegmentMemberships",
                columns: new[] { "SegmentId", "TenantId", "OwnerType" });

            migrationBuilder.CreateIndex(
                name: "IX_SegmentMemberships_Tenant_Type_Owner",
                table: "SegmentMemberships",
                columns: new[] { "TenantId", "OwnerType", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_Segments_LastRebuiltAt",
                table: "Segments",
                column: "LastRebuiltAt");

            migrationBuilder.CreateIndex(
                name: "IX_Segments_TenantId",
                table: "Segments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Segments_TenantId_IsActive",
                table: "Segments",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Segments_TenantId_Name_Unique",
                table: "Segments",
                columns: new[] { "TenantId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SegmentMemberships");

            migrationBuilder.DropTable(
                name: "Segments");
        }
    }
}
