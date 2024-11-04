namespace BetterJunimosForestryRedux
{
    using Microsoft.Xna.Framework;
    using StardewValley;

    /// <summary>
    /// A fake farmer for using fake tools.
    /// </summary>
    public class FakeFarmer : Farmer
    {
        /// <inheritdoc/>
        public override Vector2 GetToolLocation(bool ignoreClick = false)
        {
            return new Vector2(0.0f, 0.0f);
        }
    }
}