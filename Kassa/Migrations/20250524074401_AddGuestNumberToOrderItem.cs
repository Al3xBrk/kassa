using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kassa.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestNumberToOrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GuestNumber",
                table: "OrderItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuestNumber",
                table: "OrderItems");
        }
    }
}
