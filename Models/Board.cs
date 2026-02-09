using System;
using System.Collections.Generic;

namespace GomokuAI.Models
{
    /// <summary>
    /// Board class - 15x15 Gomoku board
    /// </summary>
    public class Board
    {
        public const int Size = 15;
        private int[,] _grid;
        private List<Move> _moveHistory;

        public Board()
        {
            _grid = new int[Size, Size];
            _moveHistory = new List<Move>();
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
            _moveHistory.Clear();
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
            move.MoveNumber = _moveHistory.Count + 1;
            _moveHistory.Add(move);
            return true;
        }

        /// <summary>
        /// Undo - Reverts the last move
        /// </summary>
        public Move? UndoLastMove()
        {
            if (_moveHistory.Count == 0)
                return null;

            var lastMove = _moveHistory[_moveHistory.Count - 1];
            _grid[lastMove.Row, lastMove.Col] = (int)PlayerType.None;
            _moveHistory.RemoveAt(_moveHistory.Count - 1);
            return lastMove;
        }

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
        /// Get move history
        /// </summary>
        public List<Move> GetMoveHistory()
        {
            return new List<Move>(_moveHistory);
        }

        /// <summary>
        /// Get the last move
        /// </summary>
        public Move? GetLastMove()
        {
            if (_moveHistory.Count == 0)
                return null;
            return _moveHistory[_moveHistory.Count - 1];
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

            foreach (var move in _moveHistory)
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
        public int MoveCount => _moveHistory.Count;
    }
}
