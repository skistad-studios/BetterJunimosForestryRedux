namespace BetterJunimosForestryRedux.Abilities
{
    using System;
    using System.Collections.Generic;
    using BetterJunimos.Abilities;
    using BetterJunimosForestryRedux;
    using Microsoft.Xna.Framework;
    using StardewValley;
    using StardewValley.Characters;

    /// <summary>
    /// Allows junimos to collect items off the ground.
    /// </summary>
    public class CollectDroppedObjectsAbility : IJunimoAbility
    {
        /// <inheritdoc/>
        public string AbilityName()
        {
            return "CollectDroppedObjects";
        }

        /// <inheritdoc/>
        public bool IsActionAvailable(GameLocation location, Vector2 pos, Guid hutGuid)
        {
            if (Utils.GetIsTileInHutRadius(hutGuid, location, pos) && this.GetDebrisAtTile(location, pos).Count > 0)
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
                List<Debris> debris = this.GetDebrisAtTile(location, pos);
                if (debris.Count > 0)
                {
                    return this.MoveDebrisToHut(hutGuid, location, debris);
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

        private List<Debris> GetDebrisAtTile(GameLocation location, Vector2 tile)
        {
            List<Debris> debrisList = new List<Debris>();
            foreach (Debris debris in location.debris)
            {
                foreach (Chunk chunk in debris.Chunks)
                {
                    Vector2 debrisPos = new Vector2(chunk.position.X / Game1.tileSize, chunk.position.Y / Game1.tileSize);
                    if (Vector2.Distance(tile, debrisPos) <= 1.0f)
                    {
                        Item item = debris.item ?? (debris.itemId.Value != null ? ItemRegistry.Create(debris.itemId.Value) : null);
                        if (item != null && item is StardewValley.Object)
                        {
                            debrisList.Add(debris);
                        }
                    }
                }
            }

            return debrisList;
        }

        private bool MoveDebrisToHut(Guid hutGuid, GameLocation location, List<Debris> debris)
        {
            if (debris == null)
            {
                return false;
            }

            bool movedDebris = false;
            foreach (Debris d in debris)
            {
                Item item = d.item ?? (d.itemId.Value != null ? ItemRegistry.Create(d.itemId.Value) : null);
                if (item != null && item is StardewValley.Object)
                {
                    Utils.AddItemToHut(hutGuid, (StardewValley.Object)item);
                    location.debris.Remove(d);
                    movedDebris = true;
                }
            }

            if (movedDebris)
            {
                location.playSound("pickUpItem");
            }

            return movedDebris;
        }
    }
}