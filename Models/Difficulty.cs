namespace GomokuAI.Models
{
    /// <summary>
    /// AI Difficulty levels with configurable search depth
    /// </summary>
    public enum Difficulty
    {
        Easy = 2,      // Depth 2 - Fast, less challenging
        Medium = 3,    // Depth 3 - Balanced
        Hard = 4       // Depth 4 - Slower, more challenging
    }

    /// <summary>
    /// Difficulty configuration helper
    /// </summary>
    public static class DifficultyConfig
    {
        public static string GetName(Difficulty difficulty)
        {
            return difficulty switch
            {
                Difficulty.Easy => "Easy",
                Difficulty.Medium => "Medium",
                Difficulty.Hard => "Hard",
                _ => "Medium"
            };
        }

        public static int GetDepth(Difficulty difficulty)
        {
            return (int)difficulty;
        }

        public static string GetDescription(Difficulty difficulty)
        {
            return difficulty switch
            {
                Difficulty.Easy => "Depth 2 - Quick response",
                Difficulty.Medium => "Depth 3 - Balanced",
                Difficulty.Hard => "Depth 4 - Strong AI",
                _ => ""
            };
        }
    }
}

