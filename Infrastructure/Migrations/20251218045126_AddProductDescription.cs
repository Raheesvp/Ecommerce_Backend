using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 18, 4, 51, 25, 678, DateTimeKind.Utc).AddTicks(6997), "$2a$11$ifDMKKgbmFCxmeeKPIp.MONWjT7e2FcHr1pIK1Cbs1Ij.g5H9ux4G" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Products");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 17, 15, 54, 55, 489, DateTimeKind.Utc).AddTicks(6413), "$2a$11$o2WLvtGZ8ZzE08DF9YfvuuWvQpgao8eHsRZQDOjtMtqbC5DgccUra" });
        }
    }
}
