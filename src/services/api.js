const API_BASE_URL = 'http://localhost:5158/api';

async function request(path, options = {}) {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    headers: {
      'Content-Type': 'application/json',
      ...(options.headers || {}),
    },
    ...options,
  });

  const text = await response.text();
  const payload = text ? JSON.parse(text) : null;

  if (!response.ok) {
    throw new Error(payload?.message || 'Request failed');
  }

  return payload;
}

export const api = {
  createGame(mode) {
    return request('/games', {
      method: 'POST',
      body: JSON.stringify({ mode }),
    });
  },
  getGame(gameId) {
    return request(`/games/${gameId}`);
  },
  submitMove(gameId, player, row, column) {
    return request(`/games/${gameId}/moves`, {
      method: 'POST',
      body: JSON.stringify({ player, row, column }),
    });
  },
  undoGame(gameId) {
    return request(`/games/${gameId}/undo`, { method: 'POST' });
  },
  resetGame(gameId) {
    return request(`/games/${gameId}/reset`, { method: 'POST' });
  },
  getScoreboard() {
    return request('/scoreboard');
  },
  resetScoreboard() {
    return request('/scoreboard/reset', { method: 'POST' });
  },
};
