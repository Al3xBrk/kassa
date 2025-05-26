using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kassa.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentTimeAndCashDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CashGiven",
                table: "Orders",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Change",
                table: "Orders",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentTime",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CashGiven",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Change",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentTime",
                table: "Orders");
        }
    }
}
