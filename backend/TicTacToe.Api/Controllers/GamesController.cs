using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.DTOs;
using TicTacToe.Api.Models;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api")]
public class GamesController : ControllerBase
{
    private readonly ITicTacToeService _service;

    public GamesController(ITicTacToeService service)
    {
        _service = service;
    }

    [HttpPost("games")]
    public ActionResult<GameStateResponse> CreateGame([FromBody] CreateGameRequest request)
    {
        var game = _service.CreateGame(request.Mode);
        return CreatedAtAction(nameof(GetGame), new { id = game.GameId }, MapToResponse(game));
    }

    [HttpGet("games/{id}")]
    public ActionResult<GameStateResponse> GetGame(string id)
    {
        try
        {
            var game = _service.GetGame(id);
            return Ok(MapToResponse(game));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Game not found." });
        }
    }

    [HttpPost("games/{id}/moves")]
    public ActionResult<GameStateResponse> SubmitMove(string id, [FromBody] SubmitMoveRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var game = _service.SubmitMove(id, request);
            return Ok(MapToResponse(game));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Game not found." });
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("games/{id}/undo")]
    public ActionResult<GameStateResponse> UndoMove(string id)
    {
        try
        {
            var game = _service.UndoMove(id);
            return Ok(MapToResponse(game));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Game not found." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("games/{id}/reset")]
    public ActionResult<GameStateResponse> ResetGame(string id)
    {
        try
        {
            var game = _service.ResetGame(id);
            return Ok(MapToResponse(game));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Game not found." });
        }
    }

    private static GameStateResponse MapToResponse(GameSession session)
    {
        return new GameStateResponse
        {
            GameId = session.GameId,
            Board = session.Board,
            CurrentPlayer = session.CurrentPlayer,
            Mode = session.Mode,
            Status = session.Status,
            Winner = session.Winner,
            WinningCells = session.WinningCells,
            MoveHistory = session.MoveHistory,
            CanUndo = session.CanUndo
        };
    }
}
