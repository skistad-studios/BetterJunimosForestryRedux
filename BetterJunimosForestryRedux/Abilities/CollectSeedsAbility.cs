namespace BetterJunimosForestryRedux.Abilities
{
    using System;
    using System.Collections.Generic;
    using BetterJunimos.Abilities;
    using BetterJunimosForestryRedux;
    using Microsoft.Xna.Framework;
    using StardewValley;
    using StardewValley.Characters;
    using StardewValley.TerrainFeatures;
    using StardewValley.Tools;

    /// <summary>
    /// Allows junimos to collect planted seeds.
    /// </summary>
    public class CollectSeedsAbility : IJunimoAbility
    {
        private Axe fakeAxe = new Axe()
        {
            UpgradeLevel = 1,
            IsEfficient = true,
        };

        private FakeFarmer fakeFarmer = new FakeFarmer();

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectSeedsAbility"/> class.
        /// </summary>
        public CollectSeedsAbility()
        {
            this.fakeFarmer.setSkillLevel("Foraging", 1);
        }

        /// <inheritdoc/>
        public string AbilityName()
        {
            return "CollectSeeds";
        }

        /// <inheritdoc/>
        public bool IsActionAvailable(GameLocation location, Vector2 pos, Guid hutGuid)
        {
            Mode mode = Utils.GetHutMode(hutGuid);
            if (mode == Mode.Normal)
            {
                return false;
            }

            if (mode == Mode.Forest && Utils.GetIsTileInWildTreePattern(pos))
            {
                return false;
            }

            return Utils.GetIsTileInHutRadius(hutGuid, location, pos) && location.terrainFeatures.ContainsKey(pos) && this.GetIsHarvestableSeed(location.terrainFeatures[pos], mode);
        }

        /// <inheritdoc/>
        public bool IsActionAvailable(Farm farm, Vector2 pos, Guid hutGuid)
        {
            return this.IsActionAvailable((GameLocation)farm, pos, hutGuid);
        }

        /// <inheritdoc/>
        public bool PerformAction(GameLocation location, Vector2 pos, JunimoHarvester junimo, Guid hutGuid)
        {
            Mode mode = Utils.GetHutMode(hutGuid);
            if (mode == Mode.Normal)
            {
                return false;
            }

            if (mode == Mode.Forest && Utils.GetIsTileInWildTreePattern(pos))
            {
                return false;
            }

            if (!Utils.GetIsTileInHutRadius(hutGuid, location, pos) || !location.terrainFeatures.ContainsKey(pos) || !this.GetIsHarvestableSeed(location.terrainFeatures[pos], mode))
            {
                return false;
            }

            Utils.UseToolOnTile(this.fakeAxe, this.fakeFarmer, location, pos);
            return true;
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

        private bool GetIsHarvestableSeed(TerrainFeature terrainFeature, Mode mode)
        {
            Tree tree = terrainFeature as Tree;
            if (tree == null)
            {
                return false;
            }

            if (tree.growthStage.Value != 0)
            {
                return false;
            }

            return true;
        }
    }
}