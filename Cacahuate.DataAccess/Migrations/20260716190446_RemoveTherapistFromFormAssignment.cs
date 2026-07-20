using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cacahuate.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTherapistFromFormAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormAssignments_Therapists_TherapistId",
                table: "FormAssignments");

            migrationBuilder.DropIndex(
                name: "IX_FormAssignments_TherapistId",
                table: "FormAssignments");

            migrationBuilder.DropColumn(
                name: "TherapistId",
                table: "FormAssignments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TherapistId",
                table: "FormAssignments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormAssignments_TherapistId",
                table: "FormAssignments",
                column: "TherapistId");

            migrationBuilder.AddForeignKey(
                name: "FK_FormAssignments_Therapists_TherapistId",
                table: "FormAssignments",
                column: "TherapistId",
                principalTable: "Therapists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
