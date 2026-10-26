import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import App from './App';

const buildGame = (overrides = {}) => ({
  gameId: 'game-123',
  board: [
    [null, null, null],
    [null, null, null],
    [null, null, null],
  ],
  currentPlayer: 'X',
  mode: 'TwoPlayer',
  status: 'InProgress',
  winner: null,
  winningCells: [],
  moveHistory: [],
  canUndo: true,
  ...overrides,
});

beforeEach(() => {
  global.fetch = jest.fn((url) => {
    if (url.includes('/scoreboard')) {
      return Promise.resolve({
        ok: true,
        text: async () => JSON.stringify({ xWins: 1, oWins: 2, draws: 3 }),
      });
    }

    if (url.includes('/games') && !url.includes('/moves') && !url.includes('/undo') && !url.includes('/reset')) {
      return Promise.resolve({
        ok: true,
        text: async () => JSON.stringify(buildGame()),
      });
    }

    if (url.includes('/moves')) {
      return Promise.resolve({
        ok: true,
        text: async () => JSON.stringify(buildGame({
          board: [
            ['X', null, null],
            [null, 'O', null],
            [null, null, null],
          ],
          currentPlayer: 'X',
          moveHistory: [
            { moveNumber: 1, player: 'X', row: 0, column: 0 },
            { moveNumber: 2, player: 'O', row: 1, column: 1 },
          ],
          canUndo: true,
        })),
      });
    }

    if (url.includes('/undo')) {
      return Promise.resolve({
        ok: true,
        text: async () => JSON.stringify(buildGame({
          board: [
            ['X', null, null],
            [null, null, null],
            [null, null, null],
          ],
          currentPlayer: 'X',
          moveHistory: [{ moveNumber: 1, player: 'X', row: 0, column: 0 }],
          canUndo: true,
        })),
      });
    }

    if (url.includes('/reset')) {
      return Promise.resolve({
        ok: true,
        text: async () => JSON.stringify(buildGame()),
      });
    }

    return Promise.resolve({ ok: true, text: async () => JSON.stringify({}) });
  });
});

afterEach(() => {
  jest.resetAllMocks();
});

test('renders the title and initial game state', async () => {
  render(<App />);

  expect(await screen.findByRole('heading', { name: /tic tac toe/i })).toBeInTheDocument();
  await waitFor(() => {
    expect(screen.getByText(/current turn: x/i)).toBeInTheDocument();
  });
});

test('submits a move and updates the board', async () => {
  render(<App />);

  await waitFor(() => {
    const boardCell = screen.getByRole('button', { name: /row 1, column 1, empty/i });
    expect(boardCell).not.toBeDisabled();
  });

  fireEvent.click(screen.getByRole('button', { name: /row 1, column 1, empty/i }));

  await waitFor(() => {
    expect(global.fetch).toHaveBeenCalledWith(
      'http://localhost:5158/api/games/game-123/moves',
      expect.objectContaining({ method: 'POST' })
    );
  });
});

test('supports undo and reset actions', async () => {
  render(<App />);

  await screen.findByText(/tic tac toe/i);

  const undoButton = screen.getByRole('button', { name: /undo last move/i });
  fireEvent.click(undoButton);

  await waitFor(() => {
    expect(global.fetch).toHaveBeenCalledWith(
      'http://localhost:5158/api/games/game-123/undo',
      expect.objectContaining({ method: 'POST' })
    );
  });

  const resetButton = screen.getByRole('button', { name: /reset game/i });
  fireEvent.click(resetButton);

  await waitFor(() => {
    expect(global.fetch).toHaveBeenCalledWith(
      'http://localhost:5158/api/games/game-123/reset',
      expect.objectContaining({ method: 'POST' })
    );
  });
});

test('displays scoreboard values from the backend', async () => {
  render(<App />);

  expect(await screen.findByText('1')).toBeInTheDocument();
  expect(screen.getByText('2')).toBeInTheDocument();
  expect(screen.getByText('3')).toBeInTheDocument();
});
