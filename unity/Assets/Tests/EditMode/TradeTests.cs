using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PhoenicianJourney.Trade;

namespace PhoenicianJourney.Tests
{
    /// <summary>Order rules and the dye-to-cargo contract. Ported from the Babylon.js prototype tests.</summary>
    public sealed class TradeTests
    {
        private static readonly Dictionary<ProductId, int> Ready = new Dictionary<ProductId, int>
        {
            [ProductId.PurpleCloth] = TradeState.PreparedClothBundles,
            [ProductId.Wine] = TradeState.StagedWineJars,
        };

        private static Dictionary<ProductId, LinePlan> CorrectPlan()
        {
            var plan = Shipment.EmptyPlan();
            foreach (var line in Shipment.SampleOrder)
            {
                plan[line.Product] = new LinePlan { Quantity = line.Quantity, Destination = line.Destination, Stowage = line.Stowage };
            }

            return plan;
        }

        private static void AssertSingleProblem(List<ShipmentProblem> problems, int line, ProductId product, ProblemField field, bool missing)
        {
            Assert.That(problems, Has.Count.EqualTo(1), string.Join("; ", problems));
            Assert.That(problems[0], Is.EqualTo(new ShipmentProblem(line, product, field, missing)));
        }

        [Test]
        public void SampleOrderHasTwoGoodsDestinationsAndHoldPositions()
        {
            Assert.That(Shipment.SampleOrder.Count, Is.EqualTo(2));
            Assert.That(Shipment.SampleOrder.Select(l => l.Product).Distinct().Count(), Is.EqualTo(2));
            Assert.That(Shipment.SampleOrder.Select(l => l.Destination).Distinct().Count(), Is.EqualTo(2));
            Assert.That(Shipment.SampleOrder.Select(l => l.Stowage).Distinct().Count(), Is.EqualTo(2));
        }

        [Test]
        public void SampleOrderAsksForNoMoreThanTheLandingSupplies()
        {
            foreach (var line in Shipment.SampleOrder)
            {
                Assert.That(line.Quantity, Is.LessThanOrEqualTo(Ready[line.Product]), $"line {line.Line}");
            }
        }

        [Test]
        public void AcceptsTheShipmentTheLetterDescribes()
        {
            Assert.That(Shipment.Check(Shipment.SampleOrder, CorrectPlan(), Ready), Is.Empty);
        }

        [Test]
        public void FlagsMissingClothBeforeAnythingElseOnItsLine()
        {
            var noCloth = new Dictionary<ProductId, int>(Ready) { [ProductId.PurpleCloth] = 0 };
            AssertSingleProblem(Shipment.Check(Shipment.SampleOrder, CorrectPlan(), noCloth), 1, ProductId.PurpleCloth, ProblemField.Availability, true);
        }

        [Test]
        public void PointsToTheLineWithTheWrongQuantity()
        {
            var plan = CorrectPlan();
            plan[ProductId.Wine].Quantity = 4;
            AssertSingleProblem(Shipment.Check(Shipment.SampleOrder, plan, Ready), 2, ProductId.Wine, ProblemField.Quantity, false);
        }

        [Test]
        public void PointsToTheLineWithTheWrongDestination()
        {
            var plan = CorrectPlan();
            plan[ProductId.PurpleCloth].Destination = DestinationId.Egypt;
            AssertSingleProblem(Shipment.Check(Shipment.SampleOrder, plan, Ready), 1, ProductId.PurpleCloth, ProblemField.Destination, false);
        }

        [Test]
        public void PointsToTheLineWithTheWrongStowage()
        {
            var plan = CorrectPlan();
            plan[ProductId.Wine].Stowage = HoldPositionId.DryLocker;
            AssertSingleProblem(Shipment.Check(Shipment.SampleOrder, plan, Ready), 2, ProductId.Wine, ProblemField.Stowage, false);
        }

        [Test]
        public void ReportsOneProblemPerLineInLetterOrder()
        {
            var problems = Shipment.Check(Shipment.SampleOrder, Shipment.EmptyPlan(), Ready);
            Assert.That(problems.Select(p => (p.Line, p.Field, p.Missing)), Is.EqualTo(new[]
            {
                (1, ProblemField.Quantity, true),
                (2, ProblemField.Quantity, true),
            }));
        }

        [Test]
        public void ChecksDestinationBeforeStowageOnTheSameLine()
        {
            var plan = CorrectPlan();
            plan[ProductId.Wine].Destination = null;
            plan[ProductId.Wine].Stowage = null;
            AssertSingleProblem(Shipment.Check(Shipment.SampleOrder, plan, Ready), 2, ProductId.Wine, ProblemField.Destination, true);
        }

        [Test]
        public void DockworkerNamesTheLineWithoutGivingTheAnswer()
        {
            var message = Shipment.DockworkerMessage(new ShipmentProblem(2, ProductId.Wine, ProblemField.Destination, false));
            StringAssert.Contains("line 2", message);
            StringAssert.DoesNotContain("Egypt", message);
        }

        [Test]
        public void ClothStaysUnavailableUntilTheDyeTaskIsCompleted()
        {
            var state = new TradeState();
            Assert.That(state.DyeTaskComplete, Is.False);
            Assert.That(state.AvailableOf(ProductId.PurpleCloth), Is.Zero);
            Assert.That(state.AvailableOf(ProductId.Wine), Is.EqualTo(TradeState.StagedWineJars));
        }

        [Test]
        public void CompletingTheDyeTaskUnlocksThePreparedBatchOnce()
        {
            var state = new TradeState();
            var notifications = 0;
            state.Changed += () => notifications++;

            Assert.That(state.CompleteDyeTask(), Is.True);
            Assert.That(state.CompleteDyeTask(), Is.False);
            Assert.That(state.AvailableOf(ProductId.PurpleCloth), Is.EqualTo(TradeState.PreparedClothBundles));
            Assert.That(notifications, Is.EqualTo(1));
        }

        [Test]
        public void RecordsTheShipmentOnce()
        {
            var state = new TradeState();
            var notifications = 0;
            state.Changed += () => notifications++;
            state.MarkShipmentSent();
            state.MarkShipmentSent();
            Assert.That(state.ShipmentSent, Is.True);
            Assert.That(notifications, Is.EqualTo(1));
        }
    }
}
