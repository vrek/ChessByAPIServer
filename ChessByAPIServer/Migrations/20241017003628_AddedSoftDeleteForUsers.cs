using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChessByAPIServer.Migrations;

/// <inheritdoc />
public partial class AddedSoftDeleteForUsers : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.AddColumn<DateTime>(
            name: "DateDeleted",
            table: "Users",
            type: "datetime2",
            nullable: true);

        _ = migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "Users",
            type: "bit",
            nullable: false,
            defaultValue: false);

        _ = migrationBuilder.AlterColumn<DateTime>(
            name: "EndTime",
            table: "Games",
            type: "datetime2",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime2");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.DropColumn(
            name: "DateDeleted",
            table: "Users");

        _ = migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "Users");

        _ = migrationBuilder.AlterColumn<DateTime>(
            name: "EndTime",
            table: "Games",
            type: "datetime2",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true);
    }
}
