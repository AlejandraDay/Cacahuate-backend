using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cacahuate.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveParentSignatureFromFormSubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentSignature",
                table: "FormSubmissions");

            migrationBuilder.DropColumn(
                name: "ParentSignedAt",
                table: "FormSubmissions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
