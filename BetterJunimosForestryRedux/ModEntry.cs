namespace BetterJunimosForestryRedux
{
    using System;
    using System.Collections.Generic;
    using BetterJunimos;
    using BetterJunimosForestryRedux.Abilities;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewValley;
    using StardewValley.Buildings;

    /// <summary>
    /// The mod entry point.
    /// </summary>
    public class ModEntry : Mod
    {
        private const int IconSize = 16 * Game1.pixelZoom;
        private const int IconSpacing = 3 * Game1.pixelZoom;
        private const int IconOffset = IconSize + IconSpacing;
        private const int ScrollPaddingX = 10 * Game1.pixelZoom;
        private const int ScrollWidth = (IconOffset * 5) + (ScrollPaddingX * 2) - IconSpacing;
        private const int ScrollHeight = 26 * Game1.pixelZoom;

        private readonly Rectangle normalModeIconRect = new Rectangle(0, 0, 16, 16);
        private readonly Rectangle cropModeIconRect = new Rectangle(16, 0, 16, 16);
        private readonly Rectangle orchardModeIconRect = new Rectangle(32, 0, 16, 16);
        private readonly Rectangle forestModeIconRect = new Rectangle(48, 0, 16, 16);
        private readonly Rectangle mazeModeIconRect = new Rectangle(64, 0, 16, 16);
        private readonly Rectangle questsIconRect = new Rectangle(112, 0, 16, 16);

        private Dictionary<Rectangle, HutButton> hutButtons = new Dictionary<Rectangle, HutButton>();

        private Texture2D iconsTexture;
        private Texture2D scrollTexture;

        /// <summary>
        /// Gets the Monitor.
        /// </summary>
        public static IMonitor SMonitor { get; private set; }

        /// <summary>
        /// Gets the Better Junimos API.
        /// </summary>
        public static IBetterJunimosApi BetterJunimosApi { get; private set; }

        /// <summary>
        /// Gets the Better Generic Mod Config Menu API.
        /// </summary>
        public static IGenericModConfigMenuApi GenericModConfigMenuApi { get; private set; }

        /// <summary>
        /// Gets the config settings.
        /// </summary>
        public static ModConfig Config { get; private set; }

        /// <inheritdoc/>
        public override void Entry(IModHelper helper)
        {
            this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            this.Helper.Events.Display.RenderedWorld += this.RenderedWorld;
            this.Helper.Events.GameLoop.GameLaunched += this.OnLaunched;
            this.Helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            this.Helper.Events.GameLoop.Saving += this.OnSaving;
            this.Helper.Events.GameLoop.DayStarted += this.OnDayStarted;

            SMonitor = this.Monitor;
        }

        private void RenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            this.hutButtons.Clear();
            List<Guid> hutGuids = Utils.GetAllHutGuids();
            foreach (Guid hutGuid in hutGuids)
            {
                this.RenderHutMenu(e, hutGuid);
            }
        }

        private void RenderHutMenu(RenderedWorldEventArgs e, Guid hutGuid)
        {
            Mode mode = Utils.GetHutMode(hutGuid);

            if (!Utils.GetHutShowHud(hutGuid))
            {
                return;
            }

            JunimoHut hut = Utils.GetHutFromGuid(hutGuid);
            if (hut == null)
            {
                return;
            }

            int hutViewportX = (hut.tileX.Value * Game1.tileSize) - Game1.viewport.X;
            int hutViewportY = (hut.tileY.Value * Game1.tileSize) - Game1.viewport.Y;

            int scrollViewportX = (int)(hutViewportX + (Game1.tileSize * 1.5f) - (ScrollWidth / 2.0f));
            int scrollViewportY = (int)(hutViewportY + (Game1.tileSize * 2.25f)) - (Game1.tileSize / 3);
            int offsetY = scrollViewportY + (int)((ScrollHeight - IconSize) * 0.5f) - 2;

            Rectangle normalRect = new Rectangle(scrollViewportX + ScrollPaddingX, offsetY, IconSize, IconSize);
            Rectangle cropsRect = new Rectangle(scrollViewportX + ScrollPaddingX + IconOffset, offsetY, IconSize, IconSize);
            Rectangle orchardRect = new Rectangle(scrollViewportX + ScrollPaddingX + (IconOffset * 2), offsetY, IconSize, IconSize);
            Rectangle forestRect = new Rectangle(scrollViewportX + ScrollPaddingX + (IconOffset * 3), offsetY, IconSize, IconSize);
            Rectangle questsRect = new Rectangle(scrollViewportX + ScrollPaddingX + (IconOffset * 4), offsetY, IconSize, IconSize);

            this.hutButtons[normalRect] = new HutButton(hutGuid, Mode.Normal);
            this.hutButtons[cropsRect] = new HutButton(hutGuid, Mode.Crops);
            this.hutButtons[orchardRect] = new HutButton(hutGuid, Mode.Orchard);
            this.hutButtons[forestRect] = new HutButton(hutGuid, Mode.Forest);
            this.hutButtons[questsRect] = new HutButton(hutGuid);

            this.DrawScroll(e.SpriteBatch, new Vector2(scrollViewportX, scrollViewportY));
            e.SpriteBatch.Draw(this.iconsTexture, normalRect, this.normalModeIconRect, Color.White * (mode == Mode.Normal ? 1.0f : 0.25f));
            e.SpriteBatch.Draw(this.iconsTexture, cropsRect, this.cropModeIconRect, Color.White * (mode == Mode.Crops ? 1.0f : 0.25f));
            e.SpriteBatch.Draw(this.iconsTexture, orchardRect, this.orchardModeIconRect, Color.White * (mode == Mode.Orchard ? 1.0f : 0.25f));
            e.SpriteBatch.Draw(this.iconsTexture, forestRect, this.forestModeIconRect, Color.White * (mode == Mode.Forest ? 1.0f : 0.25f));
            e.SpriteBatch.Draw(this.iconsTexture, questsRect, this.questsIconRect, Color.White);
        }

        private void DrawScroll(SpriteBatch spriteBatch, Vector2 position)
        {
            const float layerDepth = 0.88f;
            spriteBatch.Draw(
                this.scrollTexture,
                new Rectangle((int)position.X, (int)position.Y, ScrollWidth, ScrollHeight),
                new Rectangle(0, 0, 144, 24),
                Color.White,
                0.0f,
                Vector2.Zero,
                SpriteEffects.None,
                layerDepth - 0.001f);
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
            {
                return;
            }

            if (Game1.activeClickableMenu != null)
            {
                return;
            }

            if (!e.Button.IsUseToolButton())
            {
                return;
            }

            if (this.HandleMenuClick(e))
            {
                return;
            }

            Guid hutGuid = Guid.Empty;
            foreach (Guid guid in Utils.GetAllHutGuids())
            {
                JunimoHut hut = Utils.GetHutFromGuid(guid);
                if (hut.occupiesTile(e.Cursor.Tile))
                {
                    hutGuid = guid;
                    break;
                }
            }

            if (hutGuid == Guid.Empty)
            {
                return;
            }

            Utils.SetHutShowHud(hutGuid, !Utils.GetHutShowHud(hutGuid));
            Game1.playSound("junimoMeep1");

            this.Helper.Input.Suppress(e.Button);
        }

        private bool HandleMenuClick(ButtonPressedEventArgs e)
        {
            foreach ((Rectangle rect, HutButton hutButton) in this.hutButtons)
            {
                if (!rect.Contains((int)e.Cursor.ScreenPixels.X, (int)e.Cursor.ScreenPixels.Y))
                {
                    continue;
                }

                this.Helper.Input.Suppress(e.Button);
                JunimoHut hut = Utils.GetHutFromGuid(hutButton.Guid);
                Game1.playSound("junimoMeep1");

                if (hutButton.IsQuestButton)
                {
                    BetterJunimosApi.ShowPerfectionTracker();
                }
                else
                {
                    Utils.SetHutMode(hutButton.Guid, hutButton.Mode);
                }

                return true;
            }

            return false;
        }

        private void OnLaunched(object sender, GameLaunchedEventArgs e)
        {
            Config = this.Helper.ReadConfig<ModConfig>();

            GenericModConfigMenuApi = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            this.SetupGenericModConfigMenuApi();

            BetterJunimosApi = this.Helper.ModRegistry.GetApi<IBetterJunimosApi>("hawkfalcon.BetterJunimos");
            if (BetterJunimosApi == null)
            {
                this.Monitor.Log($"Could not load Better Junimos API", LogLevel.Error);
            }

            this.iconsTexture = this.Helper.ModContent.Load<Texture2D>("assets/icons.png");
            this.scrollTexture = this.Helper.ModContent.Load<Texture2D>("assets/scroll.png");
        }

        private void SetupGenericModConfigMenuApi()
        {
            if (GenericModConfigMenuApi == null)
            {
                return;
            }

            GenericModConfigMenuApi.Register(this.ModManifest, () => Config = new ModConfig(), () => this.Helper.WriteConfig(Config));

            GenericModConfigMenuApi.AddBoolOption(
                this.ModManifest,
                () => Config.HarvestGrassEnabled,
                (val) => Config.HarvestGrassEnabled = val,
                () => "Harvest Grass");

            GenericModConfigMenuApi.AddBoolOption(
                this.ModManifest,
                () => Config.SustainableWildTreeHarvesting,
                (val) => Config.SustainableWildTreeHarvesting = val,
                () => "Sustainable tree harvesting",
                () => "Only harvest wild trees when they've grown a seed");

            GenericModConfigMenuApi.AddTextOption(
                this.ModManifest,
                () => Config.WildTreePattern.ToString(),
                (val) =>
                {
                    if (Enum.TryParse(val, out ModConfig.WildTreePatternType parsedEnum))
                    {
                        Config.WildTreePattern = parsedEnum;
                    }
                },
                () => "Wild tree pattern",
                allowedValues: Enum.GetNames<ModConfig.WildTreePatternType>());

            GenericModConfigMenuApi.AddTextOption(
                this.ModManifest,
                () => Config.FruitTreePattern.ToString(),
                (val) =>
                {
                    if (Enum.TryParse(val, out ModConfig.FruitTreePatternType parsedEnum))
                    {
                        Config.FruitTreePattern = parsedEnum;
                    }
                },
                () => "Fruit tree pattern",
                allowedValues: Enum.GetNames<ModConfig.FruitTreePatternType>());
        }

        private void OnSaveLoaded(object sender, EventArgs e)
        {
            Config = this.Helper.ReadConfig<ModConfig>();
            this.RegisterAbilities();
        }

        private void RegisterAbilities()
        {
            BetterJunimosApi.RegisterJunimoAbility(new ChopWildTreesAbility());
            BetterJunimosApi.RegisterJunimoAbility(new CollectDroppedObjectsAbility());
            BetterJunimosApi.RegisterJunimoAbility(new CollectSeedsAbility());
            BetterJunimosApi.RegisterJunimoAbility(new PlantTreesAbility());
            BetterJunimosApi.RegisterJunimoAbility(new HarvestDebrisAbility());
            BetterJunimosApi.RegisterJunimoAbility(new HarvestGrass());
            BetterJunimosApi.RegisterJunimoAbility(new FertilizeTreesAbility());
            BetterJunimosApi.RegisterJunimoAbility(new HarvestFruitTreesAbility());
            BetterJunimosApi.RegisterJunimoAbility(new PlantFruitTreesAbility());
            BetterJunimosApi.RegisterJunimoAbility(new HoeDirtAbility());
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            if (!Context.IsMainPlayer)
            {
                return;
            }

            this.Helper.WriteConfig(Config);
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // Reset for rainy days, winter, or config changes
            this.Helper.GameContent.InvalidateCache(@"Characters\Junimo");
        }

        private class HutButton
        {
            public HutButton(Guid guid, Mode mode)
            {
                this.Guid = guid;
                this.Mode = mode;
                this.IsQuestButton = false;
            }

            public HutButton(Guid guid)
            {
                this.Guid = guid;
                this.IsQuestButton = true;
            }

            public Guid Guid { get; set; }

            public Mode Mode { get; set; }

            public bool IsQuestButton { get; set; }
        }
    }
}