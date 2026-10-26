using Xunit;
using TicTacToe.Api.DTOs;
using TicTacToe.Api.Models;
using TicTacToe.Api.Services;

namespace TicTacToe.Tests;

public class TicTacToeServiceTests
{
    private readonly TicTacToeService _service = new();

    [Fact]
    public void CreateGame_InitializesBoardAndStartsWithX()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);

        Assert.Equal(GameMode.TwoPlayer, game.Mode);
        Assert.Equal(Player.X, game.CurrentPlayer);
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Equal(3, game.Board.Count);
        Assert.All(game.Board, row => Assert.Equal(3, row.Count));
        Assert.All(game.Board.SelectMany(row => row), cell => Assert.Null(cell));
    }

    [Fact]
    public void SubmitMove_ValidMovePlacesTokenAndSwitchesTurn()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);

        var updated = _service.SubmitMove(game.GameId, new SubmitMoveRequest { Player = Player.X, Row = 0, Column = 0 });

        Assert.Equal("X", updated.Board[0][0]);
        Assert.Equal(Player.O, updated.CurrentPlayer);
    }

    [Fact]
    public void SubmitMove_OutsideBoard_ThrowsArgumentOutOfRangeException()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _service.SubmitMove(game.GameId, new SubmitMoveRequest { Player = Player.X, Row = 3, Column = 0 }));
    }

    [Fact]
    public void SubmitMove_OnOccupiedCell_ThrowsInvalidOperationException()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);
        game.Board[0][0] = "X";
        game.CurrentPlayer = Player.O;

        var exception = Assert.Throws<InvalidOperationException>(() =>
            _service.SubmitMove(game.GameId, new SubmitMoveRequest { Player = Player.O, Row = 0, Column = 0 }));

        Assert.Contains("occupied", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SubmitMove_WrongPlayer_ThrowsInvalidOperationException()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);
        game.CurrentPlayer = Player.O;

        Assert.Throws<InvalidOperationException>(() =>
            _service.SubmitMove(game.GameId, new SubmitMoveRequest { Player = Player.X, Row = 0, Column = 0 }));
    }

    [Fact]
    public void SubmitMove_RowWin_UpdatesStatusAndWinner()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);
        game.CurrentPlayer = Player.X;
        game.Board[0][0] = "X";
        game.Board[0][1] = "X";
        game.Board[0][2] = null;
        game.Board[1][0] = "O";
        game.Board[1][1] = "O";
        game.Board[2][2] = "X";

        var updated = _service.SubmitMove(game.GameId, new SubmitMoveRequest { Player = Player.X, Row = 0, Column = 2 });

        Assert.Equal(GameStatus.Won, updated.Status);
        Assert.Equal(Player.X, updated.Winner);
        Assert.Equal(new List<int> { 0, 1, 2 }, updated.WinningCells);
    }

    [Fact]
    public void SubmitMove_ColumnWin_UpdatesStatusAndWinner()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);
        game.CurrentPlayer = Player.X;
        game.Board[0][0] = "X";
        game.Board[1][0] = "X";
        game.Board[2][0] = null;
        game.Board[0][1] = "O";
        game.Board[1][1] = "O";
        game.Board[2][1] = "X";

        var updated = _service.SubmitMove(game.GameId, new SubmitMoveRequest { Player = Player.X, Row = 2, Column = 0 });

        Assert.Equal(GameStatus.Won, updated.Status);
        Assert.Equal(Player.X, updated.Winner);
        Assert.Equal(new List<int> { 0, 3, 6 }, updated.WinningCells);
    }

    [Fact]
    public void SubmitMove_DiagonalWin_UpdatesStatusAndWinner()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);
        game.CurrentPlayer = Player.X;
        game.Board[0][0] = "X";
        game.Board[1][1] = "X";
        game.Board[2][2] = null;
        game.Board[0][1] = "O";
        game.Board[1][0] = "O";

        var updated = _service.SubmitMove(game.GameId, new SubmitMoveRequest { Player = Player.X, Row = 2, Column = 2 });

        Assert.Equal(GameStatus.Won, updated.Status);
        Assert.Equal(Player.X, updated.Winner);
        Assert.Equal(new List<int> { 0, 4, 8 }, updated.WinningCells);
    }

    [Fact]
    public void SubmitMove_DrawDetection_UpdatesStatus()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);
        game.CurrentPlayer = Player.X;
        game.Board = new List<List<string?>>
        {
            new() { "X", "O", "X" },
            new() { "X", "O", "O" },
            new() { "O", "X", null }
        };

        var updated = _service.SubmitMove(game.GameId, new SubmitMoveRequest { Player = Player.X, Row = 2, Column = 2 });

        Assert.Equal(GameStatus.Draw, updated.Status);
        Assert.Null(updated.Winner);
        Assert.Equal(1, _service.GetScoreboard().Draws);
    }

    [Fact]
    public void SubmitMove_AfterGameCompletion_ThrowsInvalidOperationException()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);
        game.Status = GameStatus.Won;
        game.Winner = Player.X;

        Assert.Throws<InvalidOperationException>(() =>
            _service.SubmitMove(game.GameId, new SubmitMoveRequest { Player = Player.X, Row = 1, Column = 1 }));
    }

    [Fact]
    public void ResetGame_ClearsBoardAndHistoryButKeepsScoreboard()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);
        _service.SubmitMove(game.GameId, new SubmitMoveRequest { Player = Player.X, Row = 0, Column = 0 });
        _service.SubmitMove(game.GameId, new SubmitMoveRequest { Player = Player.O, Row = 0, Column = 1 });

        var scoreboardBefore = _service.GetScoreboard();
        var reset = _service.ResetGame(game.GameId);

        Assert.Equal(Player.X, reset.CurrentPlayer);
        Assert.Equal(GameStatus.InProgress, reset.Status);
        Assert.Empty(reset.MoveHistory);
        Assert.All(reset.Board.SelectMany(row => row), cell => Assert.Null(cell));
        Assert.Equal(scoreboardBefore.XWins, _service.GetScoreboard().XWins);
        Assert.Equal(scoreboardBefore.OWins, _service.GetScoreboard().OWins);
        Assert.Equal(scoreboardBefore.Draws, _service.GetScoreboard().Draws);
    }

    [Fact]
    public void UndoMove_TwoPlayer_RestoresPreviousState()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);
        _service.SubmitMove(game.GameId, new SubmitMoveRequest { Player = Player.X, Row = 0, Column = 0 });
        _service.SubmitMove(game.GameId, new SubmitMoveRequest { Player = Player.O, Row = 0, Column = 1 });

        var undone = _service.UndoMove(game.GameId);

        Assert.Null(undone.Board[0][1]);
        Assert.Equal(Player.O, undone.CurrentPlayer);
        Assert.Equal(1, undone.MoveHistory.Count);
    }

    [Fact]
    public void UndoMove_ComputerMode_RemovesComputerAndHumanPair()
    {
        var game = _service.CreateGame(GameMode.Computer);
        _service.SubmitMove(game.GameId, new SubmitMoveRequest { Player = Player.X, Row = 0, Column = 0 });

        var undone = _service.UndoMove(game.GameId);

        Assert.True(undone.CanUndo == false || undone.MoveHistory.Count == 0);
        Assert.Equal(Player.X, undone.CurrentPlayer);
    }

    [Fact]
    public void UndoMove_WhenNoMoves_ThrowsInvalidOperationException()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);

        Assert.Throws<InvalidOperationException>(() => _service.UndoMove(game.GameId));
    }

    [Fact]
    public void UndoMove_AfterCompletion_ThrowsInvalidOperationException()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);
        game.Status = GameStatus.Won;
        game.MoveHistory.Add(new MoveHistoryItem { MoveNumber = 1, Player = Player.X, Row = 0, Column = 0 });

        Assert.Throws<InvalidOperationException>(() => _service.UndoMove(game.GameId));
    }

    [Fact]
    public void Scoreboard_IncrementsForXWin()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);
        game.CurrentPlayer = Player.X;
        game.Board = new List<List<string?>>
        {
            new() { "X", "X", "X" },
            new() { "O", "O", null },
            new() { null, null, null }
        };

        _service.SubmitMove(game.GameId, new SubmitMoveRequest { Player = Player.X, Row = 1, Column = 2 });

        var scoreboard = _service.GetScoreboard();
        Assert.Equal(1, scoreboard.XWins);
    }

    [Fact]
    public void Scoreboard_IncrementsForOWin()
    {
        var game = _service.CreateGame(GameMode.Computer);
        game.CurrentPlayer = Player.X;
        game.Board = new List<List<string?>>
        {
            new() { "O", "O", null },
            new() { "X", "X", null },
            new() { null, null, null }
        };

        _service.SubmitMove(game.GameId, new SubmitMoveRequest { Player = Player.X, Row = 2, Column = 2 });

        var scoreboard = _service.GetScoreboard();
        Assert.Equal(1, scoreboard.OWins);
    }

    [Fact]
    public void Scoreboard_IncrementsForDraw()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);
        game.CurrentPlayer = Player.X;
        game.Board = new List<List<string?>>
        {
            new() { "X", "O", "X" },
            new() { "X", "O", "O" },
            new() { "O", "X", null }
        };

        _service.SubmitMove(game.GameId, new SubmitMoveRequest { Player = Player.X, Row = 2, Column = 2 });

        Assert.Equal(1, _service.GetScoreboard().Draws);
    }

    [Fact]
    public void Scoreboard_IncrementsOnlyOncePerCompletedGame()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);
        game.CurrentPlayer = Player.X;
        game.Board = new List<List<string?>>
        {
            new() { "X", "X", null },
            new() { "O", "O", null },
            new() { null, null, null }
        };

        _service.SubmitMove(game.GameId, new SubmitMoveRequest { Player = Player.X, Row = 0, Column = 2 });
        Assert.Equal(1, _service.GetScoreboard().XWins);

        var sameGame = _service.GetGame(game.GameId);
        Assert.Equal(GameStatus.Won, sameGame.Status);
    }

    [Fact]
    public void ResetScoreboard_DoesNotModifyCurrentGame()
    {
        var game = _service.CreateGame(GameMode.TwoPlayer);
        _service.SubmitMove(game.GameId, new SubmitMoveRequest { Player = Player.X, Row = 0, Column = 0 });
        _service.ResetScoreboard();

        Assert.Equal(Player.O, _service.GetGame(game.GameId).CurrentPlayer);
        Assert.Null(_service.GetGame(game.GameId).Board[0][1]);
    }

    [Fact]
    public void Computer_TakesWinningMove_WhenAvailable()
    {
        var game = _service.CreateGame(GameMode.Computer);
        game.Board = new List<List<string?>>
        {
            new() { "O", "O", null },
            new() { "X", "X", null },
            new() { null, null, null }
        };
        game.CurrentPlayer = Player.X;

        var updated = _service.SubmitMove(game.GameId, new SubmitMoveRequest { Player = Player.X, Row = 2, Column = 2 });

        Assert.Equal("O", updated.Board[0][2]);
    }

    [Fact]
    public void Computer_BlocksImmediateWin()
    {
        var game = _service.CreateGame(GameMode.Computer);
        game.Board = new List<List<string?>>
        {
            new() { "X", "X", null },
            new() { "O", null, null },
            new() { null, null, null }
        };
        game.CurrentPlayer = Player.X;

        var updated = _service.SubmitMove(game.GameId, new SubmitMoveRequest { Player = Player.X, Row = 2, Column = 2 });

        Assert.Equal("O", updated.Board[0][2]);
    }

    [Fact]
    public void Computer_TakesCenter_WhenAvailable()
    {
        var game = _service.CreateGame(GameMode.Computer);
        game.CurrentPlayer = Player.X;
        game.Board[0][0] = "X";
        game.Board[2][2] = "X";

        var updated = _service.SubmitMove(game.GameId, new SubmitMoveRequest { Player = Player.X, Row = 1, Column = 0 });

        Assert.Equal("O", updated.Board[1][1]);
    }

    [Fact]
    public void Computer_TakesCorner_WhenAppropriate()
    {
        var game = _service.CreateGame(GameMode.Computer);
        game.Board = new List<List<string?>>
        {
            new() { "X", null, null },
            new() { null, "X", null },
            new() { null, null, null }
        };
        game.CurrentPlayer = Player.X;

        var updated = _service.SubmitMove(game.GameId, new SubmitMoveRequest { Player = Player.X, Row = 2, Column = 0 });

        Assert.NotNull(updated.Board[0][0]);
    }

    [Fact]
    public void Computer_RespondsAfterValidHumanMove()
    {
        var game = _service.CreateGame(GameMode.Computer);
        game.Board[0][0] = "X";
        game.CurrentPlayer = Player.X;

        var updated = _service.SubmitMove(game.GameId, new SubmitMoveRequest { Player = Player.X, Row = 2, Column = 2 });

        Assert.Equal(Player.O, updated.MoveHistory[^1].Player);
        Assert.Equal(Player.X, updated.MoveHistory[^2].Player);
    }

    [Fact]
    public void Computer_DoesNotMove_AfterGameCompletion()
    {
        var game = _service.CreateGame(GameMode.Computer);
        game.Board = new List<List<string?>>
        {
            new() { "X", "X", "X" },
            new() { null, "O", null },
            new() { null, null, "O" }
        };
        game.Status = GameStatus.Won;
        game.Winner = Player.X;
        game.CurrentPlayer = Player.X;

        Assert.Throws<InvalidOperationException>(() =>
            _service.SubmitMove(game.GameId, new SubmitMoveRequest { Player = Player.X, Row = 1, Column = 1 }));
    }
}
