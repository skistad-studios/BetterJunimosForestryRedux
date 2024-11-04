namespace BetterJunimosRedux.Abilities
{
    using System;
    using System.Collections.Generic;
    using BetterJunimos.Abilities;
    using Microsoft.Xna.Framework;
    using StardewValley;
    using StardewValley.Characters;
    using StardewValley.TerrainFeatures;

    /// <summary>
    /// Allows junimos to harvest grass.
    /// </summary>
    public class HarvestGrass : IJunimoAbility
    {
        /// <inheritdoc/>
        public string AbilityName()
        {
            return "HarvestGrass";
        }

        /// <inheritdoc/>
        public bool IsActionAvailable(GameLocation location, Vector2 pos, Guid hutGuid)
        {
            if (!ModEntry.Config.HarvestGrassEnabled)
            {
                return false;
            }

            return Utils.GetTileIsInHutRadius(hutGuid, pos) && location.terrainFeatures.ContainsKey(pos) && location.terrainFeatures[pos] is Grass;
        }

        /// <inheritdoc/>
        public bool IsActionAvailable(Farm farm, Vector2 pos, Guid hutGuid)
        {
            return this.IsActionAvailable((GameLocation)farm, pos, hutGuid);
        }

        /// <inheritdoc/>
        public bool PerformAction(GameLocation location, Vector2 pos, JunimoHarvester junimo, Guid hutGuid)
        {
            if (!ModEntry.Config.HarvestGrassEnabled)
            {
                return false;
            }

            if (Utils.GetTileIsInHutRadius(hutGuid, pos) && location.terrainFeatures.ContainsKey(pos) && location.terrainFeatures[pos] is Grass grass)
            {
                return this.TryHarvestGrass(location, pos, grass);
            }

            return false;
        }

        /// <inheritdoc/>
        public bool PerformAction(Farm farm, Vector2 pos, JunimoHarvester junimo, Guid hutGuid)
        {
            return this.PerformAction((GameLocation)farm, pos, junimo, hutGuid);
        }

        /// <inheritdoc/>
        public List<string> RequiredItems()
        {
            return new List<string>();
        }

        private bool TryHarvestGrass(GameLocation location, Vector2 pos, Grass grass)
        {
            location.terrainFeatures.Remove(pos);
            if (Game1.random.NextDouble() < 0.5f)
            {
                GameLocation.StoreHayInAnySilo(1, location);
            }

            return true;
        }
    }
}