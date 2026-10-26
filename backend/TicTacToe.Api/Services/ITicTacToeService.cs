using TicTacToe.Api.DTOs;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public interface ITicTacToeService
{
    GameSession CreateGame(GameMode mode);
    GameSession GetGame(string id);
    GameSession SubmitMove(string id, SubmitMoveRequest request);
    GameSession UndoMove(string id);
    GameSession ResetGame(string id);
    Scoreboard GetScoreboard();
    Scoreboard ResetScoreboard();
}
