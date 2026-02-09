using System;
using System.Threading.Tasks;
using GomokuAI.Models;

namespace GomokuAI.Services
{
    /// <summary>
    /// Game state enumeration
    /// </summary>
    public enum GameState
    {
        NotStarted,
        Playing,
        BlackWin,
        WhiteWin,
        Draw
    }

    /// <summary>
    /// Game service - Manages game flow with undo/redo support
    /// </summary>
    public class GameService
    {
        private Board _board;
        private WinChecker _winChecker;
        private AIService _aiService;

        public Player HumanPlayer { get; private set; }
        public Player AIPlayer { get; private set; }
        public PlayerType CurrentPlayer { get; private set; }
        public GameState State { get; private set; }

        public event Action? OnBoardUpdated;
        public event Action<string>? OnGameMessage;
        public event Action<GameState>? OnGameStateChanged;

        public GameService()
        {
            _board = new Board();
            _winChecker = new WinChecker();
            _aiService = new AIService();
            HumanPlayer = Player.CreateHumanPlayer();
            AIPlayer = Player.CreateAIPlayer();
            State = GameState.NotStarted;
        }

        /// <summary>
        /// Start new game
        /// </summary>
        public void StartNewGame()
        {
            _board.Reset();
            CurrentPlayer = PlayerType.Black; // Black moves first
            State = GameState.Playing;
            OnGameStateChanged?.Invoke(State);
            OnBoardUpdated?.Invoke();
            OnGameMessage?.Invoke("Game started! Black (You) moves first.");
        }

        /// <summary>
        /// Player makes a move
        /// </summary>
        public async Task<bool> PlayerMove(int row, int col)
        {
            if (State != GameState.Playing)
            {
                OnGameMessage?.Invoke("Game not started or already ended!");
                return false;
            }

            if (CurrentPlayer != HumanPlayer.Type)
            {
                OnGameMessage?.Invoke("It's AI's turn, please wait...");
                return false;
            }

            var move = new Move(row, col, HumanPlayer.Type);
            if (!_board.PlaceMove(move))
            {
                OnGameMessage?.Invoke("Invalid position or already occupied!");
                return false;
            }

            OnBoardUpdated?.Invoke();

            // Check if player wins (check current position first, then full board scan)
            if (_winChecker.CheckWin(_board, row, col, HumanPlayer.Type))
            {
                State = GameState.BlackWin;
                OnGameStateChanged?.Invoke(State);
                OnGameMessage?.Invoke("🎉 Congratulations! You win!");
                return true;
            }

            // Full board scan to ensure no miss
            var winner = _winChecker.CheckBoardForWinner(_board);
            if (winner == HumanPlayer.Type)
            {
                State = GameState.BlackWin;
                OnGameStateChanged?.Invoke(State);
                OnGameMessage?.Invoke("🎉 Congratulations! You win!");
                return true;
            }
            else if (winner == AIPlayer.Type)
            {
                State = GameState.WhiteWin;
                OnGameStateChanged?.Invoke(State);
                OnGameMessage?.Invoke("💻 AI wins! Better luck next time!");
                return true;
            }

            // Check for draw
            if (_board.IsFull())
            {
                State = GameState.Draw;
                OnGameStateChanged?.Invoke(State);
                OnGameMessage?.Invoke("It's a draw!");
                return true;
            }

            // Switch to AI turn
            CurrentPlayer = AIPlayer.Type;
            OnGameMessage?.Invoke("AI is thinking...");

            // AI makes move (async to not block UI)
            await Task.Run(() => AIMove());

            return true;
        }

        /// <summary>
        /// AI makes a move
        /// </summary>
        private void AIMove()
        {
            if (State != GameState.Playing)
                return;

            // Check if player already won before AI moves
            var preCheckWinner = _winChecker.CheckBoardForWinner(_board);
            if (preCheckWinner == HumanPlayer.Type)
            {
                State = GameState.BlackWin;
                OnGameStateChanged?.Invoke(State);
                OnGameMessage?.Invoke("🎉 Congratulations! You win!");
                return;
            }

            var aiMove = _aiService.GetBestMove(_board, AIPlayer.Type);
            if (aiMove == null)
            {
                OnGameMessage?.Invoke("AI cannot find a valid move");
                return;
            }

            _board.PlaceMove(aiMove);
            OnBoardUpdated?.Invoke();

            // Check if AI wins (check current position first, then full board scan)
            if (_winChecker.CheckWin(_board, aiMove.Row, aiMove.Col, AIPlayer.Type))
            {
                State = GameState.WhiteWin;
                OnGameStateChanged?.Invoke(State);
                OnGameMessage?.Invoke("💻 AI wins! Better luck next time!");
                return;
            }

            // Full board scan to ensure no miss
            var winner = _winChecker.CheckBoardForWinner(_board);
            if (winner == AIPlayer.Type)
            {
                State = GameState.WhiteWin;
                OnGameStateChanged?.Invoke(State);
                OnGameMessage?.Invoke("💻 AI wins! Better luck next time!");
                return;
            }
            else if (winner == HumanPlayer.Type)
            {
                State = GameState.BlackWin;
                OnGameStateChanged?.Invoke(State);
                OnGameMessage?.Invoke("🎉 Congratulations! You win!");
                return;
            }

            // Check for draw
            if (_board.IsFull())
            {
                State = GameState.Draw;
                OnGameStateChanged?.Invoke(State);
                OnGameMessage?.Invoke("It's a draw!");
                return;
            }

            // Switch back to player turn
            CurrentPlayer = HumanPlayer.Type;
            OnGameMessage?.Invoke($"AI played ({aiMove.Row}, {aiMove.Col}). Your turn!");
        }

        /// <summary>
        /// Undo last move pair (reverts both player and AI moves)
        /// Uses Stack-based state management for clean LIFO operations
        /// </summary>
        public bool Undo()
        {
            if (State != GameState.Playing)
            {
                OnGameMessage?.Invoke("Game is not in progress!");
                return false;
            }

            if (!_board.CanUndo || _board.UndoCount < 2)
            {
                OnGameMessage?.Invoke("Not enough moves to undo!");
                return false;
            }

            // Undo AI's move first (last move)
            var aiUndone = _board.Undo();
            // Undo player's move
            var playerUndone = _board.Undo();

            CurrentPlayer = HumanPlayer.Type;
            OnBoardUpdated?.Invoke();
            
            if (playerUndone != null && aiUndone != null)
            {
                OnGameMessage?.Invoke($"Undone: Your ({playerUndone.Row},{playerUndone.Col}) & AI ({aiUndone.Row},{aiUndone.Col}). Your turn.");
            }
            else
            {
                OnGameMessage?.Invoke("Undo complete. Your turn.");
            }
            
            return true;
        }

        /// <summary>
        /// Undo single move (for more granular control)
        /// </summary>
        public bool UndoSingle()
        {
            if (State != GameState.Playing)
            {
                OnGameMessage?.Invoke("Game is not in progress!");
                return false;
            }

            if (!_board.CanUndo)
            {
                OnGameMessage?.Invoke("No moves to undo!");
                return false;
            }

            var undone = _board.Undo();
            
            // Switch current player
            CurrentPlayer = CurrentPlayer == PlayerType.Black ? PlayerType.White : PlayerType.Black;
            
            OnBoardUpdated?.Invoke();
            
            if (undone != null)
            {
                string player = undone.Player == PlayerType.Black ? "Black" : "White";
                OnGameMessage?.Invoke($"Undone: {player} ({undone.Row},{undone.Col})");
            }
            
            return true;
        }

        /// <summary>
        /// Redo last undone move pair (replays both moves)
        /// </summary>
        public bool Redo()
        {
            if (State != GameState.Playing)
            {
                OnGameMessage?.Invoke("Game is not in progress!");
                return false;
            }

            if (!_board.CanRedo || _board.RedoCount < 2)
            {
                OnGameMessage?.Invoke("No moves to redo!");
                return false;
            }

            // Redo player's move first
            var playerRedone = _board.Redo();
            // Redo AI's move
            var aiRedone = _board.Redo();

            CurrentPlayer = HumanPlayer.Type;
            OnBoardUpdated?.Invoke();
            
            if (playerRedone != null && aiRedone != null)
            {
                OnGameMessage?.Invoke($"Redone: Your ({playerRedone.Row},{playerRedone.Col}) & AI ({aiRedone.Row},{aiRedone.Col}). Your turn.");
            }
            else
            {
                OnGameMessage?.Invoke("Redo complete. Your turn.");
            }
            
            return true;
        }

        /// <summary>
        /// Check if undo is available
        /// </summary>
        public bool CanUndo => State == GameState.Playing && _board.UndoCount >= 2;

        /// <summary>
        /// Check if redo is available
        /// </summary>
        public bool CanRedo => State == GameState.Playing && _board.RedoCount >= 2;

        /// <summary>
        /// Get the board
        /// </summary>
        public Board GetBoard()
        {
            return _board;
        }

        /// <summary>
        /// Get piece at specified position
        /// </summary>
        public PlayerType GetCell(int row, int col)
        {
            return _board.GetCell(row, col);
        }

        /// <summary>
        /// Get move history
        /// </summary>
        public System.Collections.Generic.List<Move> GetMoveHistory()
        {
            return _board.GetMoveHistory();
        }

        /// <summary>
        /// Get the last move
        /// </summary>
        public Move? GetLastMove()
        {
            return _board.GetLastMove();
        }
    }
}
