namespace BetterJunimosForestryRedux.Abilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BetterJunimos;
    using BetterJunimos.Abilities;
    using Microsoft.Xna.Framework;
    using StardewValley;
    using StardewValley.Characters;
    using StardewValley.GameData.Crops;
    using StardewValley.Objects;
    using StardewValley.Tools;

    /// <summary>
    /// Allows junimos to harvest debris.
    /// </summary>
    public class HoeDirtAbility : IJunimoAbility
    {
        private const string MixedSeedsId = "770";
        private const string MixedFlowerSeedsId = "MixedFlowerSeeds";

        private Hoe fakeHoe = new Hoe()
        {
            UpgradeLevel = 1,
            IsEfficient = true,
        };

        private Pickaxe fakePickaxe = new Pickaxe()
        {
            UpgradeLevel = 1,
            IsEfficient = true,
        };

        private FakeFarmer fakeFarmer = new FakeFarmer();

        /// <inheritdoc/>
        public string AbilityName()
        {
            return "HoeDirt";
        }

        /// <inheritdoc/>
        public bool IsActionAvailable(GameLocation location, Vector2 pos, Guid hutGuid)
        {
            if (Utils.GetIsTileInHutRadius(hutGuid, location, pos) && this.GetToolToUse(location, pos, hutGuid) != null)
            {
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public bool IsActionAvailable(Farm farm, Vector2 pos, Guid hutGuid)
        {
            return this.IsActionAvailable((GameLocation)farm, pos, hutGuid);
        }

        /// <inheritdoc/>
        public bool PerformAction(GameLocation location, Vector2 pos, JunimoHarvester junimo, Guid hutGuid)
        {
            if (Utils.GetIsTileInHutRadius(hutGuid, location, pos))
            {
                Tool tool = this.GetToolToUse(location, pos, hutGuid);
                if (tool != null)
                {
                    Utils.UseToolOnTile(tool, this.fakeFarmer, location, pos);
                    return true;
                }
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

        private Tool GetToolToUse(GameLocation location, Vector2 pos, Guid hutGuid)
        {
            if (!Utils.GetCanBeHoed(location, pos))
            {
                return null;
            }

            Mode mode = Utils.GetHutMode(hutGuid);
            if (mode == Mode.Orchard && Utils.GetIsTileInFruitTreePattern(pos))
            {
                if (Utils.GetIsHoed(location, pos))
                {
                    return this.fakePickaxe;
                }
                else
                {
                    return null;
                }
            }

            if (mode == Mode.Forest && Utils.GetIsTileInWildTreePattern(pos))
            {
                if (Utils.GetIsHoed(location, pos))
                {
                    return this.fakePickaxe;
                }
                else
                {
                    return null;
                }
            }

            for (int x = -1; x < 2; x += 1)
            {
                for (int y = -1; y < 2; y += 1)
                {
                    var tile = new Vector2(pos.X + x, pos.Y + y);
                    if (location.terrainFeatures.ContainsKey(tile) && Utils.GetIsFruitTreeSapling(location.terrainFeatures[tile]))
                    {
                        if (Utils.GetIsHoed(location, pos))
                        {
                            return this.fakePickaxe;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }

            if (mode == Mode.Forest)
            {
                if (!Utils.GetIsHoed(location, pos))
                {
                    return this.fakeHoe;
                }
                else
                {
                    return null;
                }
            }

            if (mode == Mode.Orchard)
            {
                for (int x = -1; x < 2; x += 1)
                {
                    for (int y = -1; y < 2; y += 1)
                    {
                        Vector2 tile = new Vector2(pos.X + x, pos.Y + y);
                        if (location.terrainFeatures.ContainsKey(tile) && Utils.GetIsMatureFruitTree(location.terrainFeatures[tile]))
                        {
                            if (!Utils.GetIsHoed(location, pos))
                            {
                                return this.fakeHoe;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
            }

            if (mode != Mode.Normal && this.GetIsSeedAvailableForTile(location, pos, hutGuid))
            {
                if (!Utils.GetIsHoed(location, pos))
                {
                    return this.fakeHoe;
                }
                else
                {
                    return null;
                }
            }

            if (Utils.GetIsHoed(location, pos))
            {
                return ModEntry.Config.CleanUnneededHoedDirt ? this.fakePickaxe : null;
            }
            else
            {
                return null;
            }
        }

        private bool GetIsSeedAvailableForTile(GameLocation location, Vector2 pos, Guid hutGuid)
        {
            Chest chest = Utils.GetHutFromGuid(hutGuid).GetOutputChest();
            if (ModEntry.BetterJunimosApi.GetCropMapForHut(hutGuid) == null)
            {
                return this.GetHasPlantableSeed(location, chest);
            }
            else
            {
                string cropType = ModEntry.BetterJunimosApi.GetCropMapForHut(hutGuid).CropTypeAt(Utils.GetHutFromGuid(hutGuid), pos);
                return this.GetHasPlantableSeed(location, chest, cropType);
            }
        }

        private bool GetHasPlantableSeed(GameLocation location, Chest chest, string cropType = null)
        {
            List<Item> foundItems = chest.Items
                .ToList()
                .FindAll(
                item => item.Category == StardewValley.Object.SeedsCategory &&
                !Utils.GetAllWildTreeSeeds().Contains(item.ItemId) &&
                item.itemId.Value != MixedSeedsId &&
                item.itemId.Value != MixedFlowerSeedsId);

            if (cropType == CropTypes.Trellis)
            {
                foundItems = foundItems.FindAll(item => this.GetIsTrellisCrop(item));
            }
            else if (cropType == CropTypes.Ground)
            {
                foundItems = foundItems.FindAll(item => !this.GetIsTrellisCrop(item));
            }

            if (foundItems.Count == 0)
            {
                return false;
            }

            if (location.IsGreenhouse || location.InIslandContext())
            {
                return true;
            }

            foreach (Item foundItem in foundItems)
            {
                string seedId = Crop.ResolveSeedId(foundItem.ItemId, location);
                if (Crop.TryGetData(seedId, out var cropData))
                {
                    if (cropData.Seasons.Contains(location.GetSeason()))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool GetIsTrellisCrop(Item item)
        {
            Game1.cropData.TryGetValue(item.ItemId, out CropData cropData);
            return cropData != null && cropData.IsRaised;
        }
    }
}