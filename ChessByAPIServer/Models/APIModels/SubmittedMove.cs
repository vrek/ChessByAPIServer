namespace ChessByAPIServer.Models.APIModels;

public class SubmittedMove
{
    public Guid gameId;
    public int fromPlayerId;
    public string move;
    public DateTime dateSubmitted;
}