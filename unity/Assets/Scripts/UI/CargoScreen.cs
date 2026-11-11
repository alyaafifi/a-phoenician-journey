using System;
using System.Collections.Generic;
using System.Linq;
using PhoenicianJourney.Trade;
using UnityEngine;
using UnityEngine.UIElements;

namespace PhoenicianJourney.UI
{
    /// <summary>
    /// The 2D cargo interaction opened from the cargo station. The player reads the letter,
    /// sets quantity and destination per good, assigns each good to a place in the hold, and
    /// sends the ship. The dockworker points back to the first letter line that is wrong.
    /// Styles are in Assets/UI/Harbor.uss.
    /// </summary>
    public sealed class CargoScreen
    {
        private const string Greeting =
            "Choose a good, set how many, pick where it is going, then click its place in the hold.";

        private readonly VisualElement overlay;
        private readonly ScrollView panel;
        private readonly TradeState state;
        private Dictionary<ProductId, LinePlan> plan = Shipment.EmptyPlan();
        private ProductId? selected;
        private string message = Greeting;
        private int? flaggedLine;
        private Action onClose;
        private string focusKey;

        public CargoScreen(VisualElement root, TradeState state)
        {
            this.state = state;
            overlay = new VisualElement { name = "cargo-screen" };
            overlay.AddToClassList("cargo-screen");
            panel = new ScrollView(ScrollViewMode.Vertical);
            panel.AddToClassList("cargo-panel");
            overlay.Add(panel);
            overlay.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            root.Add(overlay);
            overlay.style.display = DisplayStyle.None;
            state.Changed += () =>
            {
                if (IsOpen)
                {
                    Render();
                }
            };
        }

        public bool IsOpen => overlay.style.display == DisplayStyle.Flex;

        /// <summary>The letter line the dockworker last flagged, for tests and debugging.</summary>
        public int? FlaggedLine => flaggedLine;

        public string DockworkerText => message;

        public void Open(Action closed)
        {
            onClose = closed;
            overlay.style.display = DisplayStyle.Flex;
            focusKey = "close";
            Render();
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            overlay.style.display = DisplayStyle.None;
            var callback = onClose;
            onClose = null;
            callback?.Invoke();
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Escape)
            {
                evt.StopPropagation();
                Close();
            }
        }

        // Actions. Each one also stands in for a click, so play-mode checks can drive the screen.

        public void SelectGood(ProductId product) => Update(() => selected = product, $"select-{product}");

        public void ChangeQuantity(ProductId product, int delta) => Update(
            () =>
            {
                var available = state.AvailableOf(product);
                plan[product].Quantity = Mathf.Clamp(plan[product].Quantity + delta, 0, available);
            },
            delta < 0 ? $"minus-{product}" : $"plus-{product}");

        public void ChooseDestination(ProductId product, DestinationId destination) =>
            Update(() => plan[product].Destination = destination, $"dest-{product}-{destination}");

        public void PlaceSelected(HoldPositionId position)
        {
            if (selected is not ProductId product)
            {
                return;
            }

            Update(
                () =>
                {
                    plan[product].Stowage = position;
                    selected = null;
                },
                $"hold-{position}");
        }

        public void Send()
        {
            var problems = Shipment.Check(Shipment.SampleOrder, plan, state.Available);
            if (problems.Count > 0)
            {
                message = Shipment.DockworkerMessage(problems[0]);
                flaggedLine = problems[0].Line;
                focusKey = "send";
                Render();
            }
            else
            {
                flaggedLine = null;
                focusKey = "close";
                state.MarkShipmentSent();
                Render();
            }
        }

        private void Update(Action change, string key)
        {
            change();
            flaggedLine = null;
            message = Greeting;
            focusKey = key;
            Render();
        }

        // Rendering

        private void Render()
        {
            panel.Clear();

            var header = Element("cargo-header");
            header.Add(Text("Loading the ship at Byblos", "cargo-title"));
            header.Add(MakeButton("Back to the harbor", Close, "close", "cargo-close"));
            panel.Add(header);

            var body = Element("cargo-body");
            body.Add(RenderLetter());
            body.Add(state.ShipmentSent ? RenderSent() : RenderLoading());
            panel.Add(body);

            if (focusKey != null)
            {
                var target = panel.Q<VisualElement>(focusKey);
                target?.schedule.Execute(() => target.Focus());
            }
        }

        private VisualElement RenderLetter()
        {
            var letter = Element("cargo-letter");
            letter.Add(Text("The shipping letter", "cargo-heading"));
            letter.Add(Text("To the dockmaster at Byblos:", "cargo-letter-from"));
            foreach (var line in Shipment.SampleOrder)
            {
                var row = Text($"{line.Line}.  {line.Text}", "cargo-letter-line");
                row.EnableInClassList("flagged", line.Line == flaggedLine);
                letter.Add(row);
            }

            return letter;
        }

        private VisualElement RenderLoading()
        {
            var work = Element("cargo-work");

            work.Add(Text("Goods on the landing", "cargo-heading"));
            var goods = Element("cargo-goods");
            foreach (var line in Shipment.SampleOrder)
            {
                goods.Add(RenderGood(line.Product));
            }
            work.Add(goods);

            work.Add(Text("The ship's hold", "cargo-heading"));
            work.Add(Text(
                selected is ProductId chosen
                    ? $"Click a place in the hold for the {TradeCatalog.Products[chosen].Name.ToLowerInvariant()}."
                    : "Select a good, then click its place in the hold.",
                "cargo-hint"));
            var ship = Element("ship");
            ship.Add(Text("Stern", "ship-end"));
            foreach (var position in TradeCatalog.HoldOrder)
            {
                ship.Add(RenderHoldSlot(position));
            }
            ship.Add(Text("Bow", "ship-end"));
            work.Add(ship);

            var dockworker = Element("dockworker");
            dockworker.Add(Text("Dockworker: ", "dockworker-name"));
            dockworker.Add(Text(message, "dockworker-text"));
            work.Add(dockworker);

            work.Add(MakeButton("Load and send the ship", Send, "send", "cargo-send"));
            return work;
        }

        private VisualElement RenderGood(ProductId productId)
        {
            var product = TradeCatalog.Products[productId];
            var chosen = plan[productId];
            var available = state.AvailableOf(productId);

            var card = Element("good");
            card.EnableInClassList("selected", selected == productId);

            var select = MakeButton(string.Empty, () => SelectGood(productId), $"select-{productId}", "good-select");
            select.Add(Swatch(productId));
            var labels = Element("good-labels");
            labels.Add(Text(product.Name, "good-name"));
            labels.Add(Text(
                available > 0
                    ? $"{available} {product.UnitLabel(available)} ready"
                    : "Not ready. Finish the swatch at the dye stall.",
                "good-stock"));
            select.Add(labels);
            card.Add(select);

            var quantity = Element("good-quantity");
            var minus = MakeButton("−", () => ChangeQuantity(productId, -1), $"minus-{productId}", "stepper");
            var plus = MakeButton("+", () => ChangeQuantity(productId, 1), $"plus-{productId}", "stepper");
            minus.SetEnabled(chosen.Quantity > 0);
            plus.SetEnabled(chosen.Quantity < available);
            quantity.Add(minus);
            quantity.Add(Text($"{chosen.Quantity} {product.UnitLabel(chosen.Quantity)}", "quantity-value"));
            quantity.Add(plus);
            card.Add(quantity);

            var destinations = Element("good-destinations");
            foreach (var destinationId in TradeCatalog.DestinationOrder)
            {
                var destination = TradeCatalog.Destinations[destinationId];
                var button = MakeButton(
                    $"{destination.Name} ({destination.Region})",
                    () => ChooseDestination(productId, destinationId),
                    $"dest-{productId}-{destinationId}",
                    "destination");
                button.EnableInClassList("chosen", chosen.Destination == destinationId);
                destinations.Add(button);
            }
            card.Add(destinations);

            card.Add(Text(
                chosen.Stowage is HoldPositionId stowage
                    ? $"Stowed in the {TradeCatalog.HoldPositions[stowage].Name.ToLowerInvariant()}"
                    : "Not stowed yet",
                "good-stowed"));
            return card;
        }

        private VisualElement RenderHoldSlot(HoldPositionId positionId)
        {
            var position = TradeCatalog.HoldPositions[positionId];
            var slot = MakeButton(string.Empty, () => PlaceSelected(positionId), $"hold-{positionId}", "hold-slot");
            slot.Add(Text(position.Name, "hold-name"));
            slot.Add(Text(position.Location.ToUpperInvariant(), "hold-location"));
            slot.Add(Text(position.Description, "hold-description"));
            var contents = Element("hold-contents");
            foreach (var product in Shipment.SampleOrder.Select(line => line.Product))
            {
                if (plan[product].Stowage == positionId)
                {
                    contents.Add(Swatch(product));
                }
            }
            slot.Add(contents);
            slot.SetEnabled(selected != null);
            return slot;
        }

        private VisualElement RenderSent()
        {
            var work = Element("cargo-work");
            work.Add(Text("The ship sails", "cargo-heading"));

            var dockworker = Element("dockworker");
            dockworker.Add(Text("Dockworker: ", "dockworker-name"));
            dockworker.Add(Text("Everything matches the letter. Fair winds!", "dockworker-text"));
            work.Add(dockworker);

            foreach (var line in Shipment.SampleOrder)
            {
                var product = TradeCatalog.Products[line.Product];
                var destination = TradeCatalog.Destinations[line.Destination];
                work.Add(Text(
                    $"<b>{line.Quantity} {product.UnitLabel(line.Quantity)} of {product.Name.ToLowerInvariant()} to {destination.Name}.</b> {destination.Route}",
                    "cargo-summary"));
            }

            work.Add(Text(
                "What the evidence shows: Byblos was among the kingdoms whose tribute to Assyria included multicoloured " +
                "garments and purple-dyed wool. Two 8th-century BCE Phoenician wrecks off Ashkelon carried wine in jars, " +
                "probably bound for Egypt or Carthage. No record survives of this exact shipment. The routes, quantities, " +
                "and the ship's hold are reconstructed for the game.",
                "cargo-note"));
            return work;
        }

        // Element helpers

        private static VisualElement Element(string className)
        {
            var element = new VisualElement();
            element.AddToClassList(className);
            return element;
        }

        private static Label Text(string text, string className)
        {
            var label = new Label(text) { enableRichText = true };
            label.AddToClassList(className);
            return label;
        }

        private static Button MakeButton(string text, Action clicked, string name, string className)
        {
            var button = new Button(clicked) { text = text, name = name };
            button.AddToClassList(className);
            return button;
        }

        private static VisualElement Swatch(ProductId product)
        {
            var swatch = Element("good-swatch");
            swatch.AddToClassList(product == ProductId.PurpleCloth ? "good-purple-cloth" : "good-wine");
            swatch.tooltip = TradeCatalog.Products[product].Name;
            return swatch;
        }
    }
}
