using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AccountBox.Data.Migrations.PostgreSQL
{
    /// <inheritdoc />
    public partial class AddDatabaseIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PostgreSQL 专用迁移：仅添加索引
            // 注意：InitialCreate 迁移已经创建了所有表和列，这里只需要添加索引
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
            // 回滚时删除索引
            migrationBuilder.DropIndex(
                name: "IX_Accounts_IsDeleted_DeletedAt",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_WebsiteId_Username",
                table: "Accounts");
        }
    }
}
