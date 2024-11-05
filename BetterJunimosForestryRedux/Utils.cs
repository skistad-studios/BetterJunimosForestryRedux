namespace BetterJunimosForestryRedux
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using StardewModdingAPI;
    using StardewValley;
    using StardewValley.Buildings;
    using StardewValley.Objects;
    using StardewValley.TerrainFeatures;
    using xTile.Dimensions;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    /// <summary>
    /// A collection of utility methods.
    /// </summary>
    public static class Utils
    {
        private const string HutShowHudId = "SkistadStudios.BetterJunimosForestryRedux.HutShowHud";
        private const string HutModeId = "SkistadStudios.BetterJunimosForestryRedux.HutState";

        /// <summary>
        /// Gets a hut from a guid.
        /// </summary>
        /// <param name="guid">The guid of the hut.</param>
        /// <returns>The hut if one exists with the given guid, otherwise null.</returns>
        public static JunimoHut GetHutFromGuid(Guid guid)
        {
            foreach (GameLocation farm in GetAllFarms())
            {
                if (farm.buildings.TryGetValue(guid, out Building building))
                {
                    JunimoHut hut = building as JunimoHut;
                    if (hut != null)
                    {
                        return hut;
                    }
                }
            }

            ModEntry.SMonitor.Log($"Could not find hut with guid '{guid}'.", LogLevel.Error);
            return null;
        }

        /// <summary>
        /// Gets all hut guids.
        /// </summary>
        /// <returns>A list of all hut guids.</returns>
        public static List<Guid> GetAllHutGuids()
        {
            List<Guid> guids = new List<Guid>();
            foreach (GameLocation farm in GetAllFarms())
            {
                foreach (Building building in farm.buildings)
                {
                    Guid guid = farm.buildings.GuidOf(building);
                    if (guid == Guid.Empty)
                    {
                        continue;
                    }

                    if (building is JunimoHut)
                    {
                        guids.Add(guid);
                    }
                }
            }

            return guids;
        }

        /// <summary>
        /// Gets the current mode of a hut.
        /// </summary>
        /// <param name="hutGuid">The hut's guid.</param>
        /// <returns>The current mode of the hut.</returns>
        public static Mode GetHutMode(Guid hutGuid)
        {
            JunimoHut hut = GetHutFromGuid(hutGuid);
            if (!hut.modData.ContainsKey(HutModeId))
            {
                hut.modData[HutModeId] = Mode.Normal.ToString();
            }

            if (Enum.TryParse(hut.modData[HutModeId], out Mode result))
            {
                return result;
            }
            else
            {
                return Mode.Normal;
            }
        }

        /// <summary>
        /// Sets the current mode of a hut.
        /// </summary>
        /// <param name="hutGuid">The hut's guid.</param>
        /// <param name="mode">The mode to set the hut to.</param>
        public static void SetHutMode(Guid hutGuid, Mode mode)
        {
            JunimoHut hut = GetHutFromGuid(hutGuid);
            hut.modData[HutModeId] = mode.ToString();
        }

        /// <summary>
        /// Gets if a hut's hud should be shown.
        /// </summary>
        /// <param name="hutGuid">The hut's guid.</param>
        /// <returns>True if the hud should be shown, otherwise false.</returns>
        public static bool GetHutShowHud(Guid hutGuid)
        {
            JunimoHut hut = GetHutFromGuid(hutGuid);
            if (!hut.modData.ContainsKey(HutShowHudId))
            {
                hut.modData[HutShowHudId] = bool.FalseString;
            }

            if (bool.TryParse(hut.modData[HutShowHudId], out bool result))
            {
                return result;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Sets if a hut's hud should be shown.
        /// </summary>
        /// <param name="hutGuid">The hut's guid.</param>
        /// <param name="showHud">If the hud should be shown.</param>
        public static void SetHutShowHud(Guid hutGuid, bool showHud)
        {
            JunimoHut hut = GetHutFromGuid(hutGuid);
            hut.modData[HutShowHudId] = showHud.ToString();
        }

        /// <summary>
        /// Adds an object to a hut.
        /// </summary>
        /// <param name="hutGuid">Guid of the hut.</param>
        /// <param name="obj">Object to be added.</param>
        public static void AddItemToHut(Guid hutGuid, StardewValley.Object obj)
        {
            JunimoHut hut = GetHutFromGuid(hutGuid);
            Chest chest = hut.GetOutputChest();
            BetterJunimos.Utils.Util.AddItemToChest(hut.GetParentLocation(), chest, obj);
        }

        /// <summary>
        /// Removes an item from a hut.
        /// </summary>
        /// <param name="hutGuid">Guid of the hut.</param>
        /// <param name="item">Item to be removed.</param>
        /// <param name="count">Number of items to remove.</param>
        public static void RemoveItemFromHut(Guid hutGuid, Item item, int count = 1)
        {
            Chest chest = GetHutFromGuid(hutGuid).GetOutputChest();
            BetterJunimos.Utils.Util.RemoveItemFromChest(chest, item, count);
        }

        /// <summary>
        /// Gets if a given tile is within the radius of a hut.
        /// </summary>
        /// <param name="hutGuid">Guid of the hut.</param>
        /// <param name="location">Location to check.</param>
        /// <param name="tile">The tile to check.</param>
        /// <returns>True if the tile is within the hut's radius, otherwise false.</returns>
        public static bool GetIsTileInHutRadius(Guid hutGuid, GameLocation location, Vector2 tile)
        {
            if (location.IsGreenhouse)
            {
                return true;
            }

            JunimoHut hut = GetHutFromGuid(hutGuid);
            int radius = ModEntry.BetterJunimosApi.GetJunimoHutMaxRadius();
            int x = (int)tile.X;
            int y = (int)tile.Y;
            return !(x < hut.tileX.Value + 1 - radius || x >= hut.tileX.Value + 2 + radius) && !(y < hut.tileY.Value + 1 - radius || y >= hut.tileY.Value + 2 + radius);
        }

        /// <summary>
        /// Gets if a tile is in front of a hut's door.
        /// </summary>
        /// <param name="hutGuid">Guid of the hut.</param>
        /// <param name="tile">The tile to check.</param>
        /// <returns>True if the tile is in front of the hut's door, otherwise false.</returns>
        public static bool GetIsTileInFrontOfDoor(Guid hutGuid, Vector2 tile)
        {
            JunimoHut hut = GetHutFromGuid(hutGuid);
            int x = (int)tile.X;
            int y = (int)tile.Y;
            return x == hut.tileX.Value + 1 && y == hut.tileY.Value + 2;
        }

        /// <summary>
        /// Gets all farm locations.
        /// </summary>
        /// <returns>A list of all farm locations.</returns>
        public static List<GameLocation> GetAllFarms()
        {
            return Game1.locations.ToList();
        }

        /// <summary>
        /// Get whether a tile is blocked due to something it contains.
        /// </summary>
        /// <param name="location">The location to check.</param>
        /// <param name="tile">The tile to check.</param>
        /// <returns>True if a tile is occupied, otherwise false.</returns>
        public static bool GetIsOccupied(GameLocation location, Vector2 tile)
        {
            // impassable tiles (e.g. water)
            if (!location.isTilePassable(new Location((int)tile.X, (int)tile.Y), Game1.viewport))
            {
                return true;
            }

            // objects & large terrain features
            if (location.objects.ContainsKey(tile) || location.largeTerrainFeatures.Any(p => p.Tile == tile))
            {
                return true;
            }

            // logs, boulders, etc
            if (location.resourceClumps.Any(resourceClump => resourceClump.occupiesTile((int)tile.X, (int)tile.Y)))
            {
                return true;
            }

            // non-dirt terrain features
            if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature feature))
            {
                HoeDirt dirt = feature as HoeDirt;
                if (dirt != null && dirt.crop != null)
                {
                    return true;
                }
            }

            // buildings
            if (location.buildings.Any(building => building.occupiesTile(tile)))
            {
                return true;
            }

            // buildings from the map
            if (location.getTileIndexAt(Utility.Vector2ToPoint(tile), "Buildings") > -1)
            {
                return true;
            }

            // furniture
            if (location.GetFurnitureAt(tile) != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get whether a tile can be hoed.
        /// </summary>
        /// <param name="location">The location to check.</param>
        /// <param name="tile">The tile to check.</param>
        /// <returns>True if a tile can be hoed, otherwise false.</returns>
        public static bool GetCanBeHoed(GameLocation location, Vector2 tile)
        {
            int x = (int)tile.X;
            int y = (int)tile.Y;

            if (location.terrainFeatures.ContainsKey(tile))
            {
                HoeDirt hoeDirt = location.terrainFeatures[tile] as HoeDirt;
                if (hoeDirt != null)
                {
                    if (hoeDirt.crop != null)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            if (location.objects.ContainsKey(tile))
            {
                return false;
            }

            if (location.doesTileHaveProperty(x, y, "Diggable", "Back") == null)
            {
                return false;
            }

            if (!location.isTilePassable(new Location(x, y), Game1.viewport))
            {
                return false;
            }

            if (location.resourceClumps.Any(resourceClump => resourceClump.occupiesTile(x, y)))
            {
                return false;
            }

            Rectangle tileLocationRect = new Rectangle((x * Game1.tileSize) + 1, (y * Game1.tileSize) + 1, 62, 62);
            if (location.largeTerrainFeatures.Any(largeTerrainFeature => largeTerrainFeature.getBoundingBox().Intersects(tileLocationRect)))
            {
                return false;
            }

            if (location.GetFurnitureAt(tile) != null)
            {
                return false;
            }

            if (location.buildings.Any(building => building.occupiesTile(tile)))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets if trees can spawn on this tile.
        /// </summary>
        /// <param name="location">The location to check.</param>
        /// <param name="tile">The tile to check.</param>
        /// <returns>True if a tree can spawn on the tile, otherwise false.</returns>
        public static bool GetCanSpawnTrees(GameLocation location, Vector2 tile)
        {
            string noSpawn = location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "NoSpawn", "Back");
            bool cantSpawnHere = noSpawn != null && (noSpawn == "Tree" || noSpawn == "All" || noSpawn == "True");
            return !cantSpawnHere;
        }

        /// <summary>
        /// Gets the ids of all wild tree seeds.
        /// </summary>
        /// <returns>An array of all wild tree seed ids.</returns>
        public static List<string> GetAllWildTreeSeeds()
        {
            return Tree.GetWildTreeSeedLookup().Keys.ToList();
        }

        /// <summary>
        /// Gets a tree id that would come from a seed with the given id.
        /// </summary>
        /// <param name="seedId">The seed's id.</param>
        /// <returns>The tree's id if the seed id is valid, otherwise null.</returns>
        public static string GetWildTreeFromSeed(string seedId)
        {
            return Tree.GetWildTreeSeedLookup()[seedId].FirstOrDefault();
        }

        /// <summary>
        /// Uses to tool on a specific tile.
        /// </summary>
        /// <param name="tool">The tool to use.</param>
        /// <param name="farmer">The farmer using the tool.</param>
        /// <param name="location">Location the tool is being used in.</param>
        /// <param name="tile">Tile the tool is being used in.</param>
        public static void UseToolOnTile(Tool tool, Farmer farmer, GameLocation location, Vector2 tile)
        {
            Vector2 pixelPosition = (tile * Game1.tileSize) + new Vector2(Game1.tileSize / 2.0f);
            tool.DoFunction(location, (int)pixelPosition.X, (int)pixelPosition.Y, 0, farmer);
        }

        /// <summary>
        /// Perform an action for each direction from a position.
        /// </summary>
        /// <param name="pos">Originating position.</param>
        /// <param name="action">Action to take in each direction, with the parameter being the position that action takes place.</param>
        public static void ForEachDirection(Vector2 pos, Action<Vector2> action)
        {
            Vector2 up = new Vector2(pos.X, pos.Y + 1);
            Vector2 right = new Vector2(pos.X + 1, pos.Y);
            Vector2 down = new Vector2(pos.X, pos.Y - 1);
            Vector2 left = new Vector2(pos.X - 1, pos.Y);
            Vector2[] positions = [up, right, down, left];
            foreach (Vector2 position in positions)
            {
                action?.Invoke(position);
            }
        }

        /// <summary>
        /// Gets if a tile is hoed.
        /// </summary>
        /// <param name="location">Location of tile.</param>
        /// <param name="pos">Position of tile.</param>
        /// <returns>True if the tile is hoed, otherwise false.</returns>
        public static bool GetIsHoed(GameLocation location, Vector2 pos)
        {
            if (location.terrainFeatures.TryGetValue(pos, out TerrainFeature feature))
            {
                return feature is HoeDirt;
            }

            return false;
        }

        /// <summary>
        /// Gets if a tile is has a crop.
        /// </summary>
        /// <param name="location">Location of tile.</param>
        /// <param name="pos">Position of tile.</param>
        /// <returns>True if the tile has a crop, otherwise false.</returns>
        public static bool GetHasCrop(GameLocation location, Vector2 pos)
        {
            if (location.terrainFeatures.TryGetValue(pos, out TerrainFeature feature))
            {
                return (feature as HoeDirt)?.crop != null;
            }

            return false;
        }

        /// <summary>
        /// Gets if a position would be part of a wild tree planting pattern.
        /// </summary>
        /// <param name="pos">Tile to check.</param>
        /// <returns>True if it falls into the patter, otherwise false.</returns>
        public static bool GetIsTileInWildTreePattern(Vector2 pos)
        {
            return ModEntry.Config.WildTreePattern switch
            {
                ModConfig.WildTreePatternType.Loose => (int)pos.X % 2 == 0 && (int)pos.Y % 2 == 0,
                ModConfig.WildTreePatternType.Tight => (int)pos.X % 2 == 0,
                _ => (int)pos.X % 2 == 0 && (int)pos.Y % 2 == 0,
            };
        }

        /// <summary>
        /// Gets if a position would be part of a fruit tree planting pattern.
        /// </summary>
        /// <param name="pos">Tile to check.</param>
        /// <returns>True if it falls into the patter, otherwise false.</returns>
        public static bool GetIsTileInFruitTreePattern(Vector2 pos)
        {
            return ModEntry.Config.FruitTreePattern switch
            {
                ModConfig.FruitTreePatternType.Rows => (int)pos.X % 3 == 0 && (int)pos.Y % 3 == 0,
                ModConfig.FruitTreePatternType.Diagonal when (int)pos.X % 4 == 2 => (int)pos.Y % 4 == 2,
                ModConfig.FruitTreePatternType.Diagonal when (int)pos.X % 4 == 0 => (int)pos.Y % 4 == 0,
                ModConfig.FruitTreePatternType.Diagonal => false,
                ModConfig.FruitTreePatternType.Tight when (int)pos.Y % 2 == 0 => (int)pos.X % 4 == 0,
                ModConfig.FruitTreePatternType.Tight when (int)pos.Y % 2 == 1 => (int)pos.X % 4 == 2,
                ModConfig.FruitTreePatternType.Tight => false,
                _ => pos.X % 3 == 0 && pos.Y % 3 == 0,
            };
        }

        /// <summary>
        /// Gets all fruit tree ids.
        /// </summary>
        /// <returns>A list of all fruit tree ids.</returns>
        public static List<string> GetAllFruitTrees()
        {
            return Game1.fruitTreeData.Keys.ToList();
        }

        /// <summary>
        /// Gets if a terrain feature is a mature fruit tree.
        /// </summary>
        /// <param name="tf">The terrain feature to check.</param>
        /// <returns>True if it is a mature fruit tree, otherwise false.</returns>
        public static bool GetIsMatureFruitTree(TerrainFeature tf)
        {
            return tf is FruitTree tree && tree.growthStage.Value >= 4;
        }

        /// <summary>
        /// Gets if a terrain feature is a fruit tree sapling.
        /// </summary>
        /// <param name="tf">The terrain feature to check.</param>
        /// <returns>True if it is a fruit tree sapling, otherwise false.</returns>
        public static bool GetIsFruitTreeSapling(TerrainFeature tf)
        {
            return tf is FruitTree tree && tree.growthStage.Value < 4;
        }
    }
}