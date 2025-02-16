﻿using StardewModdingAPI;
using StardewModdingAPI.Events;
using Survive_Net5.Framework.Bars;
using Survive_Net5.Framework.Common;
using Survive_Net5.Framework.Common.Affection;
using Survive_Net5.Framework.Databases;
using Survive_Net5.Framework.Integrations;
using Survive_Net5.Framework.Interfaces;
using Survive_Net5.Framework.Networking;
using Survive_Net5.Framework.Rendering;

namespace Survive_Net5
{
    public class ModEntry : Mod
    {
        public static ModEntry instance;
        internal static Data data;
        public static Config config;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            config = Helper.ReadConfig<Config>();

            Textures.LoadTextures();

            helper.Events.GameLoop.GameLaunched += OnGameLaunch;
            helper.Events.GameLoop.UpdateTicked += OnUpdate;
            helper.Events.GameLoop.TimeChanged += OnTimeChanged;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += OnDayStarted;

            helper.Events.Multiplayer.PeerConnected += OnPlayerConnected;
            helper.Events.Multiplayer.ModMessageReceived += OnMessageReceived;

            helper.Events.Display.RenderingHud += Renderer.OnRenderingHud;
            helper.Events.GameLoop.ReturnedToTitle += OnReturnToTitle;

            if (config.walk_of_life_support)
                helper.Events.GameLoop.DayStarted += UpdateWalkOfLifeUltimateOnDayStarted;

            helper.ConsoleCommands.Add("survivalistic_feed", "Feeds a player.\nUsage: survivalistic_feed 'player_name'", Commands.Feed);
            helper.ConsoleCommands.Add("survivalistic_hydrate", "Hydrates a player.\nUsage: survivalistic_hydrate 'player_name'", Commands.Hydrate);
            helper.ConsoleCommands.Add("survivalistic_fullness", "Set full status to a player.\nUsage: survivalistic_fullness 'player_name'", Commands.Fullness);
            helper.ConsoleCommands.Add("survivalistic_forcesync", "Forces the synchronization in multiplayer to all players.\nUsage: survivalistic_forcesync", Commands.ForceSync);

            DBController.LoadDatabases();
        }

        private void OnGameLaunch(object sender, GameLaunchedEventArgs e)
        {
            if (config.walk_of_life_support)
                WalkOfLifeIntegration.Instance.InitializeWalkOfLifeAPI(this);

            bool result = InitializeModMenu();
            string message = result ? "Generic Mod Menu successfully loaded for this mod!" :
                                      "Generic Mod Menu isn't found... skip.";

            Monitor.Log(message, LogLevel.Info);
        }

        private void UpdateWalkOfLifeUltimateOnDayStarted(object sender, DayStartedEventArgs e) =>
                     WalkOfLifeIntegration.Instance.UpdateAbilityOnDayStarted();

        private void OnReturnToTitle(object sender, ReturnedToTitleEventArgs e) =>
                     NetController.firstLoad = false;

        private void OnUpdate(object sender, UpdateTickedEventArgs e)
        {
            Interaction.EatingCheck();
            Interaction.UsingToolCheck();
            BarsPosition.SetBarsPosition();
            Interaction.UpdateTickInformation();
            Penalty.VerifyPassOut();
        }

        private void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            BarsUpdate.UpdateBarsInformation();
            BarsUpdate.CalculatePercentage();
            BarsWarnings.VerifyStatus();
            Penalty.VerifyPenalty();
            NetController.Sync();
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            data.actual_hunger -= config.food_decrease_after_sleep;
            data.actual_thirst -= config.thirst_decrease_after_sleep;

            OnUpdate(default, default);
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!NetController.firstLoad) NetController.Sync();
            Interaction.Awake();
            NetController.Sync();
            BarsPosition.SetBarsPosition();
            Interaction.ReceiveAwakeInfos();
            BarsUpdate.CalculatePercentage();
            BarsWarnings.VerifyStatus();
        }

        private void OnPlayerConnected(object sender, PeerConnectedEventArgs e) =>
                     NetController.SyncSpecificPlayer(e.Peer.PlayerID);

        private void OnMessageReceived(object sender, ModMessageReceivedEventArgs e) =>
                     NetController.OnMessageReceived(e);

        private bool InitializeModMenu()
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

            if (configMenu is null)
            {
                return false;
            }

            else
            {
                // Register mod.
                configMenu.Register(
                    mod: ModManifest,
                    reset: () => config = new Config(),
                    save: () => Helper.WriteConfig(config)
                );

                #region Multiplier settings.

                // Title.
                configMenu.AddSectionTitle(
                    mod: ModManifest,
                    text: () => Helper.Translation.Get("multiplier-settings"),
                    tooltip: () => Helper.Translation.Get("multiplier-settings-des")
                );

                // Main hunger multiplier.
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("main-hunger-setting"),
                    tooltip: () => Helper.Translation.Get("main-hunger-setting-des"),
                    getValue: () => config.hunger_multiplier,
                    setValue: value => config.hunger_multiplier = value,
                    min: 0.0F,
                    max: 5.0F
                );

                // Main thirst multiplier.
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("main-thirst-setting"),
                    tooltip: () => Helper.Translation.Get("main-thirst-setting-des"),
                    getValue: () => config.thirst_multiplier,
                    setValue: value => config.thirst_multiplier = value,
                    min: 0.0F,
                    max: 5.0F
                );

                // Action hunger multiplier.
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("action-hunger-setting"),
                    tooltip: () => Helper.Translation.Get("action-hunger-setting-des"),
                    getValue: () => config.hunger_action_multiplier,
                    setValue: value => config.hunger_action_multiplier = value,
                    min: 0.0F,
                    max: 5.0F
                );

                // Action thirst multiplier.
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("action-thirst-setting"),
                    tooltip: () => Helper.Translation.Get("action-thirst-setting-des"),
                    getValue: () => config.thirst_action_multiplier,
                    setValue: value => config.thirst_action_multiplier = value,
                    min: 0.0F,
                    max: 5.0F
                );
                #endregion

                #region Bars options.

                // Title.
                configMenu.AddSectionTitle(
                    mod: ModManifest,
                    text: () => Helper.Translation.Get("bars-position-settings"),
                    tooltip: () => Helper.Translation.Get("bars-position-settings-des")
                );

                // Bar positioning layout.
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("bars-position-variant"),
                    tooltip: () => Helper.Translation.Get("bars-position-variant-des"),
                    getValue: () => config.bars_position,
                    setValue: value => config.bars_position = value
                );

                // X position setting.
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("bars-position-x"),
                    tooltip: () => Helper.Translation.Get("bars-position-x-des"),
                    getValue: () => config.bars_custom_x,
                    setValue: value => config.bars_custom_x = value
                );

                // Y position setting.
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("bars-position-y"),
                    tooltip: () => Helper.Translation.Get("bars-position-y-des"),
                    getValue: () => config.bars_custom_y,
                    setValue: value => config.bars_custom_y = value
                );
                #endregion

                #region Compatibility settings.

                // Title.
                configMenu.AddSectionTitle(
                    mod: ModManifest,
                    text: () => Helper.Translation.Get("compatibility-settings"), //TODO: Update translation.
                    tooltip: () => Helper.Translation.Get("compatibility-settings-des")
                );

                // General compatibility setting.
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("walk-of-life-support"),
                    tooltip: () => Helper.Translation.Get("walk-of-life-support-des"),
                    getValue: () => config.walk_of_life_support,
                    setValue: value => config.walk_of_life_support = value
                );

                // Colored bar.
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("wof-colored-special"),
                    tooltip: () => Helper.Translation.Get("wof-colored-special-des"),
                    getValue: () => config.wof_colored_bar,
                    setValue: value => config.wof_colored_bar = value
                );

                // Food support bar.
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("non-supported-food"),
                    tooltip: () => Helper.Translation.Get("non-supported-food-des"),
                    getValue: () => config.non_supported_food,
                    setValue: value => config.non_supported_food = value
                );
                #endregion

                #region Sleep options.

                // Title.
                configMenu.AddSectionTitle(
                    mod: ModManifest,
                    text: () => Helper.Translation.Get("sleep-options")
                );

                // Main Setting.
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("sleep-decrease"),
                    tooltip: () => Helper.Translation.Get("sleep-decrease-des"),
                    getValue: () => config.decrease_values_after_sleep,
                    setValue: value => config.decrease_values_after_sleep = value
                );

                // Hunger.
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("hunger-increase-after-sleep"),
                    tooltip: () => Helper.Translation.Get("hunger-increase-after-sleep-des"),
                    getValue: () => config.food_decrease_after_sleep,
                    setValue: value => config.food_decrease_after_sleep = value,
                    min: -100,
                    max: 100
                );

                // Thirst.
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("thirst-increase-after-sleep"),
                    tooltip: () => Helper.Translation.Get("thirst-increase-after-sleep-des"),
                    getValue: () => config.thirst_decrease_after_sleep,
                    setValue: value => config.thirst_decrease_after_sleep = value,
                    min: -100,
                    max: 100
                );
                #endregion

                return true;
            }
        }
    }
}
