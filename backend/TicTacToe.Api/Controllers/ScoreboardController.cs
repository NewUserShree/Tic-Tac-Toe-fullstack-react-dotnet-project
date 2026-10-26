using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Models;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api")]
public class ScoreboardController : ControllerBase
{
    private readonly ITicTacToeService _service;

    public ScoreboardController(ITicTacToeService service)
    {
        _service = service;
    }

    [HttpGet("scoreboard")]
    public ActionResult<Scoreboard> GetScoreboard()
    {
        return Ok(_service.GetScoreboard());
    }

    [HttpPost("scoreboard/reset")]
    public ActionResult<Scoreboard> ResetScoreboard()
    {
        return Ok(_service.ResetScoreboard());
    }
}
