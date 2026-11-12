using System.Collections;
using System.Linq;
using NUnit.Framework;
using PhoenicianJourney.Landing;
using PhoenicianJourney.Player;
using PhoenicianJourney.Trade;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PhoenicianJourney.Tests
{
    /// <summary>
    /// Plays the generated harbor scene: the player lands on the ground, the dye task unlocks
    /// the prepared cloth, and the cargo screen accepts only the shipment the letter describes.
    /// Rebuild the scene first (Tools ▸ Phoenician Journey ▸ Rebuild Harbor Scene).
    /// </summary>
    public sealed class HarborLoopTests
    {
        private FirstPersonWalker player;
        private HarborGame game;

        [UnitySetUp]
        public IEnumerator LoadHarbor()
        {
            yield return SceneManager.LoadSceneAsync("Harbor", LoadSceneMode.Single);
            player = Object.FindAnyObjectByType<FirstPersonWalker>();
            game = Object.FindAnyObjectByType<HarborGame>();
            Assert.That(player, Is.Not.Null, "The harbor scene has no player.");
            Assert.That(game, Is.Not.Null, "The harbor scene has no HarborGame.");
            yield return new WaitForSeconds(1f);
        }

        [UnityTest]
        public IEnumerator PlayerSettlesOnTheGroundAtSpawn()
        {
            Assert.That(player.Grounded, Is.True, "The player should be standing on the landing.");
            Assert.That(player.Feet.y, Is.GreaterThan(-1f), "The player fell through the landing.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator FindsTheCargoAndDyeStations()
        {
            var ids = Station.Active.Select(s => s.Id).ToList();
            Assert.That(ids, Does.Contain(StationId.Cargo));
            Assert.That(ids, Does.Contain(StationId.Dye));
            yield return null;
        }

        [UnityTest]
        public IEnumerator CompletesTheTradeLoop()
        {
            yield return StandNextTo(StationId.Dye);
            Assert.That(game.NearbyStation?.Id, Is.EqualTo(StationId.Dye), game.PromptText);
            game.UseNearbyStation();
            var dye = game.Dye;
            Assert.That(dye.IsOpen, Is.True);
            dye.Advance();
            dye.Advance();
            Assert.That(TradeState.Shared.AvailableOf(ProductId.PurpleCloth), Is.Zero);
            dye.Cancel();
            Assert.That(dye.IsOpen, Is.False);
            Assert.That(TradeState.Shared.AvailableOf(ProductId.PurpleCloth), Is.Zero);

            // Reopening starts a fresh swatch. Replaying the full task leaves the batch unchanged.
            for (var attempt = 0; attempt < 2; attempt++)
            {
                yield return null;
                game.UseNearbyStation();
                Assert.That(dye.IsOpen, Is.True);
                dye.Advance(); // Examine murex
                dye.Advance(); // Select
                dye.Advance(); // Dip
                dye.Advance(); // Lift
                Assert.That(dye.IsOpen, Is.True, "Show the finished swatch before returning.");
                dye.Advance(); // Return
                Assert.That(dye.IsOpen, Is.False);
                Assert.That(TradeState.Shared.AvailableOf(ProductId.PurpleCloth), Is.EqualTo(TradeState.PreparedClothBundles));
            }

            // Cargo station: the screen opens and walking pauses.
            yield return StandNextTo(StationId.Cargo);
            Assert.That(game.NearbyStation?.Id, Is.EqualTo(StationId.Cargo), game.PromptText);
            game.UseNearbyStation();
            var cargo = game.Cargo;
            Assert.That(cargo.IsOpen, Is.True);

            // A wrong destination is flagged on its letter line.
            cargo.ChangeQuantity(ProductId.PurpleCloth, 2);
            cargo.ChooseDestination(ProductId.PurpleCloth, DestinationId.Egypt);
            cargo.SelectGood(ProductId.PurpleCloth);
            cargo.PlaceSelected(HoldPositionId.DryLocker);
            cargo.Send();
            Assert.That(cargo.FlaggedLine, Is.EqualTo(1), cargo.DockworkerText);
            StringAssert.Contains("going somewhere else", cargo.DockworkerText);
            Assert.That(TradeState.Shared.ShipmentSent, Is.False);

            // Correct both lines and send.
            cargo.ChooseDestination(ProductId.PurpleCloth, DestinationId.Kition);
            for (var i = 0; i < 3; i++)
            {
                cargo.ChangeQuantity(ProductId.Wine, 1);
            }
            cargo.ChooseDestination(ProductId.Wine, DestinationId.Egypt);
            cargo.SelectGood(ProductId.Wine);
            cargo.PlaceSelected(HoldPositionId.JarBay);
            cargo.Send();
            Assert.That(TradeState.Shared.ShipmentSent, Is.True, cargo.DockworkerText);

            cargo.Close();
            Assert.That(cargo.IsOpen, Is.False);
            yield return null;
        }

        /// <summary>Places the player just outside a station footprint and lets them settle.</summary>
        private IEnumerator StandNextTo(StationId id)
        {
            var station = Station.Active.First(s => s.Id == id);
            var footprint = station.Footprint;
            var spot = new Vector3(footprint.center.x, footprint.max.y + 0.5f, footprint.min.z - 0.5f);
            var controller = player.GetComponent<CharacterController>();
            controller.enabled = false;
            player.transform.position = spot;
            controller.enabled = true;
            yield return new WaitForSeconds(0.5f);
        }
    }
}
