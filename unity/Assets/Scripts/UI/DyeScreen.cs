using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace PhoenicianJourney.UI
{
    public sealed class DyeScreen
    {
        private static readonly string[] Instructions =
        {
            "Choose a plain swatch to test the dyer’s prepared bath.",
            "Dip the swatch into the vat.",
            "Lift the swatch to see its color.",
        };

        private static readonly string[] Actions = { "Select swatch", "Dip swatch", "Lift swatch" };

        private readonly VisualElement overlay;
        private readonly Label instruction;
        private readonly Label swatch;
        private readonly Button advance;
        private readonly Button leave;
        private Action<bool> onClose;
        private int step;

        public DyeScreen(VisualElement root)
        {
            overlay = new VisualElement { name = "dye-screen" };
            overlay.AddToClassList("cargo-screen");
            overlay.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);

            var panel = new ScrollView(ScrollViewMode.Vertical);
            panel.AddToClassList("cargo-panel");
            panel.AddToClassList("dye-panel");
            overlay.Add(panel);

            var title = new Label("Test a purple swatch");
            title.AddToClassList("cargo-title");
            panel.Add(title);

            instruction = new Label();
            instruction.AddToClassList("dye-instruction");
            panel.Add(instruction);

            swatch = new Label { name = "dye-swatch" };
            swatch.AddToClassList("dye-swatch");
            panel.Add(swatch);

            advance = new Button(Advance) { name = "dye-advance" };
            panel.Add(advance);
            leave = new Button(Cancel) { text = "Leave stall" };
            panel.Add(leave);

            var note = new Label("This task starts with a prepared dye bath. The Byblos stall is reconstructed from regional evidence.");
            note.AddToClassList("cargo-note");
            panel.Add(note);

            root.Add(overlay);
            overlay.style.display = DisplayStyle.None;
        }

        public bool IsOpen => overlay.style.display == DisplayStyle.Flex;

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
            {
                return;
            }

            if (step == Instructions.Length)
            {
                Close(true);
                return;
            }

            step++;
            ShowStep();
        }

        public void Cancel() => Close(step == Instructions.Length);

        private void ShowStep()
        {
            bool finished = step == Instructions.Length;
            instruction.text = finished ? "The swatch is purple. Take it back to the landing." : Instructions[step];
            advance.text = finished ? "Return to harbor" : Actions[step];
            leave.style.display = finished ? DisplayStyle.None : DisplayStyle.Flex;
            swatch.text = step switch
            {
                0 => "Plain cloth",
                1 => "Swatch selected",
                2 => "Swatch in the vat",
                _ => "Purple swatch",
            };
            swatch.EnableInClassList("dye-swatch-in-vat", step == 2);
            swatch.EnableInClassList("dye-swatch-finished", finished);
        }

        private void Close(bool finished)
        {
            if (!IsOpen)
            {
                return;
            }

            overlay.style.display = DisplayStyle.None;
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
