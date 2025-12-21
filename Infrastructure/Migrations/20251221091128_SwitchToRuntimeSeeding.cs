using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SwitchToRuntimeSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 100);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Email", "FirstName", "IsBlocked", "IsDeleted", "LastModifiedAt", "LastModifiedBy", "LastName", "PasswordHash", "RefreshToken", "RefreshTokenExpiryTime", "Role" },
                values: new object[] { 100, new DateTime(2025, 12, 21, 5, 33, 43, 424, DateTimeKind.Utc).AddTicks(2086), "System", null, null, "rahees12@gmail.com", "Admin", false, false, null, null, "User", "$2a$11$F/oVSopUMQgBk/WQViXPX.91mHXuT0H2vvGW8VN2OLAbGUFpd23pG", null, null, 2 });
        }
    }
}
