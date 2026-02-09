namespace GomokuAI.Models
{
    /// <summary>
    /// Player type enumeration
    /// </summary>
    public enum PlayerType
    {
        None = 0,
        Black = 1,  // Black piece (Human player)
        White = 2   // White piece (AI)
    }

    /// <summary>
    /// Player class
    /// </summary>
    public class Player
    {
        public PlayerType Type { get; set; }
        public string Name { get; set; }
        public bool IsAI { get; set; }

        public Player(PlayerType type, string name, bool isAI = false)
        {
            Type = type;
            Name = name;
            IsAI = isAI;
        }

        public static Player CreateHumanPlayer()
        {
            return new Player(PlayerType.Black, "Player", false);
        }

        public static Player CreateAIPlayer()
        {
            return new Player(PlayerType.White, "AI", true);
        }
    }
}
