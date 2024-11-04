namespace BetterJunimosForestryRedux.Abilities
{
    using System;
    using System.Collections.Generic;
    using BetterJunimos.Abilities;
    using Microsoft.Xna.Framework;
    using StardewValley;
    using StardewValley.Characters;
    using StardewValley.TerrainFeatures;

    /// <summary>
    /// Allows junimos to harvest fruit trees.
    /// </summary>
    public class HarvestFruitTreesAbility : IJunimoAbility
    {
        /// <inheritdoc/>
        public string AbilityName()
        {
            return "HarvestFruitTrees";
        }

        /// <inheritdoc/>
        public bool IsActionAvailable(GameLocation location, Vector2 pos, Guid hutGuid)
        {
            bool foundTree = false;
            Utils.ForEachDirection(pos, (p) =>
            {
                if (!foundTree &&
                Utils.GetIsTileInHutRadius(hutGuid, p) &&
                location.terrainFeatures.ContainsKey(p) &&
                this.GetIsHarvestableFruitTree(location.terrainFeatures[p]))
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
            bool harvestedFruit = false;
            int direction = 0;
            Utils.ForEachDirection(pos, (p) =>
            {
                if (!harvestedFruit &&
                Utils.GetIsTileInHutRadius(hutGuid, p) &&
                location.terrainFeatures.ContainsKey(p) &&
                this.GetIsHarvestableFruitTree(location.terrainFeatures[p]))
                {
                    junimo.faceDirection(direction);
                    FruitTree tree = location.terrainFeatures[p] as FruitTree;
                    harvestedFruit = this.HarvestFruitTree(tree, hutGuid);
                }

                direction += 1;
            });

            return harvestedFruit;
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

        private bool GetIsHarvestableFruitTree(TerrainFeature tf)
        {
            return tf is FruitTree tree && tree.fruit.Any();
        }

        private bool HarvestFruitTree(FruitTree tree, Guid hutGuid)
        {
            if (!tree.fruit.Any())
            {
                return false;
            }

            foreach (Item item in tree.fruit)
            {
                if (item is StardewValley.Object obj)
                {
                    Utils.AddItemToHut(hutGuid, obj);
                }
            }

            tree.fruit.Clear();
            tree.performUseAction(tree.Tile);
            return true;
        }
    }
}