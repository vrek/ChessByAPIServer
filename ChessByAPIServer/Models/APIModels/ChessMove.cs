namespace ChessByAPIServer.Models.APIModels;

public class ChessMove
{
    public Guid piece { get; set; }
    public string startPosition { get; set; }
    public string endPosition { get; set; }
    public bool isCapture { get; set; }
}