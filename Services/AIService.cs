using System;
using System.Collections.Generic;
using GomokuAI.Models;

namespace GomokuAI.Services
{
    /// <summary>
    /// AI Service - Uses Minimax algorithm with Alpha-Beta pruning
    /// </summary>
    public class AIService
    {
        private readonly WinChecker _winChecker;
        private const int MaxDepth = 3;
        private const int BoardSize = Board.Size;

        // Score constants
        private const int ScoreWin = 100000;
        private const int ScoreFour = 10000;
        private const int ScoreOpenFour = 50000;
        private const int ScoreThree = 1000;
        private const int ScoreOpenThree = 5000;
        private const int ScoreTwo = 100;
        private const int ScoreOpenTwo = 500;
        private const int ScoreOne = 10;

        // Direction array
        private static readonly int[,] Directions = {
            { 0, 1 },   // Horizontal
            { 1, 0 },   // Vertical
            { 1, 1 },   // Diagonal ↘
            { 1, -1 }   // Diagonal ↙
        };

        public AIService()
        {
            _winChecker = new WinChecker();
        }

        /// <summary>
        /// AI calculates the best move position
        /// </summary>
        public Move? GetBestMove(Board board, PlayerType aiPlayer)
        {
            int[,] grid = board.GetGridCopy();
            int opponent = aiPlayer == PlayerType.Black ? (int)PlayerType.White : (int)PlayerType.Black;

            // Get candidate positions
            var candidates = GetCandidateMoves(grid);
            if (candidates.Count == 0)
            {
                // Board is empty, play center
                return new Move(BoardSize / 2, BoardSize / 2, aiPlayer);
            }

            Move? bestMove = null;
            int bestScore = int.MinValue;
            int alpha = int.MinValue;
            int beta = int.MaxValue;

            // Sort candidates by evaluation score
            var sortedCandidates = SortCandidates(grid, candidates, (int)aiPlayer);

            foreach (var (row, col) in sortedCandidates)
            {
                // Try placing move
                grid[row, col] = (int)aiPlayer;

                // Check for immediate win
                if (CheckWinAtPosition(grid, row, col, (int)aiPlayer))
                {
                    grid[row, col] = 0;
                    return new Move(row, col, aiPlayer);
                }

                // Minimax evaluation
                int score = Minimax(grid, MaxDepth - 1, alpha, beta, false, (int)aiPlayer, opponent);

                grid[row, col] = 0;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = new Move(row, col, aiPlayer);
                }

                alpha = Math.Max(alpha, score);
            }

            return bestMove;
        }

        /// <summary>
        /// Minimax algorithm + Alpha-Beta pruning
        /// </summary>
        private int Minimax(int[,] grid, int depth, int alpha, int beta, bool isMaximizing, int aiPlayer, int opponent)
        {
            // Terminal condition
            if (depth == 0)
            {
                return EvaluateBoard(grid, aiPlayer, opponent);
            }

            var candidates = GetCandidateMoves(grid);
            if (candidates.Count == 0)
            {
                return EvaluateBoard(grid, aiPlayer, opponent);
            }

            int currentPlayer = isMaximizing ? aiPlayer : opponent;
            var sortedCandidates = SortCandidates(grid, candidates, currentPlayer);

            // Limit search width
            int maxWidth = Math.Min(sortedCandidates.Count, 10);

            if (isMaximizing)
            {
                int maxEval = int.MinValue;

                for (int i = 0; i < maxWidth; i++)
                {
                    var (row, col) = sortedCandidates[i];
                    grid[row, col] = aiPlayer;

                    // Check for win
                    if (CheckWinAtPosition(grid, row, col, aiPlayer))
                    {
                        grid[row, col] = 0;
                        return ScoreWin + depth; // Earlier win = higher score
                    }

                    int eval = Minimax(grid, depth - 1, alpha, beta, false, aiPlayer, opponent);
                    grid[row, col] = 0;

                    maxEval = Math.Max(maxEval, eval);
                    alpha = Math.Max(alpha, eval);

                    if (beta <= alpha)
                        break; // Beta cutoff
                }

                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;

                for (int i = 0; i < maxWidth; i++)
                {
                    var (row, col) = sortedCandidates[i];
                    grid[row, col] = opponent;

                    // Check for opponent win
                    if (CheckWinAtPosition(grid, row, col, opponent))
                    {
                        grid[row, col] = 0;
                        return -ScoreWin - depth; // Earlier opponent win = lower score
                    }

                    int eval = Minimax(grid, depth - 1, alpha, beta, true, aiPlayer, opponent);
                    grid[row, col] = 0;

                    minEval = Math.Min(minEval, eval);
                    beta = Math.Min(beta, eval);

                    if (beta <= alpha)
                        break; // Alpha cutoff
                }

                return minEval;
            }
        }

        /// <summary>
        /// Get candidate move positions (empty cells near existing pieces)
        /// </summary>
        private List<(int row, int col)> GetCandidateMoves(int[,] grid, int radius = 2)
        {
            var candidates = new HashSet<(int, int)>();
            bool hasAnyPiece = false;

            for (int i = 0; i < BoardSize; i++)
            {
                for (int j = 0; j < BoardSize; j++)
                {
                    if (grid[i, j] != 0)
                    {
                        hasAnyPiece = true;
                        // Add surrounding empty cells
                        for (int dr = -radius; dr <= radius; dr++)
                        {
                            for (int dc = -radius; dc <= radius; dc++)
                            {
                                int nr = i + dr;
                                int nc = j + dc;
                                if (nr >= 0 && nr < BoardSize && nc >= 0 && nc < BoardSize && grid[nr, nc] == 0)
                                {
                                    candidates.Add((nr, nc));
                                }
                            }
                        }
                    }
                }
            }

            if (!hasAnyPiece)
            {
                candidates.Add((BoardSize / 2, BoardSize / 2));
            }

            return new List<(int, int)>(candidates);
        }

        /// <summary>
        /// Sort candidates by evaluation score
        /// </summary>
        private List<(int row, int col)> SortCandidates(int[,] grid, List<(int row, int col)> candidates, int player)
        {
            var scoredCandidates = new List<(int row, int col, int score)>();

            foreach (var (row, col) in candidates)
            {
                int score = EvaluatePosition(grid, row, col, player);
                scoredCandidates.Add((row, col, score));
            }

            scoredCandidates.Sort((a, b) => b.score.CompareTo(a.score));

            var result = new List<(int row, int col)>();
            foreach (var item in scoredCandidates)
            {
                result.Add((item.row, item.col));
            }

            return result;
        }

        /// <summary>
        /// Evaluate single position value
        /// </summary>
        private int EvaluatePosition(int[,] grid, int row, int col, int player)
        {
            int score = 0;
            int opponent = player == 1 ? 2 : 1;

            // Simulate placing piece and evaluate
            grid[row, col] = player;

            // Check four directions
            for (int d = 0; d < 4; d++)
            {
                var (count, openEnds) = _winChecker.GetLineInfo(grid, row, col,
                    Directions[d, 0], Directions[d, 1], player);
                score += GetPatternScore(count, openEnds);
            }

            grid[row, col] = 0;

            // Also evaluate defense value
            grid[row, col] = opponent;
            int defenseScore = 0;

            for (int d = 0; d < 4; d++)
            {
                var (count, openEnds) = _winChecker.GetLineInfo(grid, row, col,
                    Directions[d, 0], Directions[d, 1], opponent);
                defenseScore += GetPatternScore(count, openEnds);
            }

            grid[row, col] = 0;

            // Offense value slightly higher than defense
            return score + (int)(defenseScore * 0.9);
        }

        /// <summary>
        /// Get score based on pattern
        /// </summary>
        private int GetPatternScore(int count, int openEnds)
        {
            if (count >= 5) return ScoreWin;

            if (count == 4)
            {
                if (openEnds == 2) return ScoreOpenFour;
                if (openEnds == 1) return ScoreFour;
            }
            else if (count == 3)
            {
                if (openEnds == 2) return ScoreOpenThree;
                if (openEnds == 1) return ScoreThree;
            }
            else if (count == 2)
            {
                if (openEnds == 2) return ScoreOpenTwo;
                if (openEnds == 1) return ScoreTwo;
            }
            else if (count == 1)
            {
                if (openEnds >= 1) return ScoreOne;
            }

            return 0;
        }

        /// <summary>
        /// Evaluate entire board state
        /// </summary>
        private int EvaluateBoard(int[,] grid, int aiPlayer, int opponent)
        {
            int score = 0;

            for (int i = 0; i < BoardSize; i++)
            {
                for (int j = 0; j < BoardSize; j++)
                {
                    if (grid[i, j] == aiPlayer)
                    {
                        score += EvaluatePiecePosition(grid, i, j, aiPlayer);
                    }
                    else if (grid[i, j] == opponent)
                    {
                        score -= EvaluatePiecePosition(grid, i, j, opponent);
                    }
                }
            }

            return score;
        }

        /// <summary>
        /// Evaluate single piece position value
        /// </summary>
        private int EvaluatePiecePosition(int[,] grid, int row, int col, int player)
        {
            int score = 0;

            // Only check right and down directions to avoid double counting
            int[,] halfDirections = {
                { 0, 1 },   // Horizontal right
                { 1, 0 },   // Vertical down
                { 1, 1 },   // Diagonal ↘
                { 1, -1 }   // Diagonal ↙
            };

            for (int d = 0; d < 4; d++)
            {
                int dr = halfDirections[d, 0];
                int dc = halfDirections[d, 1];

                var (count, openEnds) = GetLineInfoOneDirection(grid, row, col, dr, dc, player);
                score += GetPatternScore(count, openEnds) / 2; // Divide by 2 to avoid duplication
            }

            // Position value - center positions are more valuable
            int center = BoardSize / 2;
            int distanceFromCenter = Math.Abs(row - center) + Math.Abs(col - center);
            score += (BoardSize - distanceFromCenter);

            return score;
        }

        /// <summary>
        /// Get line info for single direction
        /// </summary>
        private (int count, int openEnds) GetLineInfoOneDirection(int[,] grid, int row, int col, int dr, int dc, int player)
        {
            int count = 1;
            int openEnds = 0;

            // Check forward
            int r = row + dr;
            int c = col + dc;
            while (r >= 0 && r < BoardSize && c >= 0 && c < BoardSize && grid[r, c] == player)
            {
                count++;
                r += dr;
                c += dc;
            }
            if (r >= 0 && r < BoardSize && c >= 0 && c < BoardSize && grid[r, c] == 0)
                openEnds++;

            // Check backward
            r = row - dr;
            c = col - dc;
            while (r >= 0 && r < BoardSize && c >= 0 && c < BoardSize && grid[r, c] == player)
            {
                count++;
                r -= dr;
                c -= dc;
            }
            if (r >= 0 && r < BoardSize && c >= 0 && c < BoardSize && grid[r, c] == 0)
                openEnds++;

            return (count, openEnds);
        }

        /// <summary>
        /// Check if position wins
        /// </summary>
        private bool CheckWinAtPosition(int[,] grid, int row, int col, int player)
        {
            for (int d = 0; d < 4; d++)
            {
                int count = 1;
                int dr = Directions[d, 0];
                int dc = Directions[d, 1];

                // Count forward
                int r = row + dr;
                int c = col + dc;
                while (r >= 0 && r < BoardSize && c >= 0 && c < BoardSize && grid[r, c] == player)
                {
                    count++;
                    r += dr;
                    c += dc;
                }

                // Count backward
                r = row - dr;
                c = col - dc;
                while (r >= 0 && r < BoardSize && c >= 0 && c < BoardSize && grid[r, c] == player)
                {
                    count++;
                    r -= dr;
                    c -= dc;
                }

                if (count >= 5)
                    return true;
            }

            return false;
        }
    }
}
