using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountBox.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KeySlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EncryptedVaultKey = table.Column<byte[]>(type: "BLOB", nullable: false),
                    VaultKeyIV = table.Column<byte[]>(type: "BLOB", nullable: false),
                    VaultKeyTag = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Argon2Salt = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Argon2Iterations = table.Column<int>(type: "INTEGER", nullable: false),
                    Argon2MemorySize = table.Column<int>(type: "INTEGER", nullable: false),
                    Argon2Parallelism = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeySlots", x => x.Id);
                    table.CheckConstraint("CK_KeySlot_Singleton", "[Id] = 1");
                });

            migrationBuilder.CreateTable(
                name: "Websites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Domain = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Websites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WebsiteId = table.Column<int>(type: "INTEGER", nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    PasswordEncrypted = table.Column<byte[]>(type: "BLOB", nullable: false),
                    PasswordIV = table.Column<byte[]>(type: "BLOB", nullable: false),
                    PasswordTag = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 5000, nullable: true),
                    NotesEncrypted = table.Column<byte[]>(type: "BLOB", nullable: true),
                    NotesIV = table.Column<byte[]>(type: "BLOB", nullable: true),
                    NotesTag = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accounts_Websites_WebsiteId",
                        column: x => x.WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_CreatedAt",
                table: "Accounts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_DeletedAt",
                table: "Accounts",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_IsDeleted",
                table: "Accounts",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Username",
                table: "Accounts",
                column: "Username");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_WebsiteId",
                table: "Accounts",
                column: "WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_WebsiteId_IsDeleted",
                table: "Accounts",
                columns: new[] { "WebsiteId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_KeySlots_CreatedAt",
                table: "KeySlots",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Websites_CreatedAt",
                table: "Websites",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Websites_DisplayName",
                table: "Websites",
                column: "DisplayName");

            migrationBuilder.CreateIndex(
                name: "IX_Websites_Domain",
                table: "Websites",
                column: "Domain",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "KeySlots");

            migrationBuilder.DropTable(
                name: "Websites");
        }
    }
}
