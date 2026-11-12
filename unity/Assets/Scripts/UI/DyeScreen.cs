using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace PhoenicianJourney.UI
{
    public sealed class DyeScreen
    {
        private static readonly (string Artwork, string Line, string Action)[] Stages =
        {
            ("dye-murex", "This is a murex shell. The dye starts with a substance from a gland inside the snail.", "Examine the murex"),
            ("dye-plain", "It took many snails and several stages to prepare this bath. It’s skilled work. Choose a plain wool swatch to test it.", "Select swatch"),
            ("dye-selected", "Lower the swatch into the prepared dye bath.", "Dip swatch"),
            ("dye-dipped", "Lift the swatch out of the vat and look at its color.", "Lift swatch"),
            ("dye-reveal", "The wool is purple. This cloth is valuable. Take the swatch back to the landing so we can load the prepared batch.", "Return to harbor"),
        };

        private readonly VisualElement overlay;
        private readonly Label instruction;
        private readonly Button advance;
        private readonly Button leave;
        private Action<bool> onClose;
        private int step;

        public DyeScreen(VisualElement root)
        {
            overlay = new VisualElement { name = "dye-screen" };
            overlay.AddToClassList("story-controls");
            overlay.AddToClassList("dye-screen");
            overlay.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);

            var card = new VisualElement();
            card.AddToClassList("story-card");
            card.AddToClassList("dye-card");
            overlay.Add(card);

            var speaker = new Label("Dyer");
            speaker.AddToClassList("story-speaker");
            card.Add(speaker);

            instruction = new Label();
            card.Add(instruction);

            var actions = new VisualElement();
            actions.AddToClassList("story-actions");
            card.Add(actions);
            advance = new Button(Advance) { name = "dye-advance" };
            actions.Add(advance);
            leave = new Button(Cancel) { name = "dye-leave", text = "Leave stall" };
            actions.Add(leave);

            root.Add(overlay);
            overlay.style.display = DisplayStyle.None;
        }

        public bool IsOpen => overlay.style.display == DisplayStyle.Flex;
        private bool Finished => step == Stages.Length - 1;

        public void Open(Action<bool> closed)
        {
            onClose = closed;
            step = 0;
            overlay.style.display = DisplayStyle.Flex;
            ShowStep();
            advance.Focus();
        }

        public void Advance()
        {
            if (!IsOpen)
                return;

            if (Finished)
            {
                Close(true);
                return;
            }

            step++;
            ShowStep();
        }

        public void Cancel() => Close(Finished);

        private void ShowStep()
        {
            var stage = Stages[step];
            overlay.style.backgroundImage = new StyleBackground(Resources.Load<Texture2D>("Harbor2D/" + stage.Artwork));
            instruction.text = stage.Line;
            advance.text = stage.Action;
            leave.style.display = Finished ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void Close(bool finished)
        {
            if (!IsOpen)
                return;

            overlay.style.display = DisplayStyle.None;
            overlay.style.backgroundImage = StyleKeyword.None;
            var callback = onClose;
            onClose = null;
            callback?.Invoke(finished);
        }

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
