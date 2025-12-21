using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 17, 11, 35, 15, 386, DateTimeKind.Utc).AddTicks(459), "$2a$11$WJxxHl2N61kYo7jY7mv6V.X2O7I3KMXebG1638Y/eOO7SD2/nOvxG" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 17, 11, 29, 31, 871, DateTimeKind.Utc).AddTicks(8863), "$2a$11$HWd2Prg4z/0MoD2es2LLXusOEjOnL6x/T/u950h.fAnbTG5EZS6nu" });
        }
    }
}
