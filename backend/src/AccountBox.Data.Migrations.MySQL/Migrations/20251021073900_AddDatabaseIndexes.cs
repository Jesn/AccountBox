using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountBox.Data.Migrations.MySQL
{
    /// <inheritdoc />
    public partial class AddDatabaseIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Accounts_IsDeleted_DeletedAt",
                table: "Accounts",
                columns: new[] { "IsDeleted", "DeletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_WebsiteId_Username",
                table: "Accounts",
                columns: new[] { "WebsiteId", "Username" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accounts_IsDeleted_DeletedAt",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_WebsiteId_Username",
                table: "Accounts");
        }
    }
}
