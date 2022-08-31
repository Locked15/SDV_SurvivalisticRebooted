using StardewModdingAPI;
using StardewValley;
using Survivalistic.Framework.Interfaces;
using System;
using static DaLion.Common.Integrations.WalkOfLife.IImmersiveProfessions;

namespace Survive_Net5.Framework.Integrations
{
    public class WalkOfLifeIntegration
    {
        #region Private Fields.

        /// <summary>
        /// Private field. Needed to make 'Instance' property work correctly.
        /// </summary>
        private static WalkOfLifeIntegration instance;
        #endregion

        #region Non-Static Properties.

        /// <summary>
        /// Cause 'Walk Of Life' create new bar, so we need to offset need bars little bit left.
        /// </summary>
        public IImmersiveProfessionsAPI WalkOfLifeAPI { get; private set; }

        /// <summary>
        /// Current ultimate ability of the player.
        /// </summary>
        public IUltimate CurrentUltimateAbility { get; private set; }

        /// <summary>
        /// Determine, visible or not in current moment ultimate gauge meter.
        /// </summary>
        public bool UltimateBarIsCurrentlyVisible { get; private set; }
        #endregion

        #region Interaction Interface.

        /// <summary>
        /// Instance of "WalkOfLifeIntegration". Creates united interface of interaction with this mod compatibility. 
        /// <br /> <br />
        /// If compatibility setting equal 'false', this property will return null.
        /// </summary>
        public static WalkOfLifeIntegration Instance
        {
            get
            {
                if (Survivalistic.ModEntry.config.walk_of_life_support)
                    return instance;

                else
                    return null;
            }

            private set
            {
                instance = value;
            }
        }

        /// <summary>
        /// Static constructor.
        /// </summary>
        static WalkOfLifeIntegration()
        {
            Instance = new();
        }
        #endregion

        #region Functions.

        /// <summary>
        /// Initialize API.
        /// </summary>
        /// <param name="instance">Current mod instance to get access to between-mods interaction interface.</param>
        public void InitializeWalkOfLifeAPI(Mod instance)
        {
            try
            {
                WalkOfLifeAPI = instance.Helper.ModRegistry.GetApi<IImmersiveProfessionsAPI>("DaLion.ImmersiveProfessions");

                if (WalkOfLifeAPI != null)
                    instance.Monitor.Log("Walk of Life API initialized.", LogLevel.Info);

                InitializeSubscriptionForEvents();
            }

            catch (Exception ex)
            {
                instance.Monitor.Log($"Walk Of Life API initialization error. Exception occurred: {ex.Message}.", LogLevel.Warn);
                WalkOfLifeAPI = null;
            }
        }

        /// <summary>
        /// Initializes new day, by updating current ultimate ability info.
        /// </summary>
        public void UpdateAbilityOnDayStarted() =>
                    CurrentUltimateAbility = WalkOfLifeAPI?.GetRegisteredUltimate(Game1.player) ?? null;

        /// <summary>
        /// Initializes new events, to update properties with info.
        /// </summary>
        private void InitializeSubscriptionForEvents()
        {
            var increaseChargeEvent = WalkOfLifeAPI.RegisterUltimateChargeIncreasedEvent((val, val2) =>
            {
                UltimateBarIsCurrentlyVisible = CurrentUltimateAbility?.IsHudVisible ?? false;
            }, true);

            var chargeEmptiedEvent = WalkOfLifeAPI.RegisterUltimateEmptiedEvent((val, val2) =>
            {
                UltimateBarIsCurrentlyVisible = false;
            }, true);
        }
        #endregion
    }
}
