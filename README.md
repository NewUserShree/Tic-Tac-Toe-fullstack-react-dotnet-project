# Tic Tac Toe Assessment

This project is a full-stack Tic Tac Toe application built with React.js on the frontend and ASP.NET Core Web API on the backend. The backend owns the game rules, move validation, score tracking, and the computer strategy. The frontend consumes the API and renders the game state without directly managing authoritative rules.

## Tech stack

- Frontend: React.js, JavaScript, CSS
- Backend: ASP.NET Core Web API (.NET 8), C#, Swagger/OpenAPI
- State management: backend in-memory session store with a session-level scoreboard
- Communication: REST API over HTTP on localhost

## Features implemented

- Two Player and Play Against Computer modes.
- 3 × 3 board with turn validation, win/draw detection, and highlighted winning cells.
- Computer plays O using deterministic win/block/center/corner/first-open priorities.
- Move history with one-based row and column positions.
- Undo: one move in Two Player mode; the most recent human/computer pair in Computer mode. Undo is disabled after a result.
- Reset Game preserves the scoreboard. Reset Scoreboard asks for confirmation.


## Local setup

Prerequisites:

- Node.js 18+
- npm 9+
- .NET 8 SDK

 ## How to run the backend locally
Start the backend:

```bash
cd backend/TicTacToe.Api
"$HOME/.dotnet/dotnet" run
```

On Windows PowerShell:

```powershell
cd "C:\Users\user\OneDrive\Desktop\ABB\abbassignment\backend\TicTacToe.Api"
& "$env:USERPROFILE\.dotnet\dotnet" run
```

The API listens on:

- http://localhost:5158
- Swagger UI: http://localhost:5158/swagger


## How to run the frontend locally
Start the frontend:

```bash
cd ..
cd ..
npm install
npm start
```

The frontend is served at:

- http://localhost:3000

## API endpoint summary
 Base URL: http://localhost:5158/api

The backend must be running for these endpoints to respond. Requests use JSON where a body is shown. Player, mode, and status values are serialized as strings.

Method	Endpoint	Purpose
POST	/games	Create a new game
GET	/games/{id}	Retrieve a game by ID
POST	/games/{id}/moves	Submit a move
POST	/games/{id}/undo	Undo the most recent move or turn
POST	/games/{id}/reset	Reset a game without changing the scoreboard
GET	/scoreboard	Get the scoreboard
POST	/scoreboard/reset	Reset the scoreboard


### Create a game

```http
POST /api/games
Content-Type: application/json

{
  "mode": "TwoPlayer"
}
```

Response:

```json
{
  "gameId": "e1d5...",
  "board": [[null, null, null], [null, null, null], [null, null, null]],
  "currentPlayer": "X",
  "mode": "TwoPlayer",
  "status": "InProgress",
  "winner": null,
  "winningCells": [],
  "moveHistory": [],
  "canUndo": false
}
```

### Submit a move

Row and column indexes are zero-based.

```http
POST /api/games/{id}/moves
Content-Type: application/json

{
  "player": "X",
  "row": 0,
  "column": 0
}
```

### Undo last move

```http
POST /api/games/{id}/undo
```

### Reset game

```http
POST /api/games/{id}/reset
```

### Scoreboard

```http
GET /api/scoreboard
POST /api/scoreboard/reset
```

## Game rules

- Players alternate turns starting with X.
- A valid move requires an empty cell and the correct player turn.
- Win status is detected for all 8 winning combinations.
- Draw occurs when all cells are occupied without a winner.
- In computer mode, the human is X and the computer is O.
- Computer logic priority is:
  1. Take a winning move
  2. Block an immediate X win
  3. Take center if available
  4. Take a corner if available
  5. Take any empty cell

## Undo policy

This app follows Option A: undo is disabled after a game has been completed. The API rejects undo requests for a finished game with a conflict response, and the frontend surfaces that as user feedback. This keeps the final scoreboard result stable and prevents modifying an already-finished game result.

In two-player mode, undo removes the last valid move.

In computer mode, undo removes the latest human/computer pair and returns play to X.

If there are no moves, or the game is already complete, the backend rejects the undo request with an appropriate HTTP error and message.

## Scoreboard policy

- The scoreboard is session-level and in-memory only.
- Data is lost when the backend process restarts.
- Scoreboard updates occur once per completed game.
- Resetting a game does not change the scoreboard.
- Resetting the scoreboard does not change the current game.

## How to run tests

Backend:

```bash
cd backend
"$HOME/.dotnet/dotnet" test TicTacToe.Tests/TicTacToe.Tests.csproj
```

Frontend:

```bash
cd ..
CI=true npm test -- --watch=false --runInBand
```

## Troubleshooting

- If the frontend cannot connect to the backend, confirm the API is running on http://localhost:5158 and that no other process is using that port.
- If CORS errors appear, verify that the backend includes the React development origin http://localhost:3000.
- If the .NET SDK is not found, install .NET 8 SDK and reopen the terminal.
- If `npm start` fails, delete the local `node_modules` folder and rerun `npm install`.

## AI tools and prompt summary
Tool	Used 
[ GitHub Copilot]	Planning the architecture, generating initial code, writing tests.

AI was used as an accelerator. I made the design decisions, reviewed all generated code, and ran the tests.

Prompt Summary

I gave the AI a single detailed prompt asking for a full-stack Tic Tac Toe application. It covered:

Architecture: React.js frontend talking to an ASP.NET Core Web API over REST.
Game rules: Turn management, move validation, win detection, draw detection, and game completion.
Computer opponent: A deterministic, priority-based strategy implemented in the backend (win, then block, then center, then corner, then side).
Undo and reset: Mode-specific undo, game reset, and an independent scoreboard reset.
Scoreboard: Backend-managed X wins, O wins, and draws, with protection against duplicate updates when a game completes.
Testing: xUnit tests for the backend and [Jest/Vitest + React Testing Library] for the frontend.
Documentation: Setup steps, API examples, design decisions, assumptions, limitations, and future improvements.
AI-Assisted Workflow
Inspected the existing project structure before making changes.
Defined the models, API contract, and game-state rules.
Implemented backend services and REST endpoints.
Added backend unit tests.
Built the React API service and UI components.
Integrated the frontend with the backend.
Added frontend tests and verified the main game flows.
Documented setup, API usage, and technical decisions.
Reviewed the result against the requirements checklist.
Review and Validation

I reviewed all AI-generated code against the requirements rather than accepting it as-is.  


##  Design Decisions

- **Backend-authoritative game logic:** The ASP.NET Core Web API owns game state, move validation, turn management, win/draw detection, undo behavior, and computer moves. This prevents the frontend from being the source of truth for game rules.
- **React frontend:** React is responsible for rendering the board, displaying the current game status and scoreboard, accepting user interactions, and presenting API errors.
- **REST API communication:** The frontend and backend communicate through HTTP endpoints, keeping the UI and business logic separate and making the API independently testable.
- **In-memory storage:** Game sessions and scoreboard data are stored in memory to keep the assessment implementation simple and avoid requiring a database setup.
- **Deterministic computer strategy:** The computer follows a fixed priority: win if possible, block an immediate opponent win, take the center, take an available corner, and otherwise select an available cell. This makes its behavior predictable and easy to test.
- **Undo policy:** Undo is disabled after a game is completed. In two-player mode, it removes the most recent move; in computer mode, it removes the latest human/computer move pair. The backend enforces this policy.
- **Scoreboard consistency:** The backend updates the scoreboard once when a game reaches a win or draw. Resetting a game does not modify the scoreboard, and resetting the scoreboard does not alter an active game.
- **HTTP status codes:** The API uses appropriate HTTP responses to communicate successful operations, invalid requests, missing games, and operations that conflict with the current game state.

## 10. Clarifications and Assumptions

- The application supports two modes: two-player mode and play-against-computer mode.
- X always makes the first move. In computer mode, the human plays as X and the computer plays as O.
- Board row and column indexes are zero-based, ranging from 0 to 2.
- Only valid moves are accepted. A move must target an empty cell and must belong to the player whose turn it is.
- A game ends immediately when a player completes a winning combination or when the board is full without a winner.
- The computer uses the specified priority-based strategy rather than advanced algorithms such as minimax.
- Undo is permitted only while a game is in progress. A completed game cannot be modified through undo.
- The scoreboard is maintained for the lifetime of the running backend process and is shared by games handled by that process.
- Game sessions and scores are not persisted to a database. Restarting the backend clears the in-memory data.
- The frontend is expected to run at `http://localhost:3000` and the backend at `http://localhost:5158` during local development.

##  Known Limitations

- **No persistent storage:** Game sessions and scoreboard results are lost when the backend restarts.
- **Limited computer intelligence:** The computer uses a simple rule-based strategy and does not guarantee optimal play in every situation.
- **No user accounts:** The application does not provide authentication, user profiles, or personalized score histories.
- **Local development configuration:** The default URLs and CORS settings are intended for local development and may need changes for deployment.


##  Future Improvements

- Add database persistence using SQL Server or PostgreSQL to retain game sessions and scoreboard history across restarts.
- Add authentication, player profiles, and individual statistics.
- Introduce real-time multiplayer using WebSockets.



