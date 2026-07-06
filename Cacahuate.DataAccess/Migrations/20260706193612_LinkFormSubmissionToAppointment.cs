using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cacahuate.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class LinkFormSubmissionToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clear existing test data before schema changes
            migrationBuilder.Sql("DELETE FROM \"FormAnswers\";");
            migrationBuilder.Sql("DELETE FROM \"FormSubmissions\";");
            migrationBuilder.Sql("DELETE FROM \"FormAssignments\";");

            migrationBuilder.DropForeignKey(
                name: "FK_FormAssignments_Therapists_TherapistId",
                table: "FormAssignments");

            migrationBuilder.DropIndex(
                name: "IX_FormSubmissions_FormAssignmentId",
                table: "FormSubmissions");

            migrationBuilder.DropIndex(
                name: "IX_FormAssignments_TherapistId",
                table: "FormAssignments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "FormAssignments");

            migrationBuilder.DropColumn(
                name: "TherapistId",
                table: "FormAssignments");

            migrationBuilder.AddColumn<Guid>(
                name: "AppointmentId",
                table: "FormSubmissions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_FormSubmissions_AppointmentId",
                table: "FormSubmissions",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_FormSubmissions_FormAssignmentId_AppointmentId",
                table: "FormSubmissions",
                columns: new[] { "FormAssignmentId", "AppointmentId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FormSubmissions_Appointments_AppointmentId",
                table: "FormSubmissions",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormSubmissions_Appointments_AppointmentId",
                table: "FormSubmissions");

            migrationBuilder.DropIndex(
                name: "IX_FormSubmissions_AppointmentId",
                table: "FormSubmissions");

            migrationBuilder.DropIndex(
                name: "IX_FormSubmissions_FormAssignmentId_AppointmentId",
                table: "FormSubmissions");

            migrationBuilder.DropColumn(
                name: "AppointmentId",
                table: "FormSubmissions");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "FormAssignments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "TherapistId",
                table: "FormAssignments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_FormSubmissions_FormAssignmentId",
                table: "FormSubmissions",
                column: "FormAssignmentId",
                unique: true);

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
