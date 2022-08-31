using DaLion.Common.Integrations.WalkOfLife;
using DaLion.Stardew.Professions.Framework.Events.Ultimate;
using StardewValley;
using System;

namespace Survivalistic.Framework.Interfaces
{
    /// <summary>Interface for the Immersive Professions' API.</summary>
    /// <remarks>Version 5.0.2</remarks>
    public interface IImmersiveProfessionsAPI
    {
        /// <summary>
        /// Get a player's currently registered combat Ultimate, if any.
        /// </summary>
        IImmersiveProfessions.IUltimate GetRegisteredUltimate(Farmer farmer = null);

        /// <summary>Register a new <see cref="UltimateChargeIncreasedEvent"/> instance.</summary>
        /// <param name="callback">The delegate that will be called when the event is triggered.</param>
        /// <param name="alwaysEnabled">Whether the event should be allowed to override the <c>enabled</c> flag.</param>
        IImmersiveProfessions.IManagedEvent RegisterUltimateChargeIncreasedEvent(Action<object, IImmersiveProfessions.IUltimateChargeIncreasedEventArgs> callback, bool alwaysEnabled = false);

        /// <summary>Register a new <see cref="UltimateEmptiedEvent"/> instance.</summary>
        /// <param name="callback">The delegate that will be called when the event is triggered.</param>
        /// <param name="alwaysEnabled">Whether the event should be allowed to override the <c>enabled</c> flag.</param>
        IImmersiveProfessions.IManagedEvent RegisterUltimateEmptiedEvent(Action<object, IImmersiveProfessions.IUltimateEmptiedEventArgs> callback, bool alwaysEnabled = false);
    }
}
