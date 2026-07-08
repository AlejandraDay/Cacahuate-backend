using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cacahuate.DataAccess.Migrations;

public partial class SeedInitialUsers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var adminHash = BCrypt.Net.BCrypt.HashPassword("Control123");
        var therapistHash = BCrypt.Net.BCrypt.HashPassword("Control123");
        var parentHash = BCrypt.Net.BCrypt.HashPassword("Control123");

        migrationBuilder.Sql($"""
            INSERT INTO "Users" ("Id", "FirstName", "LastName", "Email", "PasswordHash", "Role", "IsActive", "CreatedAt")
            VALUES
                ('11111111-1111-1111-1111-111111111111', 'Admin', 'One', 'admin1@gmail.com', '{adminHash}', 'Admin', true, NOW()),
                ('22222222-2222-2222-2222-222222222222', 'Therapist', 'One', 'therapist1@gmail.com', '{therapistHash}', 'Therapist', true, NOW()),
                ('33333333-3333-3333-3333-333333333333', 'Parent', 'One', 'parent1@gmail.com', '{parentHash}', 'Parent', true, NOW())
            ON CONFLICT ("Email") DO NOTHING;

            INSERT INTO "Therapists" ("Id", "UserId", "Bio", "SessionDurationMinutes", "IsActive")
            SELECT 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '22222222-2222-2222-2222-222222222222', NULL, 60, true
            WHERE NOT EXISTS (SELECT 1 FROM "Therapists" WHERE "UserId" = '22222222-2222-2222-2222-222222222222');

            INSERT INTO "Parents" ("Id", "UserId")
            SELECT 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', '33333333-3333-3333-3333-333333333333'
            WHERE NOT EXISTS (SELECT 1 FROM "Parents" WHERE "UserId" = '33333333-3333-3333-3333-333333333333');
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DELETE FROM "Therapists" WHERE "UserId" = '22222222-2222-2222-2222-222222222222';
            DELETE FROM "Parents" WHERE "UserId" = '33333333-3333-3333-3333-333333333333';
            DELETE FROM "Users"
            WHERE "Email" IN ('admin1@gmail.com', 'therapist1@gmail.com', 'parent1@gmail.com');
            """);
    }
}
