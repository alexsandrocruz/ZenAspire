using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanAspire.Migrators.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class AddActivitiesAndInteractions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    Start = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Due = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    RegardingType = table.Column<int>(type: "INTEGER", nullable: true),
                    RegardingId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AssignedToUserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    ReminderAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 2),
                    Location = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    DurationMinutes = table.Column<int>(type: "INTEGER", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Interactions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Direction = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Outbound"),
                    At = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OwnerType = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChannelRef = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Snippet = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", maxLength: 450, nullable: true),
                    DurationSeconds = table.Column<int>(type: "INTEGER", nullable: true),
                    HandledByUserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    Sentiment = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_AssignedToUserId",
                table: "Activities",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_Due",
                table: "Activities",
                column: "Due");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_Priority",
                table: "Activities",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_ReminderAt_Status",
                table: "Activities",
                columns: new[] { "ReminderAt", "Status" },
                filter: "\"ReminderAt\" IS NOT NULL AND \"Status\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_Start",
                table: "Activities",
                column: "Start");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_Status",
                table: "Activities",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_TenantId",
                table: "Activities",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_TenantId_AssignedToUserId_Status",
                table: "Activities",
                columns: new[] { "TenantId", "AssignedToUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_TenantId_RegardingType_RegardingId",
                table: "Activities",
                columns: new[] { "TenantId", "RegardingType", "RegardingId" });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_TenantId_Status_Due",
                table: "Activities",
                columns: new[] { "TenantId", "Status", "Due" });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_Type",
                table: "Activities",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Interactions_At",
                table: "Interactions",
                column: "At");

            migrationBuilder.CreateIndex(
                name: "IX_Interactions_ChannelRef_Unique",
                table: "Interactions",
                columns: new[] { "TenantId", "ChannelRef", "Type" },
                unique: true,
                filter: "\"ChannelRef\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Interactions_Direction",
                table: "Interactions",
                column: "Direction");

            migrationBuilder.CreateIndex(
                name: "IX_Interactions_HandledByUserId",
                table: "Interactions",
                column: "HandledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Interactions_OwnerType",
                table: "Interactions",
                column: "OwnerType");

            migrationBuilder.CreateIndex(
                name: "IX_Interactions_Sentiment",
                table: "Interactions",
                column: "Sentiment");

            migrationBuilder.CreateIndex(
                name: "IX_Interactions_TenantId",
                table: "Interactions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Interactions_TenantId_Direction_At",
                table: "Interactions",
                columns: new[] { "TenantId", "Direction", "At" });

            migrationBuilder.CreateIndex(
                name: "IX_Interactions_TenantId_HandledByUserId_At",
                table: "Interactions",
                columns: new[] { "TenantId", "HandledByUserId", "At" });

            migrationBuilder.CreateIndex(
                name: "IX_Interactions_TenantId_Type_At",
                table: "Interactions",
                columns: new[] { "TenantId", "Type", "At" });

            migrationBuilder.CreateIndex(
                name: "IX_Interactions_Timeline",
                table: "Interactions",
                columns: new[] { "TenantId", "OwnerType", "OwnerId", "At" });

            migrationBuilder.CreateIndex(
                name: "IX_Interactions_Type",
                table: "Interactions",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropTable(
                name: "Interactions");
        }
    }
}
