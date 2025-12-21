using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "IsBlocked", "LastName", "PasswordHash", "RefreshToken", "RefreshTokenExpiryTime", "Role", "UpdatedAt" },
                values: new object[] { 100, new DateTime(2025, 12, 17, 11, 29, 31, 871, DateTimeKind.Utc).AddTicks(8863), "rahees12@gmail.com", "Admin", false, "User", "$2a$11$HWd2Prg4z/0MoD2es2LLXusOEjOnL6x/T/u950h.fAnbTG5EZS6nu", null, null, 2, null });

            migrationBuilder.CreateIndex(
                name: "IX_Wishlists_UserId_ProductId",
                table: "Wishlists",
                columns: new[] { "UserId", "ProductId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Wishlists_UserId_ProductId",
                table: "Wishlists");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 100);
        }
    }
}
