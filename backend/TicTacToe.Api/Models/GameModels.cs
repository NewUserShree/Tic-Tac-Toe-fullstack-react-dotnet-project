namespace TicTacToe.Api.Models;

public enum Player
{
    X,
    O
}

public enum GameMode
{
    TwoPlayer,
    Computer
}

public enum GameStatus
{
    InProgress,
    Won,
    Draw
}

public class GameSession
{
    public string GameId { get; set; } = string.Empty;
    public List<List<string?>> Board { get; set; } = CreateEmptyBoard();
    public Player CurrentPlayer { get; set; } = Player.X;
    public GameMode Mode { get; set; }
    public GameStatus Status { get; set; } = GameStatus.InProgress;
    public Player? Winner { get; set; }
    public List<int> WinningCells { get; set; } = new();
    public List<MoveHistoryItem> MoveHistory { get; set; } = new();
    public bool CanUndo => MoveHistory.Count > 0 && Status == GameStatus.InProgress;
    public bool ScoreRecorded { get; set; }

    public static List<List<string?>> CreateEmptyBoard()
    {
        return new List<List<string?>>
        {
            new() { null, null, null },
            new() { null, null, null },
            new() { null, null, null }
        };
    }
}

public class MoveHistoryItem
{
    public int MoveNumber { get; set; }
    public Player Player { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
}

public class Scoreboard
{
    public int XWins { get; set; }
    public int OWins { get; set; }
    public int Draws { get; set; }
}
