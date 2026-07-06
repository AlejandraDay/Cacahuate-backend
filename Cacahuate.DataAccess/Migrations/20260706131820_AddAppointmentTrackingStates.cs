using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cacahuate.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentTrackingStates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EnRouteAt",
                table: "Appointments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InProgressAt",
                table: "Appointments",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnRouteAt",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "InProgressAt",
                table: "Appointments");
        }
    }
}
