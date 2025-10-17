using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountBox.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEncryption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiKeys_KeySlots_VaultId",
                table: "ApiKeys");

            migrationBuilder.DropTable(
                name: "KeySlots");

            migrationBuilder.DropIndex(
                name: "IX_ApiKeys_VaultId",
                table: "ApiKeys");

            migrationBuilder.DropColumn(
                name: "VaultId",
                table: "ApiKeys");

            migrationBuilder.DropColumn(
                name: "NotesEncrypted",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "NotesIV",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "NotesTag",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "PasswordEncrypted",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "PasswordIV",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "PasswordTag",
                table: "Accounts");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Accounts",
                type: "TEXT",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "Accounts");

            migrationBuilder.AddColumn<int>(
                name: "VaultId",
                table: "ApiKeys",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>(
                name: "NotesEncrypted",
                table: "Accounts",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "NotesIV",
                table: "Accounts",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "NotesTag",
                table: "Accounts",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "PasswordEncrypted",
                table: "Accounts",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "PasswordIV",
                table: "Accounts",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "PasswordTag",
                table: "Accounts",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateTable(
                name: "KeySlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Argon2Iterations = table.Column<int>(type: "INTEGER", nullable: false),
                    Argon2MemorySize = table.Column<int>(type: "INTEGER", nullable: false),
                    Argon2Parallelism = table.Column<int>(type: "INTEGER", nullable: false),
                    Argon2Salt = table.Column<byte[]>(type: "BLOB", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EncryptedVaultKey = table.Column<byte[]>(type: "BLOB", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    VaultKeyIV = table.Column<byte[]>(type: "BLOB", nullable: false),
                    VaultKeyTag = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeySlots", x => x.Id);
                    table.CheckConstraint("CK_KeySlot_Singleton", "[Id] = 1");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_VaultId",
                table: "ApiKeys",
                column: "VaultId");

            migrationBuilder.CreateIndex(
                name: "IX_KeySlots_CreatedAt",
                table: "KeySlots",
                column: "CreatedAt");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiKeys_KeySlots_VaultId",
                table: "ApiKeys",
                column: "VaultId",
                principalTable: "KeySlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
