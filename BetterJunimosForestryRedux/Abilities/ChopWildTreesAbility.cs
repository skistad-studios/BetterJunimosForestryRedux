namespace BetterJunimosForestryRedux.Abilities
{
    using System;
    using System.Collections.Generic;
    using BetterJunimos.Abilities;
    using Microsoft.Xna.Framework;
    using StardewValley;
    using StardewValley.Characters;
    using StardewValley.TerrainFeatures;
    using StardewValley.Tools;

    /// <summary>
    /// Allows junimos to chop wild trees.
    /// </summary>
    public class ChopWildTreesAbility : IJunimoAbility
    {
        private Axe fakeAxe = new Axe()
        {
            UpgradeLevel = 1,
            IsEfficient = true,
            additionalPower = new Netcode.NetInt(100),
        };

        private FakeFarmer fakeFarmer = new FakeFarmer();

        /// <inheritdoc/>
        public string AbilityName()
        {
            return "ChopWildTrees";
        }

        /// <inheritdoc/>
        public bool IsActionAvailable(GameLocation location, Vector2 pos, Guid hutGuid)
        {
            Mode mode = Utils.GetHutMode(hutGuid);
            if (mode == Mode.Normal)
            {
                return false;
            }

            bool foundTree = false;
            Utils.ForEachDirection(pos, (p) =>
            {
                if (Utils.GetIsTileInHutRadius(hutGuid, location, p))
                {
                    if (!foundTree && location.terrainFeatures.ContainsKey(p) && GetShouldHarvestTree(location.terrainFeatures[p], mode))
                    {
                        foundTree = true;
                    }
                }
            });

            return foundTree;
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
            bool foundTree = false;
            int direction = 0;
            Utils.ForEachDirection(pos, (p) =>
            {
                if (Utils.GetIsTileInHutRadius(hutGuid, location, p))
                {
                    if (!foundTree && location.terrainFeatures.ContainsKey(p) && GetShouldHarvestTree(location.terrainFeatures[p], mode))
                    {
                        junimo.faceDirection(direction);
                        Utils.UseToolOnTile(this.fakeAxe, this.fakeFarmer, location, p);
                        foundTree = true;
                    }
                }

                direction += 1;
            });

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

        private static bool GetShouldHarvestTree(TerrainFeature terrainFeature, Mode mode)
        {
            Tree tree = terrainFeature as Tree;
            if (tree == null)
            {
                return false;
            }

            if (tree.tapped.Value)
            {
                return false;
            }

            if (mode == Mode.Crops || mode == Mode.Orchard)
            {
                return true;
            }

            if (tree.growthStage.Value < 5)
            {
                return false;
            }

            if (ModEntry.Config.SustainableWildTreeHarvesting && !tree.hasSeed.Value)
            {
                return false;
            }

            return true;
        }
    }
}