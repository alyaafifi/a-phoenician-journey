using System;
using System.Linq;
using PhoenicianJourney.Trade;
using UnityEngine;
using UnityEngine.UIElements;

namespace PhoenicianJourney.UI
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class HarborStory : MonoBehaviour
    {
        private const float FrameAspect = 1366f / 768f;

        [SerializeField] private StyleSheet styles;

        private VisualElement frame;
        private VisualElement controls;
        private DyeScreen dye;
        private CargoScreen cargo;
        private IVisualElementScheduledItem transition;
        private TradeState trade;
        private bool cedarRepaired;

        private void OnEnable()
        {
            trade = new TradeState();
            cedarRepaired = false;
            var root = GetComponent<UIDocument>().rootVisualElement;
            root.Clear();
            root.styleSheets.Add(styles);
            root.AddToClassList("harbor-root");
            root.AddToClassList("story-root");

            frame = new VisualElement { name = "story-frame" };
            frame.AddToClassList("story-frame");
            controls = new VisualElement { name = "story-controls" };
            controls.AddToClassList("story-controls");
            frame.Add(controls);
            root.Add(frame);
            root.RegisterCallback<GeometryChangedEvent>(FitFrame);
            dye = new DyeScreen(frame);
            cargo = new CargoScreen(frame, trade);
            ShowTitle();
        }

        private void OnDisable()
        {
            transition?.Pause();
            GetComponent<UIDocument>().rootVisualElement?.UnregisterCallback<GeometryChangedEvent>(FitFrame);
        }

        private void FitFrame(GeometryChangedEvent evt)
        {
            float width = Mathf.Min(evt.newRect.width, evt.newRect.height * FrameAspect);
            frame.style.width = width;
            frame.style.height = width / FrameAspect;
        }

        private void ShowTitle() =>
            ShowCard("title", null, null, "Begin", ShowMenu, "Skip to harbor", ShowHarbor);

        private void ShowMenu() =>
            ShowCard("menu", null, null, "Start story", ShowClassroom, "Back to title", ShowTitle);

        private void ShowClassroom() =>
            ShowCard("classroom", "History class", "The teacher has been talking about the Phoenicians for a while.", "Continue", ShowTeacher);

        private void ShowTeacher()
        {
            ShowCard("classroom", "Teacher",
                "Now, back to Byblos. They shipped cedar, traded cloth, and sailed as far as Cyprus. We’ll get to Cyprus in a minute. First, the timber…",
                "Continue", ShowDrowsy);
            var teacher = new VisualElement { pickingMode = PickingMode.Ignore };
            teacher.AddToClassList("story-teacher");
            teacher.style.backgroundImage = Artwork("teacher-cutout");
            controls.Insert(0, teacher);
        }

        private void ShowDrowsy() =>
            ShowCard("teacher", "Teacher",
                "So where were we? Right. Once the timber reached the harbor, someone still had to load the ship. Now, if you look at the diagram…",
                "Your eyes close", () => ShowTransition("eyes-half", 950,
                    () => ShowTransition("eyes-shut", 850,
                        () => ShowTransition(null, 650,
                            () => ShowTransition("wake-narrow", 400,
                                () => ShowTransition("wake-wide", 450, ShowWake))))));

        private void ShowWake() =>
            ShowCard("wake", "A dream of Byblos", "The teacher’s voice fades. You hear waves against stone. Three strangers lean over you.",
                "Listen", ShowQuestion);

        private void ShowQuestion() =>
            ShowCard("wake", "Young worker", "Who’s this? Have either of you seen him before?",
                "Continue", ShowReply);

        private void ShowReply() =>
            ShowCard("crew", "Dockworker", "I haven’t. Where’d he come from? I didn’t see him get off a boat.",
                "Continue", ShowInvitation);

        private void ShowInvitation() =>
            ShowCard("crew", "Harbor worker",
                "Well, he’s here now, and we’re short a pair of hands. Come on, lad. Pick a trade and get to work. The dyer’s on your left. The carpenter’s on your right.",
                "Continue", ShowOffer);

        private void ShowOffer() =>
            ShowCard("offer", "Harbor worker", "And if you want to help on the ship, take this. The order tells us what goes where.", "Take the bottle",
                () => ShowCard("bottle", null, null, "Unseal the bottle", () => ShowLetter(true), icon: "bottle-icon"));

        private void ShowLetter(bool fromIntro)
        {
            string order = string.Join("\n\n", Shipment.SampleOrder.Select(line => line.Text));
            ShowCard("letter", "Shipping order", order, fromIntro ? "Look around" : "Return to harbor", ShowHarbor);
        }

        private void ShowHarbor()
        {
            SetFrame("harbor");
            var banner = new Label(trade.ShipmentSent
                ? "Shipment sent"
                : trade.DyeTaskComplete ? "Purple cloth is ready to load" : "Byblos harbor");
            banner.AddToClassList("story-banner");
            controls.Add(banner);
            AddHotspot("dye", "Purple dye", "harbor-left", 6, 40, 22, 46, OpenDye);
            AddHotspot("ship", "Ship loading & routes", "harbor-ship", 39, 33, 23, 43, ShowShip);
            AddHotspot("cedar", cedarRepaired ? "Cedar carpentry · repaired" : "Cedar carpentry",
                "harbor-right", 72, 37, 27, 49, ShowCarpentry);
        }

        private void ShowShip() =>
            ShowCard("ship", "Cargo ship", "Read the shipping order, explore the routes, or load the hold.",
                "Load the ship", OpenCargo, "Learn trade routes", ShowRoutes, "Return to harbor", ShowHarbor);

        private void ShowRoutes()
        {
            string routes = string.Join("\n\n", Shipment.SampleOrder.Select(line =>
            {
                var destination = TradeCatalog.Destinations[line.Destination];
                return $"{TradeCatalog.Products[line.Product].Name} · Byblos to {destination.Name}\n{destination.Route}";
            }));
            ShowCard("ship", "Trade routes", routes + "\n\nThese are reconstructed routes for this order.",
                "Load the ship", OpenCargo, "Read shipping order", () => ShowLetter(false), "Back to ship", ShowShip);
        }

        private void ShowCarpentry() => ShowPlankChoices("This boat needs a replacement plank. Choose one that spans the gap.");

        private void ShowPlankChoices(string instruction) =>
            ShowCard("harbor-right", "Cedar carpentry", instruction, "Short plank",
                () => ShowPlankChoices("That plank leaves part of the gap open. Try a longer one."),
                "Matching plank", AlignPlank, "Return to harbor", ShowHarbor);

        private void AlignPlank() =>
            ShowCard("harbor-right", "Cedar carpentry", "Turn the plank so its long edge follows the hull.",
                "Across the hull", () => ShowCard("harbor-right", "Cedar carpentry",
                    "Across the hull, the plank won’t follow the seam.", "Try again", AlignPlank, "Return to harbor", ShowHarbor),
                "Along the hull", FitPlank, "Return to harbor", ShowHarbor);

        private void FitPlank() =>
            ShowCard("harbor-right", "Cedar carpentry", "The edges line up with the neighboring planks.",
                "Fit the plank", () =>
                {
                    cedarRepaired = true;
                    ShowCard("harbor-right", "Repair complete", "The new plank closes the gap in the hull.",
                        "Return to harbor", ShowHarbor);
                }, "Return to harbor", ShowHarbor);

        private void OpenDye()
        {
            frame.style.backgroundImage = Artwork("harbor-left");
            controls.SetEnabled(false);
            dye.Open(finished =>
            {
                if (finished)
                    trade.CompleteDyeTask();
                ShowHarbor();
                controls.Q<Button>("dye")?.Focus();
            });
        }

        private void OpenCargo()
        {
            controls.SetEnabled(false);
            cargo.Open(() =>
            {
                ShowHarbor();
                controls.Q<Button>("ship")?.Focus();
            });
        }

        private void ShowTransition(string art, long duration, Action next)
        {
            SetFrame(art);
            transition = frame.schedule.Execute(next).StartingIn(duration);
        }

        private void SetFrame(string art)
        {
            transition?.Pause();
            controls.Clear();
            controls.SetEnabled(true);
            frame.style.backgroundImage = Artwork(art);
        }

        private static StyleBackground Artwork(string name) => name == null
            ? new StyleBackground(StyleKeyword.None)
            : new StyleBackground(Resources.Load<Texture2D>("Harbor2D/" + name));

        private void ShowCard(string art, string speaker, string line, string action, Action next,
            string secondary = null, Action alternative = null, string third = null, Action back = null, string icon = null)
        {
            SetFrame(art);
            var card = new VisualElement();
            card.AddToClassList("story-card");
            if (speaker != null)
            {
                var heading = new Label(speaker);
                heading.AddToClassList("story-speaker");
                card.Add(heading);
            }
            if (line != null)
                card.Add(new Label(line));
            var actions = new VisualElement();
            actions.AddToClassList("story-actions");
            var primary = new Button(next) { text = action, name = "story-next" };
            if (icon != null)
            {
                var image = new VisualElement { pickingMode = PickingMode.Ignore };
                image.AddToClassList("story-action-icon");
                image.style.backgroundImage = Artwork(icon);
                actions.Add(image);
            }
            actions.Add(primary);
            if (alternative != null)
                actions.Add(new Button(alternative) { text = secondary, name = "story-back" });
            if (back != null)
                actions.Add(new Button(back) { text = third });
            card.Add(actions);
            controls.Add(card);
            primary.Focus();
        }

        private void AddHotspot(string id, string label, string highlight, float x, float y, float width, float height, Action action)
        {
            var button = new Button(action) { text = label, name = id };
            button.AddToClassList("story-hotspot");
            button.style.left = Length.Percent(x);
            button.style.top = Length.Percent(y);
            button.style.width = Length.Percent(width);
            button.style.height = Length.Percent(height);
            var pointer = new VisualElement { pickingMode = PickingMode.Ignore };
            pointer.AddToClassList("story-pointer");
            pointer.style.backgroundImage = Artwork("pointer-icon");
            button.Add(pointer);
            button.RegisterCallback<PointerEnterEvent>(_ =>
            {
                if (controls.enabledInHierarchy)
                    frame.style.backgroundImage = Artwork(highlight);
            });
            button.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                if (button.panel != null && controls.enabledInHierarchy)
                    frame.style.backgroundImage = Artwork("harbor");
            });
            controls.Add(button);
        }
    }
}
