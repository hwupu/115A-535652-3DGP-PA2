namespace PuzzleMaster.Monsters
{
    /// <summary>What a recovered (purified) monster does afterwards (ADR-0014).</summary>
    public enum RecoveredStyle
    {
        /// <summary>Stays where it is, purified (tinted, no longer attacking).</summary>
        Stay,
        /// <summary>Holds the purified pose briefly, then rises, spins and fades out (the released spirit).</summary>
        FloatAway,
    }
}
