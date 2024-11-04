namespace BetterJunimosRedux.Abilities
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
        private const string TreeFertilizer = "805";

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
                if (!foundTree &&
                Utils.GetTileIsInHutRadius(hutGuid, p) &&
                location.terrainFeatures.ContainsKey(p) &&
                location.terrainFeatures[p] is Tree tree &&
                tree.growthStage.Value < 5 &&
                !location.objects.ContainsKey(p) &&
                !tree.fertilized.Value)
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
            Item foundItem = chest.Items.FirstOrDefault(item => item.itemId.Value == TreeFertilizer);
            if (foundItem == null)
            {
                return false;
            }

            bool fertilizedTree = false;
            int direction = 0;
            Utils.ForEachDirection(pos, (p) =>
            {
                if (!fertilizedTree &&
                Utils.GetTileIsInHutRadius(hutGuid, p) &&
                location.terrainFeatures.ContainsKey(p) &&
                location.terrainFeatures[p] is Tree tree &&
                tree.growthStage.Value < 5 &&
                !location.objects.ContainsKey(p) &&
                !tree.fertilized.Value)
                {
                    tree.fertilize();
                    Utils.RemoveItemFromHut(hutGuid, foundItem);
                    junimo.faceDirection(direction);
                    fertilizedTree = true;
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
                TreeFertilizer,
            };
        }
    }
}