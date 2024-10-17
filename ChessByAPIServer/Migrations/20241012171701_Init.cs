using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChessByAPIServer.Migrations;

/// <inheritdoc />
public partial class Init : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Password = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_Users", x => x.Id);
            });

        _ = migrationBuilder.CreateTable(
            name: "Games",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                WhitePlayerId = table.Column<int>(type: "int", nullable: false),
                BlackPlayerId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_Games", x => x.Id);
                _ = table.ForeignKey(
                    name: "FK_Games_Users_BlackPlayerId",
                    column: x => x.BlackPlayerId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                _ = table.ForeignKey(
                    name: "FK_Games_Users_WhitePlayerId",
                    column: x => x.WhitePlayerId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        _ = migrationBuilder.CreateTable(
            name: "ChessPositions",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Position = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                IsEmpty = table.Column<bool>(type: "bit", nullable: false),
                Piece = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_ChessPositions", x => x.Id);
                _ = table.ForeignKey(
                    name: "FK_ChessPositions_Games_GameId",
                    column: x => x.GameId,
                    principalTable: "Games",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        _ = migrationBuilder.CreateTable(
            name: "GameMoves",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                GameGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                MoveNumber = table.Column<int>(type: "int", nullable: false),
                MoveNotation = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_GameMoves", x => x.Id);
                _ = table.ForeignKey(
                    name: "FK_GameMoves_Games_GameGuid",
                    column: x => x.GameGuid,
                    principalTable: "Games",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        _ = migrationBuilder.CreateIndex(
            name: "IX_ChessPositions_GameId",
            table: "ChessPositions",
            column: "GameId");

        _ = migrationBuilder.CreateIndex(
            name: "IX_GameMoves_GameGuid",
            table: "GameMoves",
            column: "GameGuid");

        _ = migrationBuilder.CreateIndex(
            name: "IX_Games_BlackPlayerId",
            table: "Games",
            column: "BlackPlayerId");

        _ = migrationBuilder.CreateIndex(
            name: "IX_Games_WhitePlayerId",
            table: "Games",
            column: "WhitePlayerId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.DropTable(
            name: "ChessPositions");

        _ = migrationBuilder.DropTable(
            name: "GameMoves");

        _ = migrationBuilder.DropTable(
            name: "Games");

        _ = migrationBuilder.DropTable(
            name: "Users");
    }
}
