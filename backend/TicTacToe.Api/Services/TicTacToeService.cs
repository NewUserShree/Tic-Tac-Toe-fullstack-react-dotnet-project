using TicTacToe.Api.DTOs;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public class TicTacToeService : ITicTacToeService
{
    private readonly Dictionary<string, GameSession> _games = new(StringComparer.Ordinal);
    private readonly Scoreboard _scoreboard = new();

    public GameSession CreateGame(GameMode mode)
    {
        var game = new GameSession
        {
            GameId = Guid.NewGuid().ToString("N"),
            Mode = mode,
            Board = GameSession.CreateEmptyBoard(),
            CurrentPlayer = Player.X,
            Status = GameStatus.InProgress,
            Winner = null,
            WinningCells = new List<int>(),
            MoveHistory = new List<MoveHistoryItem>()
        };

        _games[game.GameId] = game;
        return game;
    }

    public GameSession GetGame(string id)
    {
        if (!_games.TryGetValue(id, out var game))
        {
            throw new KeyNotFoundException("Game not found.");
        }

        return game;
    }

    public GameSession SubmitMove(string id, SubmitMoveRequest request)
    {
        var game = GetGame(id);

        if (game.Status != GameStatus.InProgress)
        {
            throw new InvalidOperationException("This game is already complete.");
        }

        if (request.Row is < 0 or > 2 || request.Column is < 0 or > 2)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Row and column values must be between 0 and 2.");
        }

        if (request.Player != game.CurrentPlayer)
        {
            throw new InvalidOperationException($"It is {game.CurrentPlayer}'s turn.");
        }

        if (game.Board[request.Row][request.Column] != null)
        {
            throw new InvalidOperationException("That cell is already occupied.");
        }

        ApplyMove(game, request.Row, request.Column, request.Player);

        if (game.Status == GameStatus.InProgress && game.Mode == GameMode.Computer && request.Player == Player.X)
        {
            var computerMove = GetComputerMove(game);
            if (computerMove.HasValue)
            {
                ApplyMove(game, computerMove.Value.Row, computerMove.Value.Column, Player.O);
            }
        }

        return game;
    }

    public GameSession UndoMove(string id)
    {
        var game = GetGame(id);

        if (game.MoveHistory.Count == 0)
        {
            throw new InvalidOperationException("There are no moves to undo.");
        }

        if (game.Status != GameStatus.InProgress)
        {
            throw new InvalidOperationException("Completed games cannot be undone.");
        }

        if (game.Mode == GameMode.TwoPlayer)
        {
            RemoveLastMove(game);
            return game;
        }

        if (game.MoveHistory.Count >= 2)
        {
            RemoveLastMove(game);
            RemoveLastMove(game);
            game.CurrentPlayer = Player.X;
            return game;
        }

        RemoveLastMove(game);
        game.CurrentPlayer = Player.X;
        return game;
    }

    public GameSession ResetGame(string id)
    {
        var game = GetGame(id);
        game.Board = GameSession.CreateEmptyBoard();
        game.CurrentPlayer = Player.X;
        game.Status = GameStatus.InProgress;
        game.Winner = null;
        game.WinningCells = new List<int>();
        game.MoveHistory = new List<MoveHistoryItem>();
        game.ScoreRecorded = false;
        return game;
    }

    public Scoreboard GetScoreboard()
    {
        return _scoreboard;
    }

    public Scoreboard ResetScoreboard()
    {
        _scoreboard.XWins = 0;
        _scoreboard.OWins = 0;
        _scoreboard.Draws = 0;
        return _scoreboard;
    }

    private void ApplyMove(GameSession game, int row, int column, Player player)
    {
        game.Board[row][column] = player.ToString();
        game.MoveHistory.Add(new MoveHistoryItem
        {
            MoveNumber = game.MoveHistory.Count + 1,
            Player = player,
            Row = row,
            Column = column
        });

        if (TryUpdateGameOutcome(game, player))
        {
            return;
        }

        game.CurrentPlayer = player == Player.X ? Player.O : Player.X;
    }

    private bool TryUpdateGameOutcome(GameSession game, Player player)
    {
        if (TryFindWinner(game.Board, out var winningCells))
        {
            game.Status = GameStatus.Won;
            game.Winner = player;
            game.WinningCells = winningCells;
            if (!game.ScoreRecorded)
            {
                if (player == Player.X)
                {
                    _scoreboard.XWins++;
                }
                else
                {
                    _scoreboard.OWins++;
                }

                game.ScoreRecorded = true;
            }

            return true;
        }

        if (IsBoardFull(game.Board))
        {
            game.Status = GameStatus.Draw;
            game.Winner = null;
            game.WinningCells = new List<int>();
            if (!game.ScoreRecorded)
            {
                _scoreboard.Draws++;
                game.ScoreRecorded = true;
            }

            return true;
        }

        return false;
    }

    private void RemoveLastMove(GameSession game)
    {
        var lastMove = game.MoveHistory[^1];
        game.Board[lastMove.Row][lastMove.Column] = null;
        game.MoveHistory.RemoveAt(game.MoveHistory.Count - 1);
        game.Status = GameStatus.InProgress;
        game.Winner = null;
        game.WinningCells = new List<int>();
        game.CurrentPlayer = lastMove.Player;
    }

    private static bool IsBoardFull(List<List<string?>> board)
    {
        foreach (var row in board)
        {
            foreach (var cell in row)
            {
                if (cell is null)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool TryFindWinner(List<List<string?>> board, out List<int> winningCells)
    {
        var winningCombinations = new[]
        {
            new[] { 0, 1, 2 },
            new[] { 3, 4, 5 },
            new[] { 6, 7, 8 },
            new[] { 0, 3, 6 },
            new[] { 1, 4, 7 },
            new[] { 2, 5, 8 },
            new[] { 0, 4, 8 },
            new[] { 2, 4, 6 }
        };

        foreach (var combination in winningCombinations)
        {
            var firstIndex = combination[0];
            var firstRow = firstIndex / 3;
            var firstColumn = firstIndex % 3;
            var firstValue = board[firstRow][firstColumn];

            if (firstValue is null)
            {
                continue;
            }

            var match = true;
            foreach (var cellIndex in combination.Skip(1))
            {
                var row = cellIndex / 3;
                var column = cellIndex % 3;
                if (board[row][column] != firstValue)
                {
                    match = false;
                    break;
                }
            }

            if (match)
            {
                winningCells = combination.ToList();
                return true;
            }
        }

        winningCells = new List<int>();
        return false;
    }

    private static (int Row, int Column)? GetComputerMove(GameSession game)
    {
        var emptyCell = FindImmediateWinningMove(game.Board, Player.O);
        if (emptyCell.HasValue)
        {
            return emptyCell.Value;
        }

        emptyCell = FindImmediateWinningMove(game.Board, Player.X);
        if (emptyCell.HasValue)
        {
            return emptyCell.Value;
        }

        var center = (Row: 1, Column: 1);
        if (game.Board[1][1] is null)
        {
            return center;
        }

        var corners = new[] { (Row: 0, Column: 0), (Row: 0, Column: 2), (Row: 2, Column: 0), (Row: 2, Column: 2) };
        foreach (var corner in corners)
        {
            if (game.Board[corner.Row][corner.Column] is null)
            {
                return corner;
            }
        }

        for (var row = 0; row < 3; row++)
        {
            for (var column = 0; column < 3; column++)
            {
                if (game.Board[row][column] is null)
                {
                    return (row, column);
                }
            }
        }

        return null;
    }

    private static (int Row, int Column)? FindImmediateWinningMove(List<List<string?>> board, Player player)
    {
        for (var row = 0; row < 3; row++)
        {
            for (var column = 0; column < 3; column++)
            {
                if (board[row][column] is not null)
                {
                    continue;
                }

                var candidateBoard = CloneBoard(board);
                candidateBoard[row][column] = player.ToString();
                if (TryFindWinner(candidateBoard, out _))
                {
                    return (row, column);
                }
            }
        }

        return null;
    }

    private static List<List<string?>> CloneBoard(List<List<string?>> board)
    {
        return board.Select(row => row.Select(cell => cell).ToList()).ToList();
    }
}
