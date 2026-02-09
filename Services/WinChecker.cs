using GomokuAI.Models;

namespace GomokuAI.Services
{
    /// <summary>
    /// Win detection service
    /// </summary>
    public class WinChecker
    {
        private const int WinCount = 5;

        // Four directions: Horizontal, Vertical, Diagonal ↘, Diagonal ↙
        private static readonly int[,] Directions = {
            { 0, 1 },   // Horizontal
            { 1, 0 },   // Vertical
            { 1, 1 },   // Diagonal ↘
            { 1, -1 }   // Diagonal ↙
        };

        /// <summary>
        /// Check if the specified position wins
        /// </summary>
        public bool CheckWin(Board board, int row, int col, PlayerType player)
        {
            if (player == PlayerType.None)
                return false;

            for (int d = 0; d < 4; d++)
            {
                int count = 1; // Include current piece
                int dr = Directions[d, 0];
                int dc = Directions[d, 1];

                // Count forward
                count += CountDirection(board, row, col, dr, dc, player);
                // Count backward
                count += CountDirection(board, row, col, -dr, -dc, player);

                if (count >= WinCount)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Scan the entire board for a winner
        /// </summary>
        public PlayerType CheckBoardForWinner(Board board)
        {
            for (int row = 0; row < Board.Size; row++)
            {
                for (int col = 0; col < Board.Size; col++)
                {
                    var cell = board.GetCell(row, col);
                    if (cell != PlayerType.None)
                    {
                        if (CheckWin(board, row, col, cell))
                        {
                            return cell;
                        }
                    }
                }
            }
            return PlayerType.None;
        }

        /// <summary>
        /// Count consecutive pieces in specified direction
        /// </summary>
        private int CountDirection(Board board, int row, int col, int dr, int dc, PlayerType player)
        {
            int count = 0;
            int r = row + dr;
            int c = col + dc;

            while (board.IsValidPosition(r, c) && board.GetCell(r, c) == player)
            {
                count++;
                r += dr;
                c += dc;
            }

            return count;
        }

        /// <summary>
        /// Count consecutive pieces at position (for AI evaluation)
        /// </summary>
        public int CountConsecutive(int[,] grid, int row, int col, int dr, int dc, int player)
        {
            int count = 0;
            int r = row;
            int c = col;
            int size = grid.GetLength(0);

            while (r >= 0 && r < size && c >= 0 && c < size && grid[r, c] == player)
            {
                count++;
                r += dr;
                c += dc;
            }

            return count;
        }

        /// <summary>
        /// Get line pattern info (count, open ends)
        /// </summary>
        public (int count, int openEnds) GetLineInfo(int[,] grid, int row, int col, int dr, int dc, int player)
        {
            int size = grid.GetLength(0);
            int count = 1;
            int openEnds = 0;

            // Check forward
            int r = row + dr;
            int c = col + dc;
            while (r >= 0 && r < size && c >= 0 && c < size && grid[r, c] == player)
            {
                count++;
                r += dr;
                c += dc;
            }
            // Check if forward end is open
            if (r >= 0 && r < size && c >= 0 && c < size && grid[r, c] == 0)
                openEnds++;

            // Check backward
            r = row - dr;
            c = col - dc;
            while (r >= 0 && r < size && c >= 0 && c < size && grid[r, c] == player)
            {
                count++;
                r -= dr;
                c -= dc;
            }
            // Check if backward end is open
            if (r >= 0 && r < size && c >= 0 && c < size && grid[r, c] == 0)
                openEnds++;

            return (count, openEnds);
        }
    }
}
