using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhoenicianJourney.Trade
{
    /// <summary>
    /// Shared harbor state for the trade loop. The dye minigame reports its result here
    /// and the cargo screen reads it.
    ///
    /// Contract for the dye minigame (Programmer A):
    /// - Call <c>TradeState.Shared.CompleteDyeTask()</c> once, when the player finishes the swatch.
    /// - Do not call it when the player leaves an attempt unfinished. The cloth then stays unavailable.
    /// - Calling it again after a completed attempt does nothing, so cargo is never duplicated.
    /// </summary>
    public sealed class TradeState
    {
        /// <summary>
        /// Proposed kickoff contract: one completed dye swatch unlocks a prepared batch of
        /// cloth on the landing. Confirm the batch size with Programmer A.
        /// </summary>
        public const int PreparedClothBundles = 4;

        /// <summary>Wine jars already staged on the landing.</summary>
        public const int StagedWineJars = 6;

        /// <summary>The state for the running game. Reset whenever play starts.</summary>
        public static TradeState Shared { get; private set; } = new TradeState();

        public event Action Changed;

        public bool DyeTaskComplete { get; private set; }
        public bool ShipmentSent { get; private set; }

        /// <summary>Goods ready to load, by product.</summary>
        public IReadOnlyDictionary<ProductId, int> Available => new Dictionary<ProductId, int>
        {
            [ProductId.PurpleCloth] = DyeTaskComplete ? PreparedClothBundles : 0,
            [ProductId.Wine] = StagedWineJars,
        };

        public int AvailableOf(ProductId product) => Available[product];

        /// <summary>Records a finished dye task. Returns false if it was already recorded.</summary>
        public bool CompleteDyeTask()
        {
            if (DyeTaskComplete)
            {
                return false;
            }

            DyeTaskComplete = true;
            Changed?.Invoke();
            return true;
        }

        public void MarkShipmentSent()
        {
            if (ShipmentSent)
            {
                return;
            }

            ShipmentSent = true;
            Changed?.Invoke();
        }

        // Static state survives between play sessions when domain reload is off, so start fresh.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetShared() => Shared = new TradeState();
    }
}
