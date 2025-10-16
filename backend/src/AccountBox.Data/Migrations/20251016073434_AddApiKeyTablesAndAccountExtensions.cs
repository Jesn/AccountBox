using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountBox.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddApiKeyTablesAndAccountExtensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExtendedData",
                table: "Accounts",
                type: "TEXT",
                maxLength: 10240,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Accounts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ApiKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    KeyPlaintext = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    KeyHash = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ScopeType = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LastUsedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    VaultId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiKeys_KeySlots_VaultId",
                        column: x => x.VaultId,
                        principalTable: "KeySlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApiKeyWebsiteScopes",
                columns: table => new
                {
                    ApiKeyId = table.Column<int>(type: "INTEGER", nullable: false),
                    WebsiteId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeyWebsiteScopes", x => new { x.ApiKeyId, x.WebsiteId });
                    table.ForeignKey(
                        name: "FK_ApiKeyWebsiteScopes_ApiKeys_ApiKeyId",
                        column: x => x.ApiKeyId,
                        principalTable: "ApiKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApiKeyWebsiteScopes_Websites_WebsiteId",
                        column: x => x.WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_KeyPlaintext",
                table: "ApiKeys",
                column: "KeyPlaintext",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_VaultId",
                table: "ApiKeys",
                column: "VaultId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeyWebsiteScopes_WebsiteId",
                table: "ApiKeyWebsiteScopes",
                column: "WebsiteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiKeyWebsiteScopes");

            migrationBuilder.DropTable(
                name: "ApiKeys");

            migrationBuilder.DropColumn(
                name: "ExtendedData",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Accounts");
        }
    }
}
