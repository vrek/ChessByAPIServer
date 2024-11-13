namespace ChessByAPIServer.Models.APIModels;

public class ChessMove
{
    public Guid Piece { get; set; }
    public string StartPosition { get; set; }
    public string EndPosition { get; set; }
    public bool IsCapture { get; set; }
}