using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cacahuate.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddTherapistToFormAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clear existing test assignments (and dependent submissions/answers) before adding required FK
            migrationBuilder.Sql("DELETE FROM \"FormAnswers\";");
            migrationBuilder.Sql("DELETE FROM \"FormSubmissions\";");
            migrationBuilder.Sql("DELETE FROM \"FormAssignments\";");

            migrationBuilder.AddColumn<Guid>(
                name: "TherapistId",
                table: "FormAssignments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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
                name: "IX_FormAssignments_TherapistId",
                table: "FormAssignments");

            migrationBuilder.DropColumn(
                name: "TherapistId",
                table: "FormAssignments");
        }
    }
}
