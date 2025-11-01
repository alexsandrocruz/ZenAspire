using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanAspire.Migrators.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class AddTagsNotesAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    OwnerType = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnerId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Body = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    IsPinned = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    IsPrivate = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Color = table.Column<string>(type: "TEXT", maxLength: 7, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    NoteId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    BlobKey = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    FileHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attachments_Notes_NoteId",
                        column: x => x.NoteId,
                        principalTable: "Notes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TagLinks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    TagId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    OwnerType = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnerId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    LinkedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LinkedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TagLinks_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_BlobKey",
                table: "Attachments",
                column: "BlobKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_NoteId",
                table: "Attachments",
                column: "NoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_TenantId",
                table: "Attachments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_TenantId",
                table: "Notes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_TenantId_OwnerType_OwnerId",
                table: "Notes",
                columns: new[] { "TenantId", "OwnerType", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_Notes_TenantId_OwnerType_OwnerId_IsPinned",
                table: "Notes",
                columns: new[] { "TenantId", "OwnerType", "OwnerId", "IsPinned" });

            migrationBuilder.CreateIndex(
                name: "IX_TagLinks_OwnerType_OwnerId",
                table: "TagLinks",
                columns: new[] { "OwnerType", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_TagLinks_TagId",
                table: "TagLinks",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_TagLinks_TenantId",
                table: "TagLinks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TagLinks_TenantId_TagId_OwnerType_OwnerId",
                table: "TagLinks",
                columns: new[] { "TenantId", "TagId", "OwnerType", "OwnerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_TenantId",
                table: "Tags",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_TenantId_Name",
                table: "Tags",
                columns: new[] { "TenantId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "TagLinks");

            migrationBuilder.DropTable(
                name: "Notes");

            migrationBuilder.DropTable(
                name: "Tags");
        }
    }
}
