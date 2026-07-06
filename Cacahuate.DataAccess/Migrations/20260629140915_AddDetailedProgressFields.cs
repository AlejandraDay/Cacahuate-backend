using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cacahuate.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddDetailedProgressFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProgressAchievements",
                table: "Appointments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProgressAreasToReinforce",
                table: "Appointments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProgressBehavior",
                table: "Appointments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProgressCommunication",
                table: "Appointments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProgressObjectives",
                table: "Appointments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProgressParticipationLevel",
                table: "Appointments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProgressRecommendations",
                table: "Appointments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProgressSensoryResponse",
                table: "Appointments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProgressSocialInteraction",
                table: "Appointments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProgressAchievements",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ProgressAreasToReinforce",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ProgressBehavior",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ProgressCommunication",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ProgressObjectives",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ProgressParticipationLevel",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ProgressRecommendations",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ProgressSensoryResponse",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ProgressSocialInteraction",
                table: "Appointments");
        }
    }
}
