using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kassa.Migrations
{
    /// <inheritdoc />
    public partial class EditTimeReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "From",
                table: "TableReservations");

            migrationBuilder.RenameColumn(
                name: "To",
                table: "TableReservations",
                newName: "Date");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "FromTime",
                table: "TableReservations",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "ToTime",
                table: "TableReservations",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromTime",
                table: "TableReservations");

            migrationBuilder.DropColumn(
                name: "ToTime",
                table: "TableReservations");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "TableReservations",
                newName: "To");

            migrationBuilder.AddColumn<DateTime>(
                name: "From",
                table: "TableReservations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
