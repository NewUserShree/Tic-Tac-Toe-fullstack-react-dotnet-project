using TicTacToe.Api.Models;

namespace TicTacToe.Api.DTOs;

public class CreateGameRequest
{
    public GameMode Mode { get; set; } = GameMode.TwoPlayer;
}
