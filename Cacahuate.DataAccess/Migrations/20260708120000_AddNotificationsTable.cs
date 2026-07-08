using System;
using Cacahuate.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cacahuate.DataAccess.Migrations;

public partial class AddNotificationsTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Use idempotent SQL so migration succeeds if the table already exists in the target DB
        migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS ""Notifications"" (
            ""Id"" uuid NOT NULL,
            ""UserId"" uuid NOT NULL,
            ""Title"" text NOT NULL,
            ""Message"" text NOT NULL,
            ""IsRead"" boolean NOT NULL,
            ""CreatedAt"" timestamp with time zone NOT NULL,
            ""AppointmentId"" uuid,
            CONSTRAINT ""PK_Notifications"" PRIMARY KEY (""Id""
        ));");

        migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_Notifications_UserId"" ON ""Notifications"" (""UserId"");");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Notifications");
    }
}
