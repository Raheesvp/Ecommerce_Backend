using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdminUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "CreatedAt", "CreatedBy", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 21, 5, 33, 43, 424, DateTimeKind.Utc).AddTicks(2086), "System", "$2a$11$F/oVSopUMQgBk/WQViXPX.91mHXuT0H2vvGW8VN2OLAbGUFpd23pG" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "CreatedAt", "CreatedBy", "PasswordHash" },
                values: new object[] { new DateTime(2025, 12, 20, 22, 51, 30, 901, DateTimeKind.Utc).AddTicks(4437), null, "$2a$11$pcQkMXRdp22IBwWrZvCMiOQGzB/igTm2zp7HCGvDxHOvo2wD7f6TG" });
        }
    }
}
