namespace BetterJunimosRedux.Abilities
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
        private Hoe fakeHoke = new Hoe()
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
            Mode mode = Utils.GetHutMode(hutGuid);
            if (mode != Mode.Normal && Utils.GetTileIsInHutRadius(hutGuid, pos) && this.GetShouldHoe(location, pos, hutGuid))
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
            Mode mode = Utils.GetHutMode(hutGuid);
            if (mode != Mode.Normal && Utils.GetTileIsInHutRadius(hutGuid, pos) && this.GetShouldHoe(location, pos, hutGuid))
            {
                Utils.UseToolOnTile(this.fakeHoke, this.fakeFarmer, location, pos);
                return true;
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

        private bool GetShouldHoe(GameLocation location, Vector2 pos, Guid hutGuid)
        {
            if (!Utils.GetCanBeHoed(location, pos))
            {
                return false;
            }

            if (Utils.GetIsHoed(location, pos))
            {
                return false;
            }

            Mode mode = Utils.GetHutMode(hutGuid);

            if (mode == Mode.Orchard && PlantFruitTreesAbility.GetShouldPlantFruitTreeHere(hutGuid, location, pos))
            {
                return false;
            }

            if (mode == Mode.Forest && PlantTreesAbility.GetShouldPlantWildTreeHere(hutGuid, location, pos))
            {
                return false;
            }

            for (int x = -1; x < 2; x += 1)
            {
                for (int y = -1; y < 2; y += 1)
                {
                    var tile = new Vector2(pos.X + x, pos.Y + y);
                    if (location.terrainFeatures.ContainsKey(tile) && Utils.GetIsFruitTreeSapling(location.terrainFeatures[tile]))
                    {
                        return false;
                    }
                }
            }

            if (mode == Mode.Forest)
            {
                return true;
            }

            if (!this.GetIsSeedAvailableForTile(location, pos, hutGuid))
            {
                return false;
            }

            if (mode != Mode.Orchard)
            {
                return true;
            }

            for (int x = -1; x < 2; x += 1)
            {
                for (int y = -1; y < 2; y += 1)
                {
                    Vector2 tile = new Vector2(pos.X + x, pos.Y + y);
                    if (location.terrainFeatures.ContainsKey(tile) && Utils.GetIsMatureFruitTree(location.terrainFeatures[tile]))
                    {
                        return true;
                    }
                }
            }

            return false;
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
            List<Item> foundItems = chest.Items.ToList().FindAll(item => item.Category == StardewValley.Object.SeedsCategory && !Utils.GetAllWildTreeSeeds().Contains(item.ItemId));
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