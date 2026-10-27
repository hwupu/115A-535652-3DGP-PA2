namespace PuzzleMaster.Core
{
    /// <summary>The four states of the spec's gameplay flow (ADR-0006).</summary>
    public enum GameState
    {
        /// <summary>Introduction screen; the world is frozen until SPACE.</summary>
        Intro,
        /// <summary>Gameplay: the timer runs, the player can move and fight.</summary>
        Playing,
        /// <summary>All five puzzles solved ("Congratulations!").</summary>
        Won,
        /// <summary>The timer reached zero ("Time's Up!").</summary>
        Lost,
    }
}
