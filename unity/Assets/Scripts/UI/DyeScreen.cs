using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace PhoenicianJourney.UI
{
    public sealed class DyeScreen
    {
        private const float DipDuration = 5f;
        private const float ArtAspect = 16f / 9f;
        private const float CloakWidth = 0.32f;
        private const float CloakHeight = 0.38f;
        private const float BathSurface = 0.59f;
        private static readonly Vector2 Rest = new Vector2(0.80f, 0.76f);
        private static readonly int[] BandWidths = { 24, 14, 9, 6, 9, 14, 24 };
        private static readonly int[] BandStars = { 0, 1, 2, 3, 2, 1, 0 };
        private static readonly Color[] BandColors =
        {
            new Color32(166, 44, 64, 255), new Color32(218, 109, 40, 255),
            new Color32(240, 190, 62, 255), new Color32(85, 157, 86, 255),
        };
        private static readonly Color[] DyeColors =
        {
            new Color32(204, 162, 188, 255), new Color32(170, 112, 168, 255),
            new Color32(139, 78, 148, 255), new Color32(108, 49, 121, 255),
            new Color32(86, 32, 96, 255), new Color32(66, 25, 76, 255),
            new Color32(48, 18, 56, 255),
        };

        private enum Stage { Intro, Ready, Dipping, Result }

        private readonly VisualElement overlay;
        private readonly VisualElement artwork;
        private readonly VisualElement clothLayer;
        private readonly VisualElement cloak;
        private readonly VisualElement meter;
        private readonly VisualElement needle;
        private readonly Label meterStatus;
        private readonly VisualElement[] ratingStars = new VisualElement[3];
        private readonly Label instruction;
        private readonly Label controlsGuide;
        private readonly Button advance;
        private readonly Button leave;
        private readonly IVisualElementScheduledItem timer;
        private Action<bool> onClose;
        private Stage stage;
        private Vector2 cloakPosition = Rest;
        private Vector2 grabOffset;
        private int pointer = -1;
        private int stars;
        private float dipStarted;

        public DyeScreen(VisualElement root)
        {
            overlay = new VisualElement { name = "dye-screen" };
            overlay.AddToClassList("story-controls");
            overlay.AddToClassList("dye-screen");
            overlay.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            overlay.RegisterCallback<GeometryChangedEvent>(evt => FitArtwork(evt.newRect.size));

            artwork = new VisualElement { name = "dye-artwork" };
            artwork.AddToClassList("dye-artwork");
            overlay.Add(artwork);

            clothLayer = new VisualElement { pickingMode = PickingMode.Ignore };
            clothLayer.AddToClassList("dye-cloth-layer");
            artwork.Add(clothLayer);
            cloak = new VisualElement { name = "dye-cloak" };
            cloak.AddToClassList("dye-cloak");
            cloak.style.width = Length.Percent(CloakWidth * 100f);
            cloak.style.backgroundImage = LoadArt("dye-cloak");
            clothLayer.Add(cloak);
            artwork.RegisterCallback<GeometryChangedEvent>(_ => PlaceCloak(cloakPosition));
            cloak.RegisterCallback<PointerDownEvent>(Grab);
            cloak.RegisterCallback<PointerMoveEvent>(Move);
            cloak.RegisterCallback<PointerUpEvent>(Release);
            cloak.RegisterCallback<PointerCaptureOutEvent>(_ =>
            {
                if (pointer != -1)
                    ResetAttempt();
            });
            cloak.RegisterCallback<PointerCancelEvent>(_ => ResetAttempt());

            var card = new VisualElement();
            card.AddToClassList("story-card");
            card.AddToClassList("dye-card");
            artwork.Add(card);
            var speaker = new Label("Dyer");
            speaker.AddToClassList("story-speaker");
            card.Add(speaker);
            instruction = new Label();
            card.Add(instruction);
            controlsGuide = new Label("1. Click and hold the wool cloak on the right.\n2. Drag it into the purple dye and hold it there.\n3. When the marker reaches green, drag the cloak up or release the mouse.");
            controlsGuide.AddToClassList("dye-controls-guide");
            card.Add(controlsGuide);

            meter = new VisualElement();
            meter.AddToClassList("dye-meter");
            var track = new VisualElement();
            track.AddToClassList("dye-track");
            for (int i = 0; i < BandWidths.Length; i++)
            {
                var band = new VisualElement();
                band.style.width = Length.Percent(BandWidths[i]);
                band.style.backgroundColor = BandColors[BandStars[i]];
                track.Add(band);
            }
            needle = new VisualElement { pickingMode = PickingMode.Ignore };
            needle.AddToClassList("dye-needle");
            track.Add(needle);
            meter.Add(track);
            meterStatus = new Label();
            var feedback = new VisualElement();
            feedback.AddToClassList("dye-feedback");
            for (int i = 0; i < ratingStars.Length; i++)
            {
                var star = new VisualElement { name = $"dye-star-{i + 1}", pickingMode = PickingMode.Ignore };
                star.AddToClassList("dye-star");
                star.generateVisualContent += context => DrawStar(context, star);
                ratingStars[i] = star;
                feedback.Add(star);
            }
            feedback.Add(meterStatus);
            meter.Add(feedback);
            card.Add(meter);

            var actions = new VisualElement();
            actions.AddToClassList("story-actions");
            card.Add(actions);
            advance = new Button(Advance) { name = "dye-advance" };
            actions.Add(advance);
            leave = new Button(Cancel) { name = "dye-leave", text = "Leave stall" };
            actions.Add(leave);
            root.Add(overlay);
            timer = overlay.schedule.Execute(UpdateMeter).Every(16);
            timer.Pause();
            overlay.style.display = DisplayStyle.None;
        }

        public bool IsOpen => overlay.style.display == DisplayStyle.Flex;

        public void Open(Action<bool> closed)
        {
            onClose = closed;
            stage = Stage.Intro;
            stars = 0;
            overlay.style.display = DisplayStyle.Flex;
            artwork.style.backgroundImage = LoadArt("dye-vat");
            clothLayer.style.display = DisplayStyle.Flex;
            clothLayer.style.height = Length.Percent(100);
            cloak.style.unityBackgroundImageTintColor = Color.white;
            PlaceCloak(Rest);
            meter.style.display = DisplayStyle.None;
            instruction.text = "Those are murex shells in the basket. The dye starts with a substance from a gland inside the snail.";
            controlsGuide.style.display = DisplayStyle.Flex;
            ShowAction("Start dyeing");
        }

        public void Advance()
        {
            if (!IsOpen)
                return;

            if (stage == Stage.Intro)
                ResetAttempt();
            else if (stage == Stage.Result && stars == 0)
                ResetAttempt();
            else if (stage == Stage.Result)
                Close(true);
        }

        public void Cancel() => Close(stage == Stage.Result && stars > 0);

        private void FitArtwork(Vector2 size)
        {
            float width = Mathf.Min(size.x, size.y * ArtAspect);
            artwork.style.width = width;
            artwork.style.height = width / ArtAspect;
        }

        private void Grab(PointerDownEvent evt)
        {
            if (stage != Stage.Ready || pointer != -1 || evt.button != 0)
                return;

            pointer = evt.pointerId;
            grabOffset = cloakPosition - PointerPosition(evt.position);
            cloak.CapturePointer(pointer);
            evt.StopPropagation();
        }

        private void Move(PointerMoveEvent evt)
        {
            if (evt.pointerId != pointer)
                return;

            Vector2 position = PointerPosition(evt.position) + grabOffset;
            UpdateDrag(position);
            evt.StopPropagation();
        }

        private void Release(PointerUpEvent evt)
        {
            if (evt.pointerId != pointer)
                return;

            // A final move can arrive on release, including when the pointer leaves the vat quickly.
            UpdateDrag(PointerPosition(evt.position) + grabOffset);
            if (stage == Stage.Dipping)
                LiftCloak();
            else if (stage == Stage.Ready)
                ResetAttempt();
            evt.StopPropagation();
        }

        private Vector2 PointerPosition(Vector2 panelPosition)
        {
            Vector2 local = artwork.WorldToLocal(panelPosition);
            return new Vector2(local.x / artwork.layout.width, local.y / artwork.layout.height);
        }

        private void UpdateDrag(Vector2 position)
        {
            position.x = Mathf.Clamp(position.x, CloakWidth / 2, 1f - CloakWidth / 2);
            position.y = Mathf.Clamp(position.y, CloakHeight / 2, 1f - CloakHeight / 2);
            bool overBath = position.x >= 0.30f && position.x <= 0.56f;
            if (stage == Stage.Ready && overBath && position.y >= BathSurface && position.y <= 0.68f)
                BeginDip();
            else if (stage == Stage.Dipping && (!overBath || position.y < BathSurface - 0.025f))
            {
                LiftCloak();
                return;
            }

            if (stage == Stage.Dipping)
                position.y = Mathf.Min(position.y, BathSurface + 0.035f);
            PlaceCloak(position);
        }

        public void BeginDip()
        {
            if (stage != Stage.Ready || !IsOpen)
                return;

            stage = Stage.Dipping;
            dipStarted = Time.unscaledTime;
            clothLayer.style.height = Length.Percent((BathSurface + 0.045f) * 100);
            cloak.style.unityBackgroundImageTintColor = DyeColors[0];
            meter.style.display = DisplayStyle.Flex;
            UpdateMeter();
            timer.Resume();
        }

        public void LiftCloak()
        {
            if (stage == Stage.Dipping)
                ShowResult(MeterProgress());
        }

        private float MeterProgress()
        {
            float time = Mathf.Clamp01((Time.unscaledTime - dipStarted) / DipDuration);
            return (1f - Mathf.Cos(Mathf.PI * time)) * 0.5f;
        }

        private static int BandAt(float progress)
        {
            int end = 0;
            for (int i = 0; i < BandWidths.Length - 1; i++)
            {
                end += BandWidths[i];
                if (progress < end / 100f)
                    return i;
            }
            return BandWidths.Length - 1;
        }

        private void UpdateMeter()
        {
            float progress = MeterProgress();
            ShowMeter(progress);
            if (progress >= 1f)
                ShowResult(progress);
        }

        private void ShowMeter(float progress)
        {
            needle.style.left = Length.Percent(progress * 100f);
            int band = BandAt(progress);
            int score = BandStars[band];
            for (int i = 0; i < ratingStars.Length; i++)
            {
                var star = ratingStars[i];
                bool earned = i < score;
                if (star.ClassListContains("is-earned") == earned)
                    continue;
                star.EnableInClassList("is-earned", earned);
                star.MarkDirtyRepaint();
            }
            meterStatus.text = band == 0 ? "Too early" : band == 6 ? "Too late" : $"{score} of 3";
        }

        private static void DrawStar(MeshGenerationContext context, VisualElement star)
        {
            bool earned = star.ClassListContains("is-earned");
            var painter = context.painter2D;
            Vector2 center = star.contentRect.center;
            float radius = Mathf.Min(star.contentRect.width, star.contentRect.height) / 2f - 2f;
            if (radius <= 0)
                return;

            Vector2 Point(int index)
            {
                float angle = index * Mathf.PI / 5f - Mathf.PI / 2f;
                float length = index % 2 == 0 ? radius : radius * 0.47f;
                return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * length;
            }

            painter.BeginPath();
            painter.MoveTo(Point(0));
            for (int i = 1; i < 10; i++)
                painter.LineTo(Point(i));
            painter.ClosePath();
            painter.fillColor = earned ? new Color32(222, 154, 45, 255) : new Color32(76, 49, 65, 255);
            painter.Fill();
            painter.lineWidth = 1.5f;
            painter.strokeColor = earned ? new Color32(255, 226, 151, 255) : new Color32(166, 131, 141, 255);
            painter.Stroke();

            if (!earned)
                return;

            painter.BeginPath();
            painter.MoveTo(Point(0));
            painter.LineTo(center);
            painter.LineTo(Point(2));
            painter.LineTo(Point(1));
            painter.ClosePath();
            painter.fillColor = new Color32(255, 216, 110, 255);
            painter.Fill();
        }

        private void ShowResult(float progress)
        {
            timer.Pause();
            ReleasePointer();
            stage = Stage.Result;
            int band = BandAt(progress);
            stars = BandStars[band];
            ShowMeter(progress);
            clothLayer.style.height = Length.Percent(100);
            cloak.style.unityBackgroundImageTintColor = DyeColors[band];
            PlaceCloak(Rest);
            controlsGuide.style.display = DisplayStyle.None;
            instruction.text = band switch
            {
                0 => "Too soon. The wool barely took the dye. Try again.",
                6 => "Too long. The purple came out too dark. Try again.",
                3 => "That’s the purple we wanted. The prepared batch is ready for the ship.",
                < 3 => "The purple came out light. The prepared batch is ready for the ship.",
                _ => "The purple came out dark. The prepared batch is ready for the ship.",
            };
            ShowAction(stars == 0 ? "Try again" : "Return to harbor");
        }

        private void ResetAttempt()
        {
            timer.Pause();
            ReleasePointer();
            stage = Stage.Ready;
            stars = 0;
            clothLayer.style.height = Length.Percent(100);
            cloak.style.unityBackgroundImageTintColor = Color.white;
            PlaceCloak(Rest);
            meter.style.display = DisplayStyle.None;
            controlsGuide.style.display = DisplayStyle.Flex;
            instruction.text = "It took many snails and skilled work to prepare this bath.";
            advance.style.display = DisplayStyle.None;
            leave.Focus();
        }

        private void PlaceCloak(Vector2 position)
        {
            cloakPosition = position;
            cloak.style.left = Length.Percent((position.x - CloakWidth / 2) * 100f);
            // The clipping layer changes height while the garment stays in artwork coordinates.
            float height = artwork.layout.height;
            if (float.IsNaN(height) || height <= 0)
                return;
            cloak.style.top = (position.y - CloakHeight / 2) * height;
            cloak.style.height = height * CloakHeight;
        }

        private void ReleasePointer()
        {
            int captured = pointer;
            pointer = -1;
            if (captured != -1 && cloak.HasPointerCapture(captured))
                cloak.ReleasePointer(captured);
        }

        private void ShowAction(string label)
        {
            advance.text = label;
            advance.style.display = DisplayStyle.Flex;
            advance.Focus();
        }

        private void Close(bool finished)
        {
            if (!IsOpen)
                return;

            timer.Pause();
            ReleasePointer();
            overlay.style.display = DisplayStyle.None;
            var callback = onClose;
            onClose = null;
            callback?.Invoke(finished);
        }

        private static StyleBackground LoadArt(string name) =>
            new StyleBackground(Resources.Load<Texture2D>("Harbor2D/" + name));

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Escape)
            {
                evt.StopPropagation();
                Cancel();
            }
        }
    }
}
