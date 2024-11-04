namespace BetterJunimosRedux
{
    /// <summary>
    /// Junimo hut mode.
    /// </summary>
    public enum Mode
    {
        /// <summary>
        /// Defaul behaviour.
        /// </summary>
        Normal,

        /// <summary>
        /// Grows and harvests crops.
        /// </summary>
        Crops,

        /// <summary>
        /// Grows and harvests fruit trees.
        /// </summary>
        Orchard,

        /// <summary>
        /// Grows and harvests trees.
        /// </summary>
        Forest,

        /// <summary>
        /// Creates a new maze each day.
        /// </summary>
        Maze,
    }
}