namespace BetterJunimosForestryRedux.Abilities
{
    using System;
    using System.Collections.Generic;
    using BetterJunimos.Abilities;
    using Microsoft.Xna.Framework;
    using StardewValley;
    using StardewValley.Characters;
    using StardewValley.Objects;
    using StardewValley.Tools;

    /// <summary>
    /// Allows junimos to harvest debris.
    /// </summary>
    public class HarvestDebrisAbility : IJunimoAbility
    {
        private FakeFarmer fakeFarmer = new FakeFarmer();
        private Pickaxe fakePickaxe = new Pickaxe()
        {
            UpgradeLevel = 1,
            IsEfficient = true,
        };

        private Axe fakeAxe = new Axe()
        {
            UpgradeLevel = 1,
            IsEfficient = true,
        };

        private MeleeWeapon fakeScythe = new MeleeWeapon("47")
        {
            IsEfficient = true,
        };

        /// <inheritdoc/>
        public string AbilityName()
        {
            return "HarvestDebris";
        }

        /// <inheritdoc/>
        public bool IsActionAvailable(GameLocation location, Vector2 pos, Guid hutGuid)
        {
            bool foundDebris = false;
            Utils.ForEachDirection(pos, (p) =>
            {
                if (!foundDebris && Utils.GetIsTileInHutRadius(hutGuid, p) && location.objects.ContainsKey(p) && this.GetIsDebris(location.objects[p]))
                {
                    foundDebris = true;
                }
            });

            return foundDebris;
        }

        /// <inheritdoc/>
        public bool IsActionAvailable(Farm farm, Vector2 pos, Guid hutGuid)
        {
            return this.IsActionAvailable((GameLocation)farm, pos, hutGuid);
        }

        /// <inheritdoc/>
        public bool PerformAction(GameLocation location, Vector2 pos, JunimoHarvester junimo, Guid hutGuid)
        {
            bool harvestedDebris = false;
            int direction = 0;
            Utils.ForEachDirection(pos, (p) =>
            {
                if (!harvestedDebris && Utils.GetIsTileInHutRadius(hutGuid, p) && location.objects.ContainsKey(p) && this.GetIsDebris(location.objects[p]))
                {
                    StardewValley.Object item = location.objects[p];
                    if (this.GetIsStone(item))
                    {
                        Utils.UseToolOnTile(this.fakePickaxe, this.fakeFarmer, location, p);
                    }
                    else if (this.GetIsTwig(item))
                    {
                        Utils.UseToolOnTile(this.fakeAxe, this.fakeFarmer, location, p);
                    }
                    else if (this.GetIsWeed(item))
                    {
                        Utils.UseToolOnTile(this.fakeScythe, this.fakeFarmer, location, p);
                        item.performToolAction(this.fakeAxe);
                        location.removeObject(p, false);
                    }

                    junimo.faceDirection(direction);
                    harvestedDebris = true;
                }

                direction += 1;
            });

            return harvestedDebris;
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

        private bool GetIsDebris(StardewValley.Object obj)
        {
            return this.GetIsTwig(obj) || this.GetIsWeed(obj) || this.GetIsStone(obj);
        }

        private bool GetIsTwig(StardewValley.Object obj)
        {
            return obj is not Chest && obj?.Name == "Twig";
        }

        private bool GetIsWeed(StardewValley.Object obj)
        {
            return obj is not Chest && obj?.Name == "Weeds";
        }

        private bool GetIsStone(StardewValley.Object obj)
        {
            return obj is not Chest && obj?.Name == "Stone";
        }
    }
}