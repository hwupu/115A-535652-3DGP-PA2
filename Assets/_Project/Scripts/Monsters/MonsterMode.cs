namespace PuzzleMaster.Monsters
{
    /// <summary>What a monster does while the player is out of range (spec §3: patrol or remain stationary).</summary>
    public enum MonsterMode
    {
        /// <summary>Stays at its spot.</summary>
        Stationary,
        /// <summary>Walks back and forth between its waypoints.</summary>
        Patrol,
    }
}
