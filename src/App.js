import { useEffect, useMemo, useState } from 'react';
import './App.css';
import { api } from './services/api';

const emptyBoard = () => Array.from({ length: 3 }, () => Array(3).fill(null));

function App() {
  const [game, setGame] = useState(null);
  const [scoreboard, setScoreboard] = useState({ xWins: 0, oWins: 0, draws: 0 });
  const [selectedMode, setSelectedMode] = useState('TwoPlayer');
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState('');
  const [pendingAction, setPendingAction] = useState('');

  const loadScoreboard = async () => {
    try {
      const result = await api.getScoreboard();
      setScoreboard({
        xWins: result.xWins ?? result.XWins ?? 0,
        oWins: result.oWins ?? result.OWins ?? 0,
        draws: result.draws ?? result.Draws ?? 0,
      });
    } catch (err) {
      setErrorMessage(err.message || 'Unable to load the scoreboard.');
    }
  };

  const refreshGame = async (mode = selectedMode) => {
    setIsLoading(true);
    setErrorMessage('');

    try {
      const createdGame = await api.createGame(mode);
      setGame(createdGame);
      await loadScoreboard();
    } catch (err) {
      setErrorMessage(err.message || 'Unable to create a game session.');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    refreshGame(selectedMode);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const statusMessage = useMemo(() => {
    if (!game) {
      return 'Loading...';
    }

    if (game.status === 'Won') {
      return `${game.winner ?? 'Winner'} wins!`;
    }

    if (game.status === 'Draw') {
      return 'It\'s a draw!';
    }

    return `Current turn: ${game.currentPlayer}`;
  }, [game]);

  const handleModeChange = async (event) => {
    const nextMode = event.target.value;
    setSelectedMode(nextMode);
    await refreshGame(nextMode);
  };

  const handleCellClick = async (row, column) => {
    if (!game || game.status !== 'InProgress' || pendingAction || game.board[row][column]) {
      return;
    }

    setPendingAction('move');
    setErrorMessage('');

    try {
      const nextGame = await api.submitMove(game.gameId, game.currentPlayer, row, column);
      setGame(nextGame);
      await loadScoreboard();
    } catch (err) {
      setErrorMessage(err.message || 'Move could not be submitted.');
    } finally {
      setPendingAction('');
    }
  };

  const handleUndo = async () => {
    if (!game || pendingAction || !game.canUndo) {
      return;
    }

    setPendingAction('undo');
    setErrorMessage('');

    try {
      const nextGame = await api.undoGame(game.gameId);
      setGame(nextGame);
    } catch (err) {
      setErrorMessage(err.message || 'Undo is unavailable for this game state.');
    } finally {
      setPendingAction('');
    }
  };

  const handleResetGame = async () => {
    if (!game || pendingAction) {
      return;
    }

    setPendingAction('reset-game');
    setErrorMessage('');

    try {
      const nextGame = await api.resetGame(game.gameId);
      setGame(nextGame);
    } catch (err) {
      setErrorMessage(err.message || 'The game could not be reset.');
    } finally {
      setPendingAction('');
    }
  };

  const handleResetScoreboard = async () => {
    setPendingAction('reset-scoreboard');
    setErrorMessage('');

    try {
      await api.resetScoreboard();
      await loadScoreboard();
    } catch (err) {
      setErrorMessage(err.message || 'The scoreboard could not be reset.');
    } finally {
      setPendingAction('');
    }
  };

  const boardCells = game?.board || emptyBoard();
  const winningCells = new Set(game?.winningCells || []);

  return (
    <div className="app-shell">
      <header className="app-header">
        <div>
          <p className="eyebrow">Assessment project</p>
          <h1>Tic Tac Toe</h1>
        </div>

        <div className="mode-picker">
          <label htmlFor="mode-select">Game mode</label>
          <select id="mode-select" value={selectedMode} onChange={handleModeChange} disabled={isLoading || !!pendingAction}>
            <option value="TwoPlayer">Two Player</option>
            <option value="Computer">Play Against Computer</option>
          </select>
        </div>
      </header>

      <main className="app-layout">
        <section className="panel game-panel">
          <div className="status-row">
            <div>
              <span className="label">Current player</span>
              <strong>{game ? game.currentPlayer : '-'}</strong>
            </div>
            <div>
              <span className="label">Status</span>
              <strong>{game ? game.status : 'Starting'}</strong>
            </div>
          </div>

          <div className="status-banner" data-status={game?.status || 'Loading'}>
            {statusMessage}
          </div>

          {errorMessage && <div className="error-box">{errorMessage}</div>}

          <div className="board" role="grid" aria-label="Tic Tac Toe board">
            {boardCells.map((row, rowIndex) =>
              row.map((cell, columnIndex) => {
                const index = rowIndex * 3 + columnIndex;
                const isWinningCell = winningCells.has(index);
                const isDisabled = !game || game.status !== 'InProgress' || !!pendingAction || !!cell;

                return (
                  <button
                    key={`${rowIndex}-${columnIndex}`}
                    type="button"
                    className={`cell ${cell ? 'filled' : ''} ${isWinningCell ? 'winning' : ''} ${cell === 'X' ? 'x-mark' : cell === 'O' ? 'o-mark' : ''}`}
                    onClick={() => handleCellClick(rowIndex, columnIndex)}
                    disabled={isDisabled}
                    aria-label={`Row ${rowIndex + 1}, column ${columnIndex + 1}${cell ? `, occupied by ${cell}` : ', empty'}`}
                  >
                    {cell || ''}
                  </button>
                );
              })
            )}
          </div>

          <div className="controls">
            <button type="button" onClick={handleResetGame} disabled={!game || !!pendingAction}>
              Reset Game
            </button>
            <button type="button" onClick={handleUndo} disabled={!game || !game.canUndo || !!pendingAction}>
              Undo Last Move
            </button>
          </div>
        </section>

        <aside className="panel sidebar">
          <div className="stack">
            <h2>Scoreboard</h2>
            <div className="scoreboard">
              <div><span>X Wins</span><strong>{scoreboard.xWins}</strong></div>
              <div><span>O Wins</span><strong>{scoreboard.oWins}</strong></div>
              <div><span>Draws</span><strong>{scoreboard.draws}</strong></div>
            </div>
            <button type="button" className="secondary" onClick={handleResetScoreboard} disabled={!!pendingAction}>
              Reset
            </button>
          </div>

          <div className="stack">
            <h2>Move History</h2>
            <div className="history-table-wrap">
              <table>
                <thead>
                  <tr>
                    <th>#</th>
                    <th>Player</th>
                    <th>Row</th>
                    <th>Column</th>
                  </tr>
                </thead>
                <tbody>
                  {(game?.moveHistory || []).map((move) => (
                    <tr key={`${move.moveNumber}-${move.player}-${move.row}-${move.column}`}>
                      <td>{move.moveNumber}</td>
                      <td>{move.player}</td>
                      <td>{move.row + 1}</td>
                      <td>{move.column + 1}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </aside>
      </main>
      <footer className="app-footer"></footer>
    </div>
  );
}

export default App;
