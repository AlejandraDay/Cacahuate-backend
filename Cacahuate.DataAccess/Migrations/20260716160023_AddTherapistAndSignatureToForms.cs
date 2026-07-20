using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cacahuate.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddTherapistAndSignatureToForms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FormSubmissions_AppointmentId",
                table: "FormSubmissions");

            migrationBuilder.AddColumn<string>(
                name: "ParentSignature",
                table: "FormSubmissions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ParentSignedAt",
                table: "FormSubmissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TherapistId",
                table: "FormAssignments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormSubmissions_AppointmentId",
                table: "FormSubmissions",
                column: "AppointmentId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormAssignments_Therapists_TherapistId",
                table: "FormAssignments");

            migrationBuilder.DropIndex(
                name: "IX_FormSubmissions_AppointmentId",
                table: "FormSubmissions");

            migrationBuilder.DropIndex(
                name: "IX_FormAssignments_TherapistId",
                table: "FormAssignments");

            migrationBuilder.DropColumn(
                name: "ParentSignature",
                table: "FormSubmissions");

            migrationBuilder.DropColumn(
                name: "ParentSignedAt",
                table: "FormSubmissions");

            migrationBuilder.DropColumn(
                name: "TherapistId",
                table: "FormAssignments");

            migrationBuilder.CreateIndex(
                name: "IX_FormSubmissions_AppointmentId",
                table: "FormSubmissions",
                column: "AppointmentId",
                unique: true);
        }
    }
}
