using System.ComponentModel.DataAnnotations;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.DTOs;

public class SubmitMoveRequest
{
    public Player Player { get; set; }

    [Range(0, 2)]
    public int Row { get; set; }

    [Range(0, 2)]
    public int Column { get; set; }
}
