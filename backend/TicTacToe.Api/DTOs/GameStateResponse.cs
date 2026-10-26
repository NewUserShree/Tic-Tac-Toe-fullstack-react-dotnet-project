using TicTacToe.Api.Models;

namespace TicTacToe.Api.DTOs;

public class GameStateResponse
{
    public string GameId { get; set; } = string.Empty;
    public List<List<string?>> Board { get; set; } = new();
    public Player CurrentPlayer { get; set; }
    public GameMode Mode { get; set; }
    public GameStatus Status { get; set; }
    public Player? Winner { get; set; }
    public List<int> WinningCells { get; set; } = new();
    public List<MoveHistoryItem> MoveHistory { get; set; } = new();
    public bool CanUndo { get; set; }
}
