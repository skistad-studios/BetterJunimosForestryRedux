namespace BetterJunimosRedux
{
    /// <summary>
    /// The mod's config file.
    /// </summary>
    public class ModConfig
    {
        /// <summary>
        /// Wild tree planting patterns.
        /// </summary>
        public enum WildTreePatternType
        {
            /// <summary>
            /// Allows for a more walkable area.
            /// </summary>
            Loose = 0,

            /// <summary>
            /// Allows for a very small walkable area.
            /// </summary>
            Tight = 1,
        }

        /// <summary>
        /// Fruit tree planting patterns.
        /// </summary>
        public enum FruitTreePatternType
        {
            /// <summary>
            /// Plants in rows.
            /// </summary>
            Rows = 0,

            /// <summary>
            /// Plants diagnaly.
            /// </summary>
            Diagonal = 1,

            /// <summary>
            /// Plants very tightly, with minimal room for walking.
            /// </summary>
            Tight = 2,
        }

        /// <summary>
        /// Gets or sets the patter wild trees will be planted.
        /// </summary>
        public WildTreePatternType WildTreePattern { get; set; } = WildTreePatternType.Loose;

        /// <summary>
        /// Gets or sets the patter fruit trees will be planted.
        /// </summary>
        public FruitTreePatternType FruitTreePattern { get; set; } = FruitTreePatternType.Rows;

        /// <summary>
        /// Gets or sets a value indicating whether junimos will wait until they have a seed for wild tree harvesting.
        /// </summary>
        public bool SustainableWildTreeHarvesting { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether junimos will harvest grass.
        /// </summary>
        public bool HarvestGrassEnabled { get; set; } = true;
    }
}