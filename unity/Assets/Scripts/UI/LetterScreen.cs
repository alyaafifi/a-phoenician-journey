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
        private readonly Font glyphFont = Resources.Load<Font>("Harbor2D/NotoSansPhoenician");
        private readonly List<Button> slots = new List<Button>();
        private readonly Button[] tiles = new Button[Lines.Length];
        private readonly ScrollView parchment = new ScrollView();
        private readonly Label feedback = new Label("Drag an English tile to its line, or select a tile and then a line.");
        private readonly Button finish;
        private int selected = -1;
        private int solved;
        private Label ghost;

        public LetterScreen(VisualElement parent, Action completed)
        {
            screen.AddToClassList("letter-game");
            parent.Add(screen);

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
            screen.Add(parchment);

            var reference = new ScrollView();
            reference.AddToClassList("letter-reference");
            reference.Add(new Label("Alphabet reference") { name = "letter-reference-title" });
            reference.Add(new Label("Read from right to left. The key gives consonant sounds."));
            var alphabet = new VisualElement();
            alphabet.AddToClassList("letter-alphabet");
            foreach (char letter in Alphabet)
                alphabet.Add(ReferenceEntry(letter.ToString(), letter.ToString().ToUpperInvariant()));
            reference.Add(alphabet);

            var glossary = new Foldout { text = "Cargo glossary", value = false };
            foreach (var entry in Glossary)
                glossary.Add(ReferenceEntry(entry.Reading, entry.Meaning));
            reference.Add(glossary);
            reference.Add(new Label("Phoenician reconstruction · draft for review"));
            screen.Add(reference);

            var bank = new VisualElement();
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
            screen.Add(bank);
            options.Q<Button>().Focus();
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
