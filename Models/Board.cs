using System;
using System.Collections.Generic;

namespace GomokuAI.Models
{
    /// <summary>
    /// Board class - 15x15 Gomoku board with undo/redo support
    /// </summary>
    public class Board
    {
        public const int Size = 15;
        private int[,] _grid;
        
        // Use Stack for LIFO operations - cleaner for undo/redo
        private Stack<Move> _undoStack;
        private Stack<Move> _redoStack;

        public Board()
        {
            _grid = new int[Size, Size];
            _undoStack = new Stack<Move>();
            _redoStack = new Stack<Move>();
            Reset();
        }

        /// <summary>
        /// Reset the board
        /// </summary>
        public void Reset()
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    _grid[i, j] = (int)PlayerType.None;
                }
            }
            _undoStack.Clear();
            _redoStack.Clear();
        }

        /// <summary>
        /// Get the piece at specified position
        /// </summary>
        public PlayerType GetCell(int row, int col)
        {
            if (!IsValidPosition(row, col))
                return PlayerType.None;
            return (PlayerType)_grid[row, col];
        }

        /// <summary>
        /// Place a move
        /// </summary>
        public bool PlaceMove(Move move)
        {
            if (!IsValidPosition(move.Row, move.Col))
                return false;

            if (_grid[move.Row, move.Col] != (int)PlayerType.None)
                return false;

            _grid[move.Row, move.Col] = (int)move.Player;
            move.MoveNumber = _undoStack.Count + 1;
            _undoStack.Push(move);
            
            // Clear redo stack when new move is made
            _redoStack.Clear();
            
            return true;
        }

        /// <summary>
        /// Undo - Reverts the last move (returns the undone move)
        /// </summary>
        public Move? Undo()
        {
            if (_undoStack.Count == 0)
                return null;

            var move = _undoStack.Pop();
            _grid[move.Row, move.Col] = (int)PlayerType.None;
            _redoStack.Push(move);
            
            return move;
        }

        /// <summary>
        /// Redo - Replays the last undone move (returns the redone move)
        /// </summary>
        public Move? Redo()
        {
            if (_redoStack.Count == 0)
                return null;

            var move = _redoStack.Pop();
            _grid[move.Row, move.Col] = (int)move.Player;
            _undoStack.Push(move);
            
            return move;
        }

        /// <summary>
        /// Check if undo is available
        /// </summary>
        public bool CanUndo => _undoStack.Count > 0;

        /// <summary>
        /// Check if redo is available
        /// </summary>
        public bool CanRedo => _redoStack.Count > 0;

        /// <summary>
        /// Check if position is valid
        /// </summary>
        public bool IsValidPosition(int row, int col)
        {
            return row >= 0 && row < Size && col >= 0 && col < Size;
        }

        /// <summary>
        /// Check if position is empty
        /// </summary>
        public bool IsEmpty(int row, int col)
        {
            return IsValidPosition(row, col) && _grid[row, col] == (int)PlayerType.None;
        }

        /// <summary>
        /// Get move history as list (for display)
        /// </summary>
        public List<Move> GetMoveHistory()
        {
            var moves = new List<Move>(_undoStack);
            moves.Reverse(); // Stack is LIFO, reverse to get chronological order
            return moves;
        }

        /// <summary>
        /// Get the last move
        /// </summary>
        public Move? GetLastMove()
        {
            if (_undoStack.Count == 0)
                return null;
            return _undoStack.Peek();
        }

        /// <summary>
        /// Get a copy of the board grid (for AI calculation)
        /// </summary>
        public int[,] GetGridCopy()
        {
            int[,] copy = new int[Size, Size];
            Array.Copy(_grid, copy, _grid.Length);
            return copy;
        }

        /// <summary>
        /// Set board state (for AI calculation)
        /// </summary>
        public void SetGrid(int[,] grid)
        {
            Array.Copy(grid, _grid, grid.Length);
        }

        /// <summary>
        /// Check if board is full
        /// </summary>
        public bool IsFull()
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (_grid[i, j] == (int)PlayerType.None)
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Get all empty positions
        /// </summary>
        public List<(int row, int col)> GetEmptyPositions()
        {
            var positions = new List<(int, int)>();
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (_grid[i, j] == (int)PlayerType.None)
                        positions.Add((i, j));
                }
            }
            return positions;
        }

        /// <summary>
        /// Get empty positions near existing pieces (for AI pruning)
        /// </summary>
        public List<(int row, int col)> GetNeighborEmptyPositions(int radius = 2)
        {
            var positions = new HashSet<(int, int)>();

            foreach (var move in _undoStack)
            {
                for (int dr = -radius; dr <= radius; dr++)
                {
                    for (int dc = -radius; dc <= radius; dc++)
                    {
                        int newRow = move.Row + dr;
                        int newCol = move.Col + dc;
                        if (IsEmpty(newRow, newCol))
                        {
                            positions.Add((newRow, newCol));
                        }
                    }
                }
            }

            // If board is empty, return center position
            if (positions.Count == 0)
            {
                positions.Add((Size / 2, Size / 2));
            }

            return new List<(int, int)>(positions);
        }

        /// <summary>
        /// Get total move count
        /// </summary>
        public int MoveCount => _undoStack.Count;

        /// <summary>
        /// Get undo stack count
        /// </summary>
        public int UndoCount => _undoStack.Count;

        /// <summary>
        /// Get redo stack count
        /// </summary>
        public int RedoCount => _redoStack.Count;
    }
}
