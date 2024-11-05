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
    /// Allows junimos to plant trees.
    /// </summary>
    public class PlantTreesAbility : IJunimoAbility
    {
        /// <summary>
        /// Get if a wild tree should be planted on the given tile.
        /// </summary>
        /// <param name="hutGuid">Guid of the hut planting trees.</param>
        /// <param name="location">Location of the tile.</param>
        /// <param name="pos">Position of the tile.</param>
        /// <returns>True if a wild tree should be planted at the tile, otherwise false.</returns>
        public static bool GetShouldPlantWildTreeHere(Guid hutGuid, GameLocation location, Vector2 pos)
        {
            if (Utils.GetIsTileInFrontOfDoor(hutGuid, pos))
            {
                return false;
            }

            return Utils.GetIsTileInWildTreePattern(pos) && GetIsPlantable(location, pos);
        }

        /// <inheritdoc/>
        public string AbilityName()
        {
            return "PlantTrees";
        }

        /// <inheritdoc/>
        public bool IsActionAvailable(GameLocation location, Vector2 pos, Guid hutGuid)
        {
            Mode mode = Utils.GetHutMode(hutGuid);
            if (mode != Mode.Forest)
            {
                return false;
            }

            bool foundPos = false;
            Utils.ForEachDirection(pos, (p) =>
            {
                if (!foundPos && Utils.GetIsTileInHutRadius(hutGuid, location, p) && GetShouldPlantWildTreeHere(hutGuid, location, p))
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
            if (mode != Mode.Forest)
            {
                return false;
            }

            Chest chest = Utils.GetHutFromGuid(hutGuid).GetOutputChest();
            var foundItem = chest.Items.FirstOrDefault(item => item != null && Utils.GetAllWildTreeSeeds().Contains(item.ItemId));
            if (foundItem == null)
            {
                return false;
            }

            bool planted = false;
            int direction = 0;
            Utils.ForEachDirection(pos, (p) =>
            {
                if (!planted && Utils.GetIsTileInHutRadius(hutGuid, location, p) && GetShouldPlantWildTreeHere(hutGuid, location, p) && this.Plant(location, p, hutGuid, foundItem))
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
            return Utils.GetAllWildTreeSeeds();
        }

        private static bool GetIsPlantable(GameLocation location, Vector2 pos)
        {
            if (location.IsTileOccupiedBy(pos) && !Utils.GetIsHoed(location, pos))
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

            return true;
        }

        private bool Plant(GameLocation location, Vector2 pos, Guid hutGuid, Item item)
        {
            if (location.terrainFeatures.TryGetValue(pos, out TerrainFeature feature))
            {
                HoeDirt hoeDirt = feature as HoeDirt;
                if (hoeDirt != null && hoeDirt.crop == null)
                {
                    location.terrainFeatures.Remove(pos);
                }
            }

            if (location.terrainFeatures.Keys.Contains(pos))
            {
                return false;
            }

            Tree tree = new Tree(Utils.GetWildTreeFromSeed(item.itemId.Value));
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