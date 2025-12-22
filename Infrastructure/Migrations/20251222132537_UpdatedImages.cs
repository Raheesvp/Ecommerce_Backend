using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "OrderItem");

            migrationBuilder.RenameColumn(
                name: "StippingAddresss",
                table: "Orders",
                newName: "StippingAddress");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StippingAddress",
                table: "Orders",
                newName: "StippingAddresss");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "OrderItem",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
