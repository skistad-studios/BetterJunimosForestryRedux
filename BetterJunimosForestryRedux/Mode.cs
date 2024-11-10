namespace BetterJunimosForestryRedux
{
    /// <summary>
    /// Junimo hut mode.
    /// </summary>
    public enum Mode
    {
        /// <summary>
        /// Defaul behaviour.
        /// </summary>
        Normal = 0,

        /// <summary>
        /// Grows and harvests crops.
        /// </summary>
        Crops = 1,

        /// <summary>
        /// Grows and harvests trees.
        /// </summary>
        Forest = 2,

        /// <summary>
        /// Grows and harvests fruit trees.
        /// </summary>
        Orchard = 3,
    }
}