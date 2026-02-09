namespace GomokuAI.Models
{
    /// <summary>
    /// Move class - Records information for each move
    /// </summary>
    public class Move
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public PlayerType Player { get; set; }
        public int MoveNumber { get; set; }

        public Move(int row, int col, PlayerType player, int moveNumber = 0)
        {
            Row = row;
            Col = col;
            Player = player;
            MoveNumber = moveNumber;
        }

        public override string ToString()
        {
            string playerName = Player == PlayerType.Black ? "Black" : "White";
            return $"#{MoveNumber}: {playerName} ({Row}, {Col})";
        }

        public Move Clone()
        {
            return new Move(Row, Col, Player, MoveNumber);
        }
    }
}
