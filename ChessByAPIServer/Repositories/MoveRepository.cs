using System.Text.RegularExpressions;
using ChessByAPIServer.Contexts;
using ChessByAPIServer.Enum;
using ChessByAPIServer.Interfaces;
using ChessByAPIServer.Models;
using ChessByAPIServer.Models.APIModels;
using Microsoft.EntityFrameworkCore;

namespace ChessByAPIServer.Repositories;

public class MoveRepository : IMoveRepository
{
    private readonly GameRepository _gameRepository;
    private readonly IChessBoardRepository _board;
    private readonly Game _game;

    public MoveRepository(Game game, IChessBoardRepository board, GameRepository gameRepository)
    {
        _game = game ?? throw new ArgumentNullException(nameof(game));
        _board = board ?? throw new ArgumentNullException(nameof(board));
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
    }

    public async Task<bool> IsValidMove(string piece, string start, string end, PlayerRole? playerColor = null)
    {
        PlayerRole _color;
        if (string.IsNullOrEmpty(piece))
            throw new ArgumentException("Piece type must not be null or empty.", nameof(piece));
        if (playerColor != null)
            _color = (PlayerRole)playerColor;
        else
            _color = PlayerRole.All;

        var _startColumn = start[0].ToString().ToLower();
        var endColumn = end[0].ToString().ToLower();
        if (!int.TryParse(start[1].ToString(), out var startRow) || !int.TryParse(end[1].ToString(), out var endRow))
            return false; // Return false if parsing rows failed
        if (!IsPositionValid(_startColumn, startRow) || !IsPositionValid(endColumn, endRow)) return false;
        return piece.ToLower() switch
        {
            "pawn" => await IsPawnMoveValid(_startColumn, startRow, endColumn, endRow, _color),
            "knight" => IsKnightMoveValid(_startColumn, startRow, endColumn, endRow),
            "bishop" => IsBishopMoveValid(_startColumn, startRow, endColumn, endRow),
            "rook" => IsRookMoveValid(_startColumn, startRow, endColumn, endRow),
            "queen" => IsQueenMoveValid(_startColumn, startRow, endColumn, endRow),
            "king" => IsKingMoveValid(_startColumn, startRow, endColumn, endRow),
            _ => throw new ArgumentException("Invalid piece type", nameof(piece))
        };
    }

    public bool IsPositionValid(string column, int row)
    {
        // Validate column
        if (string.IsNullOrEmpty(column) || column.Length != 1)
            return false;

        // Validate column (must be between 'a' and 'h')
        var colChar = column[0];
        if (colChar < 'a' || colChar > 'h')
            return false;

        // Validate row (must be a digit between 1 and 8)
        if (row < 1 || row > 8)
            return false;

        return true; // All checks passed
    }

    public async Task<bool> IsPawnMoveValid(string startColumn, int startRow, string endColumn, int endRow,
        PlayerRole playerColor)
    {
        if (string.IsNullOrEmpty(playerColor.ToString())) return false;


        var isSameColumn = startColumn == endColumn;
        var isOneColumnAway = Math.Abs(startColumn[0] - endColumn[0]) == 1;

        if (playerColor == PlayerRole.White)
        {
            var isOneRowForward = endRow == startRow + 1;

            // Check if destination square is occupied for capture or regular move
            var isOccupied =
                await _board.IsSquareOccupied(_gameRepository.GetChessDbContext(), _game.Id, $"{endColumn}{endRow}");

            if (isOccupied)
            {
                // Capture only allowed diagonally (one row forward and one column to the left or right)
                if (isOneColumnAway && isOneRowForward) return await IsPieceOpponents(endColumn, endRow, playerColor);
            }
            else
            {
                // Regular forward moves
                if (isSameColumn && isOneRowForward)
                    return true;

                // Double-step forward from initial row if both squares are unoccupied
                if (startRow == 2 && isSameColumn && endRow == 4)
                {
                    var intermediateSquareOccupied = await _board.IsSquareOccupied(_gameRepository.GetChessDbContext(),
                        _game.Id, $"{startColumn}{startRow + 1}");
                    if (!intermediateSquareOccupied)
                        return true;
                }
            }
        }
        else if (playerColor == PlayerRole.Black)
        {
            var isOneRowBackward = endRow == startRow - 1;

            // Check if destination square is occupied for capture or regular move
            var isOccupied =
                await _board.IsSquareOccupied(_gameRepository.GetChessDbContext(), _game.Id, $"{endColumn}{endRow}");

            if (isOccupied)
            {
                // Capture only allowed diagonally (one row backward and one column to the left or right)
                if (isOneColumnAway && isOneRowBackward) return await IsPieceOpponents(endColumn, endRow, playerColor);
            }
            else
            {
                // Regular backward moves
                if (isSameColumn && isOneRowBackward)
                    return true;

                // Double-step backward from initial row if both squares are unoccupied
                if (startRow == 7 && isSameColumn && endRow == 5)
                {
                    var intermediateSquareOccupied = await _board.IsSquareOccupied(_gameRepository.GetChessDbContext(),
                        _game.Id, $"{startColumn}{startRow - 1}");
                    if (!intermediateSquareOccupied)
                        return true;
                }
            }
        }

        return false;
    }

    public async Task<bool> IsPieceOpponents(string endColumn, int endRow, PlayerRole playerColor)
    {
        var pieceColor = await _board.GetPieceColorAtPositionAsync(_gameRepository.GetChessDbContext(), _game.Id,
            $"{endColumn}{endRow}");
        if (pieceColor == null) throw new ArgumentException("Invalid piece color", nameof(pieceColor));
        if (pieceColor != playerColor) return true;
        return false;
    }


    public bool IsKnightMoveValid(string startColumn, int startRow, string endColumn, int endRow)
    {
        // Check for valid start and end positions
        if (string.IsNullOrEmpty(startColumn) || string.IsNullOrEmpty(endColumn))
            return false;

        // Convert columns from letters to numbers (a=1, b=2, ..., h=8)
        var startColNum = startColumn[0] - 'a' + 1;
        var endColNum = endColumn[0] - 'a' + 1;

        // Calculate the differences in column and row
        var colDiff = Math.Abs(endColNum - startColNum);
        var rowDiff = Math.Abs(endRow - startRow);

        // A knight moves in an "L" shape: (2, 1) or (1, 2)
        return (colDiff == 2 && rowDiff == 1) || (colDiff == 1 && rowDiff == 2);
    }

    public bool IsBishopMoveValid(string startColumn, int startRow, string endColumn, int endRow)
    {
        throw new NotImplementedException();
    }

    public bool IsRookMoveValid(string startColumn, int startRow, string endColumn, int endRow)
    {
        throw new NotImplementedException();
    }

    public bool IsQueenMoveValid(string startColumn, int startRow, string endColumn, int endRow)
    {
        throw new NotImplementedException();
    }

    public bool IsKingMoveValid(string startColumn, int startRow, string endColumn, int endRow)
    {
        throw new NotImplementedException();
    }

    public bool IsCastlingMoveValid(string startColumn, int startRow, string endColumn, int endRow)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> IsEnPassantMoveValidAsync(int startRow, string endColumn, int endRow)
    {
        var context = _gameRepository.GetChessDbContext();

        var endColChar = char.ToLower(endColumn[0]);

        // Check if the target square to the left or right of the pawn is occupied
        var leftPosition = $"{(char)(endColChar - 1)}{startRow}";
        var rightPosition = $"{(char)(endColChar + 1)}{startRow}";

        var leftPiece = await _board.GetPieceAtPositionAsync(context, _game.Id, leftPosition);
        var rightPiece = await _board.GetPieceAtPositionAsync(context, _game.Id, rightPosition);

        if (leftPiece == null && rightPiece == null) return false;

        // If either adjacent square has a piece, it could be a valid en passant move
        return true;
    }

    public bool IsInCheck(string position)
    {
        throw new NotImplementedException();
    }

    public bool IsInCheckMate(string position)
    {
        throw new NotImplementedException();
    }

    public bool IsInStalemate()
    {
        throw new NotImplementedException();
    }

    public async Task SetSquareOccupiedAsync(string square, string pieceType, PlayerRole color)
    {
        await _board.UpdatePositionAsync(_gameRepository.GetChessDbContext(), _game.Id, square, pieceType, color);
    }

    public async Task AddMoveToDbAsync(string startPosition, string endPosition, PlayerRole color)
    {
        var context = _gameRepository.GetChessDbContext();
        var gameMove = new GameMove();
        var piece = await _board.GetPieceAtPositionAsync(context, _game.Id, startPosition) ?? string.Empty;
        gameMove.GameGuid = _game.Id;
        gameMove.MoveNumber = await GetMostRecentMoveNumberAsync(context, _game.Id) + 1;
        gameMove.MoveNotation = await GetLongAlgebraicNotationAsync(context, piece, startPosition, endPosition, color.ToString());
        context.GameMoves.Add(gameMove);
        await context.SaveChangesAsync();
    }
    

    public async Task<string> GetLongAlgebraicNotationAsync(
        ChessDbContext context,
        string piece,
        string startPosition,
        string endPosition,
        string? color
    )
    {
        // Check if there is an opponent's piece on the end position
        var targetPosition = await context.ChessPositions
            .FirstOrDefaultAsync(p => p.GameId == _game.Id && p.Position == endPosition);

        var isCapture = targetPosition != null && targetPosition.PieceColor != color;

        string moveNotation;
        // If piece is a Knight, use "N"; otherwise, use the first letter of the piece
        if (piece.Equals("Knight", StringComparison.OrdinalIgnoreCase))
            moveNotation = "N";
        else
            moveNotation = piece.Substring(0, 1).ToUpper();

        if (isCapture)
            moveNotation += startPosition + "x" + endPosition;
        else
            moveNotation += startPosition + "-" + endPosition;

        return moveNotation;
    }

    public async Task<int> GetMostRecentMoveNumberAsync(ChessDbContext context, Guid gameGuid)
    {
        var mostRecentMoveNumber = await context.GameMoves
            .Where(move => move.GameGuid == gameGuid)
            .OrderByDescending(move => move.MoveNumber)
            .Select(move => move.MoveNumber)
            .FirstOrDefaultAsync();

        return mostRecentMoveNumber;
    }

    public bool IsEndMoveDiagonal(string startColumn, int startRow, string endColumn, int endRow)
    {
        // Validate input lengths first
        if (startColumn.Length != 1 || endColumn.Length != 1) return false;

        // Convert columns ('a'-'h') to numbers (1-8)
        var startColNum = startColumn[0] - 'a' + 1;
        var endColNum = endColumn[0] - 'a' + 1;

        // Check if the move is diagonal
        return Math.Abs(startColNum - endColNum) == Math.Abs(startRow - endRow);
    }
    
        public ChessMove FromLongAlgebraicNotation(string notation)
        {
            var chessMove = new ChessMove();
            notation = notation.Trim();

            // Define a regex to parse long algebraic notation
            // Example pattern breakdown:
            // ^([NBRQK]?) - optional piece symbol (e.g., "N" for Knight)
            // ([a-h]?) - optional starting file for disambiguation
            // ([1-8]?) - optional starting rank for disambiguation
            // (x?) - optional capture indicator
            // ([a-h][1-8]) - mandatory destination square
            var regex = new Regex(@"^([NBRQK]?)([a-h]?)([1-8]?)(x?)([a-h][1-8])$");

            var match = regex.Match(notation);
            if (!match.Success)
            {
                throw new ArgumentException("Invalid long algebraic notation", nameof(notation));
            }

            // Extract piece, starting position (if specified), capture status, and end position
            var pieceSymbol = match.Groups[1].Value;
            var startFile = match.Groups[2].Value;
            var startRank = match.Groups[3].Value;
            var captureSymbol = match.Groups[4].Value;
            var endPosition = match.Groups[5].Value;

            // Set piece type based on the symbol
            chessMove.Piece = pieceSymbol switch
            {
                "N" => Guid.NewGuid(), // Replace with actual Knight piece GUID if available
                "B" => Guid.NewGuid(), // Bishop
                "R" => Guid.NewGuid(), // Rook
                "Q" => Guid.NewGuid(), // Queen
                "K" => Guid.NewGuid(), // King
                _ => Guid.NewGuid() // Pawn (no symbol in long algebraic notation)
            };

            // Set capture status
            chessMove.IsCapture = captureSymbol == "x";

            // Set end position
            chessMove.EndPosition = endPosition;

            // Set start position if specified (for disambiguation purposes)
            chessMove.StartPosition = !string.IsNullOrEmpty(startFile) || !string.IsNullOrEmpty(startRank)
                ? $"{startFile}{startRank}"
                : null; // null if no start position specified

            return chessMove;
        }
    }