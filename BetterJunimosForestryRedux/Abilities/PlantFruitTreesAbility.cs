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
    /// Allows junimos to plant fruit trees.
    /// </summary>
    public class PlantFruitTreesAbility : IJunimoAbility
    {
        /// <inheritdoc/>
        public string AbilityName()
        {
            return "PlantFruitTrees";
        }

        /// <inheritdoc/>
        public bool IsActionAvailable(GameLocation location, Vector2 pos, Guid hutGuid)
        {
            Mode mode = Utils.GetHutMode(hutGuid);
            if (mode != Mode.Orchard)
            {
                return false;
            }

            bool foundPos = false;
            Utils.ForEachDirection(pos, (p) =>
            {
                if (!foundPos && Utils.GetIsTileInHutRadius(hutGuid, location, p) && this.GetShouldPlantFruitTreeHere(hutGuid, location, p))
                {
                    foundPos = true;
                }
            });

            return foundPos;
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
            if (mode != Mode.Orchard)
            {
                return false;
            }

            Chest chest = Utils.GetHutFromGuid(hutGuid).GetOutputChest();
            var foundItem = chest.Items.FirstOrDefault(item => item != null && Utils.GetAllFruitTrees().Contains(item.ItemId));
            if (foundItem == null)
            {
                return false;
            }

            bool planted = false;
            int direction = 0;
            Utils.ForEachDirection(pos, (p) =>
            {
                if (!planted && Utils.GetIsTileInHutRadius(hutGuid, location, p) && this.GetShouldPlantFruitTreeHere(hutGuid, location, p) && this.Plant(location, p, hutGuid, foundItem))
                {
                    junimo.faceDirection(direction);
                    planted = true;
                }

                direction += 1;
            });

            return planted;
        }

        /// <inheritdoc/>
        public bool PerformAction(Farm farm, Vector2 pos, JunimoHarvester junimo, Guid hutGuid)
        {
            return this.PerformAction((GameLocation)farm, pos, junimo, hutGuid);
        }

        /// <inheritdoc/>
        public List<string> RequiredItems()
        {
            return Utils.GetAllFruitTrees();
        }

        private bool GetShouldPlantFruitTreeHere(Guid hutGuid, GameLocation location, Vector2 pos)
        {
            if (Utils.GetIsTileInFrontOfDoor(hutGuid, pos))
            {
                return false;
            }

            return Utils.GetIsTileInFruitTreePattern(pos) && this.GetIsPlantable(location, pos);
        }

        private bool GetIsPlantable(GameLocation location, Vector2 pos)
        {
            if (location.IsTileOccupiedBy(pos))
            {
                return false;
            }

            if (Utils.GetIsHoed(location, pos))
            {
                return false;
            }

            if (Utils.GetHasCrop(location, pos))
            {
                return false;
            }

            if (Utils.GetIsOccupied(location, pos))
            {
                return false;
            }

            if (!Utils.GetCanSpawnTrees(location, pos))
            {
                return false;
            }

            if (!Utils.GetCanBeHoed(location, pos))
            {
                return false;
            }

            if (FruitTree.IsGrowthBlocked(pos, location))
            {
                return false;
            }

            return true;
        }

        private bool Plant(GameLocation location, Vector2 pos, Guid hutGuid, Item item)
        {
            if (location.terrainFeatures.Keys.Contains(pos))
            {
                return false;
            }

            FruitTree tree = new FruitTree(item.itemId.Value);
            location.terrainFeatures.Add(pos, tree);

            if (Utility.isOnScreen(Utility.Vector2ToPoint(pos), Game1.tileSize, location))
            {
                location.playSound("stoneStep");
                location.playSound("dirtyHit");
            }

            Utils.RemoveItemFromHut(hutGuid, item);
            return true;
        }
    }
}