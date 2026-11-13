using System;
using System.Collections;
using System.Linq;
using PhoenicianJourney.Audio;
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
        private Coroutine transition;
        private Texture2D blurredFrame;
        private StoryAudio storyAudio;
        private TradeState trade;
        private bool cedarRepaired;

        private void OnEnable()
        {
            trade = new TradeState();
            cedarRepaired = false;
            storyAudio = GetComponent<StoryAudio>();
            if (storyAudio == null)
            {
                storyAudio = gameObject.AddComponent<StoryAudio>();
            }
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
            if (transition != null)
                StopCoroutine(transition);
            transition = null;
            ReleaseBlurredFrame();
            storyAudio?.StopAll();
            GetComponent<UIDocument>().rootVisualElement?.UnregisterCallback<GeometryChangedEvent>(FitFrame);
        }

        private void FitFrame(GeometryChangedEvent evt)
        {
            float width = Mathf.Min(evt.newRect.width, evt.newRect.height * FrameAspect);
            frame.style.width = width;
            frame.style.height = width / FrameAspect;
        }

        private void ShowTitle()
        {
            storyAudio.StopAll();
            ShowCard("title", null, null, "Begin", ShowMenu, "Skip to harbor", ShowHarbor);
        }

        private void ShowMenu()
        {
            storyAudio.StopAll();
            ShowCard("menu", null, null, "Start story", ShowClassroom, "Back to title", ShowTitle);
        }

        private void ShowClassroom()
        {
            storyAudio.StartClassroomAmbience();
            ShowCard("classroom", "History class", "The teacher has been talking about the Phoenicians for a while.", "Continue", ShowTeacher);
        }

        // The spoken lines match this text. Regenerate the audio (AUDIO_CUES.md) if it changes.
        private void ShowTeacher()
        {
            storyAudio.PlayTeacher("teacher-1");
            ShowCard("teacher", "Teacher",
                "Now, back to Byblos. They shipped cedar, traded cloth, and sailed as far as Cyprus. We’ll get to Cyprus in a minute. First, the timber…",
                "Continue", ShowDrowsy);
        }

        private void ShowDrowsy()
        {
            storyAudio.PlayTeacher("teacher-2");
            ShowCard("teacher", "Teacher",
                "So where were we? Right. Once the timber reached the harbor, someone still had to load the ship. Now, if you look at the diagram…",
                "Your eyes close", () => transition = StartCoroutine(FallAsleepAndWake()));
        }

        private IEnumerator FallAsleepAndWake()
        {
            // The yawn, then the classroom muffles to silence over the eyelids and blackout (1.1 + 1 + 0.8 s).
            storyAudio.FallAsleep(2.9f);
            SetFrame("eyes-half");
            yield return new WaitForSecondsRealtime(1.1f);
            SetFrame("eyes-shut");
            yield return new WaitForSecondsRealtime(1f);
            SetFrame(null);
            yield return new WaitForSecondsRealtime(0.8f);
            storyAudio.StartHarborAmbience();
            yield return FadeWakeFrame("wake-narrow", 0.85f, 1f, 0.85f);
            yield return FadeWakeFrame("wake-wide", 1f, 0.85f, 0.4f);
            yield return FadeWakeFrame("wake", 0.65f, 0.4f, 0f);
            transition = null;
            ShowWake();
        }

        private IEnumerator FadeWakeFrame(string art, float duration, float startOpacity, float endOpacity)
        {
            SetFrame(art);
            var source = Resources.Load<Texture2D>("Harbor2D/" + art);
            blurredFrame = MakeBlurredCopy(source);
            var blur = new VisualElement { pickingMode = PickingMode.Ignore };
            blur.AddToClassList("story-frame");
            blur.AddToClassList("story-controls");
            blur.style.backgroundImage = new StyleBackground(blurredFrame);
            controls.Add(blur);

            float started = Time.unscaledTime;
            float elapsed;
            do
            {
                elapsed = Time.unscaledTime - started;
                blur.style.opacity = Mathf.Lerp(startOpacity, endOpacity, elapsed / duration);
                yield return null;
            } while (elapsed < duration);

            blur.RemoveFromHierarchy();
            ReleaseBlurredFrame();
        }

        private static Texture2D MakeBlurredCopy(Texture2D source)
        {
            RenderTexture previous = RenderTexture.active;
            RenderTexture reduced = null;
            try
            {
                // Repeated bilinear downsampling averages the artwork before UI Toolkit scales it back up.
                Texture input = source;
                do
                {
                    var smaller = RenderTexture.GetTemporary(Mathf.Max(1, input.width / 2), Mathf.Max(1, input.height / 2));
                    smaller.filterMode = FilterMode.Bilinear;
                    Graphics.Blit(input, smaller);
                    if (reduced != null)
                        RenderTexture.ReleaseTemporary(reduced);
                    reduced = smaller;
                    input = smaller;
                } while (input.width > 64);

                RenderTexture.active = reduced;
                var copy = new Texture2D(reduced.width, reduced.height, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };
                copy.ReadPixels(new Rect(0, 0, reduced.width, reduced.height), 0, 0);
                copy.Apply(false, true);
                return copy;
            }
            finally
            {
                RenderTexture.active = previous;
                if (reduced != null)
                    RenderTexture.ReleaseTemporary(reduced);
            }
        }

        private void ReleaseBlurredFrame()
        {
            if (blurredFrame != null)
                Destroy(blurredFrame);
            blurredFrame = null;
        }

        private void ShowWake() =>
            ShowCard("wake", "A dream of Byblos", "The teacher’s voice fades. You hear waves against stone. Three strangers lean over you.",
                "Listen", ShowQuestion);

        private void ShowQuestion()
        {
            storyAudio.PlayHarborVoice("young-worker", aside: true);
            ShowCard("wake", "Young worker", "Who’s this? Have either of you seen him before?",
                "Continue", ShowHelpingHand);
        }

        private void ShowHelpingHand() =>
            ShowCard("helping-hand", null, null, "Take his hand", ShowHandGrab);

        private void ShowHandGrab() =>
            ShowCard("hand-grab", null, null, "Continue", ShowReply);

        private void ShowReply()
        {
            storyAudio.PlayHarborVoice("dockworker", aside: true);
            ShowCard("crew", "Dockworker", "I haven’t. Where’d he come from? I didn’t see him get off a boat.",
                "Continue", ShowInvitation);
        }

        private void ShowInvitation()
        {
            storyAudio.PlayHarborAddress();
            ShowCard("crew", "Harbor worker",
                "Well, he’s here now, and we’re short a pair of hands. Come on, lad. Pick a trade and get to work. The dyer’s on your left. The carpenter’s on your right.",
                "Continue", ShowOffer);
        }

        private void ShowOffer()
        {
            storyAudio.PlayHarborVoice("foreman-offer");
            ShowCard("offer", "Harbor worker", "And if you want to help on the ship, take this. The order tells us what goes where.", "Take the bottle",
                () =>
                {
                    storyAudio.StopVoice();
                    ShowCard("bottle-student", null, null, "Unseal the bottle", ShowTranslation, icon: "bottle-icon");
                });
        }

        private void ShowTranslation()
        {
            SetFrame("decode-letter");
            new LetterScreen(controls, ShowHarbor, storyAudio);
        }

        private void ShowLetter()
        {
            string order = string.Join("\n\n", Shipment.SampleOrder.Select(line => line.Text));
            ShowCard("decode-letter", "Shipping order", order, "Return to harbor", ShowHarbor);
        }

        private void ShowHarbor()
        {
            storyAudio.StartHarborAmbience();
            storyAudio.StopVoice();
            SetFrame("harbor");
            var banner = new Label(trade.ShipmentSent
                ? "Shipment sent"
                : trade.DyeTaskComplete ? "Purple cloth is ready to load" : "Byblos harbor");
            banner.AddToClassList("story-banner");
            controls.Add(banner);
            AddHotspot("dye", "Purple dye", "harbor-left", 0, 40, 28, 60, OpenDye);
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
                "Load the ship", OpenCargo, "Read shipping order", ShowLetter, "Back to ship", ShowShip);
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

        private void SetFrame(string art)
        {
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
