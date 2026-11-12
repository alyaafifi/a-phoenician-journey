using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace PhoenicianJourney.UI
{
    internal sealed class LetterScreen
    {
        private const string Alphabet = "ʾbgdhwzḥṭyklmnsʿpṣqršt";

        // Draft reconstruction. Sources and unresolved readings are in LETTER_TRANSLATION.md.
        private static readonly (string Reading, string English)[] Lines =
        {
            ("lʾšy hʾny b gbl", "To the crew at Byblos"),
            ("lpn ṣʾtn štm šnm hmšlḥm b ʾny", "Before we cast off, see that both orders are aboard."),
            ("l kty b ʾy ʾlkš · 2 ṣrr swyt ʾrgmn", "For Kition, Cyprus · Two bundles of purple cloth"),
            ("ʿzr lṣbʿ w štm bʾṣr ybš bʾḥr hʾny", "Help the dyer finish the batch. Keep it in the dry locker at the stern."),
            ("l mṣrm · 1 ṣrr lḥt ʾrz", "For Egypt · One bundle of cedar boards"),
            ("ʿzr lmlḥm w ʾsr ṣrr lmṭ btk hʾny", "Help the carpenter prepare it. Lash it low in the middle of the hold."),
            ("bʾw ʾly k šnm hmšlḥm b ʾny", "Come find me when both orders are loaded."),
            ("rb hnpṭ", "The harbor foreman"),
        };

        private static readonly (string Reading, string Meaning)[] Glossary =
        {
            ("gbl", "Byblos"), ("ʾny", "ship"), ("kty", "Kition"), ("mṣrm", "Egypt"),
            ("swyt", "cloth"), ("ʾrgmn", "purple"), ("ʾrz", "cedar"), ("lḥt", "boards"),
            ("ṣrr", "bundle"), ("ṣbʿ", "dyer"), ("mlḥm", "carpenter"), ("ybš", "dry"),
            ("ʾḥr", "rear"), ("btk", "in the middle"), ("1", "one"), ("2", "two"),
            ("lpn", "before"), ("ṣʾtn", "our departure"), ("bʾw", "come"),
            ("hmšlḥm", "the shipments"), ("rb", "chief"), ("hnpṭ", "the harbor"),
        };

        private readonly VisualElement screen = new VisualElement { name = "letter-game" };
        private readonly VisualElement content = new VisualElement();
        private readonly Font glyphFont = Resources.Load<Font>("Harbor2D/NotoSansPhoenician");
        private readonly List<Button> slots = new List<Button>();
        private readonly Button[] tiles = new Button[Lines.Length];
        private readonly ScrollView parchment = new ScrollView();
        private readonly ScrollView reference = new ScrollView();
        private readonly VisualElement alphabet = new VisualElement();
        private readonly Foldout glossary = new Foldout { text = "Word references · Cargo glossary" };
        private readonly VisualElement bank = new VisualElement();
        private readonly Label feedback = new Label("Drag an English tile to its line, or select a tile and then a line.");
        private readonly Button finish;
        private int selected = -1;
        private int solved;
        private Label ghost;

        public LetterScreen(VisualElement parent, Action completed)
        {
            screen.AddToClassList("letter-game");
            parent.Add(screen);
            content.AddToClassList("letter-game");
            screen.Add(content);

            parchment.AddToClassList("letter-parchment");
            for (int index = 0; index < Lines.Length; index++)
            {
                int line = index;
                var row = new VisualElement();
                row.AddToClassList("letter-row");
                row.Add(Glyphs(Lines[line].Reading));
                var slot = new Button(() => Place(selected, line)) { text = "Drop English here" };
                slot.AddToClassList("letter-slot");
                slots.Add(slot);
                row.Add(slot);
                parchment.Add(row);
            }
            content.Add(parchment);

            reference.AddToClassList("letter-reference");
            var guide = new Button { text = "Reading guide" };
            guide.clicked += () => ShowGuide(0, guide.Focus);
            var facts = new Button { text = "Alphabet facts" };
            facts.clicked += () => ShowFacts("Back to the letter", facts.Focus);
            var help = new VisualElement();
            help.AddToClassList("letter-help");
            help.Add(guide);
            help.Add(facts);
            reference.Add(help);
            reference.Add(new Label("Alphabet reference") { name = "letter-reference-title" });
            reference.Add(new Label("Use the key to identify each letter."));
            alphabet.AddToClassList("letter-alphabet");
            foreach (char letter in Alphabet)
                alphabet.Add(ReferenceEntry(letter.ToString(), letter.ToString().ToUpperInvariant()));
            reference.Add(alphabet);

            glossary.value = false;
            foreach (var entry in Glossary)
                glossary.Add(ReferenceEntry(entry.Reading, entry.Meaning));
            reference.Add(glossary);
            reference.Add(new Label("Phoenician reconstruction · draft for review"));
            content.Add(reference);

            bank.AddToClassList("letter-bank");
            bank.Add(feedback);
            var options = new VisualElement();
            options.AddToClassList("letter-options");
            foreach (int line in new[] { 4, 0, 7, 2, 5, 1, 6, 3 })
            {
                var tile = new Button(() => Select(line)) { text = Lines[line].English };
                tile.AddToClassList("letter-tile");
                RegisterDrag(tile, line);
                tiles[line] = tile;
                options.Add(tile);
            }

            bank.Add(options);
            finish = new Button(completed) { text = "Look around" };
            finish.SetEnabled(false);
            bank.Add(finish);
            content.Add(bank);
            ShowGuide(0, () => options.Q<Button>().Focus());
        }

        private void ShowGuide(int step, Action done)
        {
            (string Title, string Body, VisualElement Target, string Example) lesson = step switch
            {
                0 => ("Read the parchment", "The parchment on the left holds the Phoenician letter. Start at the right edge of each line and read toward the left. Scroll the parchment to see more lines.", parchment, (string)null),
                1 => ("Find the letters", "The alphabet key on the right pairs each Phoenician sign with its Latin equivalent. In this example, the rightmost sign is G, the middle sign is B, and the leftmost is L. Read them in that order to get GBL. Phoenician usually writes consonants, so the vowels are often left out.", alphabet, "gbl"),
                2 => ("Look up the word", "The word references are in the Cargo glossary below the alphabet key. Find the same group of signs there. GBL means Byblos, so you now know the first line names the city.", (VisualElement)glossary, "gbl"),
                _ => ("Match the English tile", "The English tiles are at the bottom. When you start decoding, drag “To the crew at Byblos” to the first line’s drop box. You can also select the tile and then the box. Match all eight lines to finish.", bank, (string)null),
            };
            if (step == 1)
                reference.schedule.Execute(() => reference.ScrollTo(alphabet));
            if (step == 2)
            {
                glossary.value = true;
                reference.schedule.Execute(() => reference.ScrollTo(glossary.Q<Label>()));
            }
            ShowPopup($"{step + 1} of 4 · {lesson.Title}", new[] { lesson.Body },
                step == 3 ? "Alphabet facts" : "Next",
                () =>
                {
                    if (step == 3)
                        ShowFacts("Start decoding", () =>
                        {
                            reference.scrollOffset = Vector2.zero;
                            done();
                        });
                    else
                        ShowGuide(step + 1, done);
                }, lesson.Target, lesson.Example, step > 0 ? () => ShowGuide(step - 1, done) : null);
        }

        private void ShowFacts(string buttonText, Action next) =>
            ShowPopup("Phoenician alphabet facts", new[]
            {
                "Twenty-two letters. They mainly represented consonants, so readers supplied most vowel sounds from context.",
                "An ox became an A. The ancestor of the Phoenician letter alf was an ox-head sign. Greek writers adapted alf to write the vowel A.",
                "Numbers worked like tallies. Phoenician number signs could be repeated to build larger numbers.",
            }, buttonText, next);

        private void ShowPopup(string title, string[] paragraphs, string buttonText, Action next,
            VisualElement highlight = null, string example = null, Action back = null)
        {
            content.SetEnabled(false);
            highlight?.AddToClassList("letter-guide-highlight");
            var backdrop = new VisualElement { name = "letter-popup" };
            backdrop.AddToClassList("letter-game");
            backdrop.AddToClassList("letter-popup");
            if (highlight != null)
                backdrop.AddToClassList(highlight == parchment ? "letter-guide-right" : "letter-guide-left");
            var panel = new ScrollView();
            panel.AddToClassList("letter-popup-panel");
            var heading = new Label(title);
            heading.AddToClassList("letter-popup-heading");
            panel.Add(heading);
            if (example != null)
                panel.Add(Glyphs(example));
            foreach (string paragraph in paragraphs)
                panel.Add(new Label(paragraph));

            void Advance(Action action)
            {
                backdrop.RemoveFromHierarchy();
                highlight?.RemoveFromClassList("letter-guide-highlight");
                content.SetEnabled(true);
                action();
            }

            var actions = new VisualElement();
            actions.AddToClassList("letter-help");
            var proceed = new Button(() => Advance(next)) { text = buttonText };
            if (back != null)
                actions.Add(new Button(() => Advance(back)) { text = "Back" });
            actions.Add(proceed);
            panel.Add(actions);
            backdrop.Add(panel);
            screen.Add(backdrop);
            proceed.Focus();
        }

        private VisualElement Glyphs(string reading)
        {
            var text = new VisualElement();
            text.AddToClassList("letter-glyphs");
            foreach (char character in reading)
            {
                int index = Alphabet.IndexOf(character);
                // One glyph per element lets the right-to-left layout control order on every Unity target.
                string symbol = index >= 0 ? char.ConvertFromUtf32(0x10900 + index) : character switch
                {
                    '1' => char.ConvertFromUtf32(0x10916),
                    '2' => char.ConvertFromUtf32(0x1091A),
                    '·' => char.ConvertFromUtf32(0x1091F),
                    _ => " ",
                };
                var glyph = new Label(symbol);
                glyph.style.unityFont = glyphFont;
                text.Add(glyph);
            }
            return text;
        }

        private VisualElement ReferenceEntry(string reading, string meaning)
        {
            var entry = new VisualElement();
            entry.AddToClassList("letter-reference-entry");
            entry.Add(Glyphs(reading));
            entry.Add(new Label(meaning));
            return entry;
        }

        private void Select(int line)
        {
            selected = line;
            foreach (var tile in tiles)
                tile.RemoveFromClassList("letter-selected");
            tiles[line].AddToClassList("letter-selected");
            feedback.text = "Now choose the line this tile translates.";
        }

        private void Place(int line, int target)
        {
            if (!slots[target].enabledSelf)
                return;

            if (line < 0)
            {
                feedback.text = "Choose an English tile first.";
                return;
            }

            if (line != target)
            {
                feedback.text = "That tile belongs to another line. Check the alphabet key or glossary.";
                return;
            }

            slots[target].text = Lines[target].English;
            slots[target].SetEnabled(false);
            tiles[line].SetEnabled(false);
            tiles[line].RemoveFromClassList("letter-selected");
            selected = -1;
            solved++;
            finish.SetEnabled(solved == Lines.Length);
            feedback.text = solved == Lines.Length ? "You translated the letter." : $"Matched {solved} of {Lines.Length} lines.";
            if (solved == Lines.Length)
                finish.Focus();
            else
                Array.Find(tiles, tile => tile.enabledSelf)?.Focus();
        }

        private void RegisterDrag(Button tile, int line)
        {
            Vector2 start = default;
            tile.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 0 || !tile.enabledInHierarchy)
                    return;
                start = evt.position;
                tile.Focus();
                tile.CapturePointer(evt.pointerId);
                evt.StopImmediatePropagation();
            }, TrickleDown.TrickleDown);

            tile.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (!tile.HasPointerCapture(evt.pointerId) || (ghost == null && Vector2.Distance(start, evt.position) < 6f))
                    return;

                if (ghost == null)
                {
                    ghost = new Label(tile.text) { pickingMode = PickingMode.Ignore };
                    ghost.AddToClassList("letter-ghost");
                    screen.Add(ghost);
                }
                Vector2 position = screen.WorldToLocal(evt.position);
                ghost.style.left = position.x + 10;
                ghost.style.top = position.y + 10;
            });

            tile.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (!tile.HasPointerCapture(evt.pointerId))
                    return;

                bool dragged = ghost != null;
                RemoveGhost();
                tile.ReleasePointer(evt.pointerId);
                Select(line);
                if (dragged)
                {
                    for (int target = 0; target < slots.Count; target++)
                    {
                        var slot = slots[target];
                        if (slot.enabledInHierarchy && slot.worldBound.Contains(evt.position)
                            && parchment.contentViewport.worldBound.Contains(evt.position))
                        {
                            Place(line, target);
                            break;
                        }
                    }
                }
                evt.StopImmediatePropagation();
            }, TrickleDown.TrickleDown);
            tile.RegisterCallback<PointerCaptureOutEvent>(_ => RemoveGhost());
        }

        private void RemoveGhost()
        {
            ghost?.RemoveFromHierarchy();
            ghost = null;
        }
    }
}
