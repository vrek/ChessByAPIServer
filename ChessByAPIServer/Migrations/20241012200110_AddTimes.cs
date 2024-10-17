using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChessByAPIServer.Migrations;

/// <inheritdoc />
public partial class AddTimes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.AddColumn<DateTime>(
            name: "EndTime",
            table: "Games",
            type: "datetime2",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

        _ = migrationBuilder.AddColumn<DateTime>(
            name: "StartTime",
            table: "Games",
            type: "datetime2",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.DropColumn(
            name: "EndTime",
            table: "Games");

        _ = migrationBuilder.DropColumn(
            name: "StartTime",
            table: "Games");
    }
}
