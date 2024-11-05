namespace BetterJunimosForestryRedux.Abilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BetterJunimos.Abilities;
    using Microsoft.Xna.Framework;
    using StardewValley;
    using StardewValley.Characters;
    using StardewValley.Objects;
    using StardewValley.TerrainFeatures;

    /// <summary>
    /// Allows junimos to fertilize trees.
    /// </summary>
    public class FertilizeTreesAbility : IJunimoAbility
    {
        private const string TreeFertilizerId = "805";

        /// <inheritdoc/>
        public string AbilityName()
        {
            return "FertilizeTrees";
        }

        /// <inheritdoc/>
        public bool IsActionAvailable(GameLocation location, Vector2 pos, Guid hutGuid)
        {
            bool foundTree = false;
            Utils.ForEachDirection(pos, (p) =>
            {
                if (!foundTree && Utils.GetIsTileInHutRadius(hutGuid, location, p) && location.terrainFeatures.ContainsKey(p) && this.GetCanFertilizeTree(location, pos, location.terrainFeatures[p]))
                {
                    foundTree = true;
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
            Chest chest = Utils.GetHutFromGuid(hutGuid).GetOutputChest();
            Item foundItem = chest.Items.FirstOrDefault(item => item.itemId.Value == TreeFertilizerId);
            if (foundItem == null)
            {
                return false;
            }

            bool fertilizedTree = false;
            int direction = 0;
            Utils.ForEachDirection(pos, (p) =>
            {
                if (!fertilizedTree && Utils.GetIsTileInHutRadius(hutGuid, location, p) && location.terrainFeatures.ContainsKey(p) && this.GetCanFertilizeTree(location, pos, location.terrainFeatures[p]))
                {
                    Utils.RemoveItemFromHut(hutGuid, foundItem);
                    junimo.faceDirection(direction);
                    fertilizedTree = this.FertilizeTree(location.terrainFeatures[p]);
                }

                direction += 1;
            });

            return fertilizedTree;
        }

        /// <inheritdoc/>
        public bool PerformAction(Farm farm, Vector2 pos, JunimoHarvester junimo, Guid hutGuid)
        {
            return this.PerformAction((GameLocation)farm, pos, junimo, hutGuid);
        }

        /// <inheritdoc/>
        public List<string> RequiredItems()
        {
            return new List<string>()
            {
                TreeFertilizerId,
            };
        }

        private bool GetCanFertilizeTree(GameLocation location, Vector2 pos, TerrainFeature terrainFeature)
        {
            Tree tree = terrainFeature as Tree;
            if (tree == null)
            {
                return false;
            }

            if (tree.growthStage.Value >= 5)
            {
                return false;
            }

            if (tree.fertilized.Value)
            {
                return false;
            }

            if (location.objects.ContainsKey(pos))
            {
                return false;
            }

            return true;
        }

        private bool FertilizeTree(TerrainFeature terrainFeature)
        {
            Tree tree = terrainFeature as Tree;
            if (tree == null)
            {
                return false;
            }

            tree.fertilize();
            return true;
        }
    }
}