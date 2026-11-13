using System.Collections;
using System.Linq;
using NUnit.Framework;
using PhoenicianJourney.Audio;
using PhoenicianJourney.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace PhoenicianJourney.Tests
{
    /// <summary>
    /// The letter puzzle's Highlights button turns the glossary marks on and off, and the tip
    /// pointing at it appears after the reading guide and alphabet facts.
    /// </summary>
    public sealed class LetterHighlightTests
    {
        private GameObject host;
        private VisualElement root;
        private bool completed;

        [UnitySetUp]
        public IEnumerator OpenLetter()
        {
            host = new GameObject("Letter test", typeof(AudioListener));
            var document = host.AddComponent<UIDocument>();
            document.panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            var audio = host.AddComponent<StoryAudio>();
            yield return null;
            root = document.rootVisualElement;
#if UNITY_EDITOR
            // The highlight colours come from the shared stylesheet, as in the Harbor2D scene.
            root.styleSheets.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/Harbor.uss"));
#endif
            _ = new LetterScreen(root, () => completed = true, audio);
            yield return null;
        }

        [TearDown]
        public void Close() => Object.Destroy(host);

        private static void Click(Button button)
        {
            using var submit = NavigationSubmitEvent.GetPooled();
            submit.target = button;
            button.SendEvent(submit);
        }

        private Button PopupButton(string text) =>
            root.Query<Button>().Where(b => b.text == text).ToList().LastOrDefault();

        private VisualElement Screen => root.Q(className: "letter-game");
        private Button Toggle => root.Q<Button>("letter-highlight-toggle");
        private int HighlightedWords => root.Query(className: "letter-glossary-word").ToList().Count;

        [UnityTest]
        public IEnumerator TipPointsToTheToggleAfterTheGuide()
        {
            Assert.That(Toggle, Is.Not.Null, "the Highlights button should be on the letter screen");
            Assert.That(root.Q("letter-tip"), Is.Null, "no tip during the guide");

            foreach (var step in new[] { "Next", "Next", "Next", "Alphabet facts" })
            {
                Click(PopupButton(step));
                yield return null;
                Assert.That(root.Q("letter-tip"), Is.Null, $"no tip before the guide ends ({step})");
            }

            Click(PopupButton("Start decoding"));
            yield return null;
            Assert.That(root.Q("letter-tip"), Is.Not.Null, "the tip appears after the facts");
            Assert.That(Toggle.ClassListContains("letter-tip-target"), Is.True, "the tip spotlights the button");
            Assert.That(Toggle.parent.IndexOf(Toggle), Is.EqualTo(Toggle.parent.childCount - 1),
                "the button sits above the dimmed backdrop");

            Click(root.Q<Button>("letter-tip-ok"));
            yield return null;
            Assert.That(root.Q("letter-tip"), Is.Null, "Got it closes the tip");
            Assert.That(Toggle.ClassListContains("letter-tip-target"), Is.False);
            Assert.That(Toggle.ClassListContains("letter-tip-pulse"), Is.False);
            Assert.That(completed, Is.False);
        }

        [UnityTest]
        public IEnumerator ToggleTurnsHighlightsOffAndOn()
        {
            Assert.That(Toggle.text, Is.EqualTo("Highlights: On"));
            Assert.That(Screen.ClassListContains("letter-highlights-off"), Is.False);
            Assert.That(HighlightedWords, Is.GreaterThan(0), "glossary words are marked by default");
            var word = root.Q(className: "letter-glossary-word");
            Assert.That(word.resolvedStyle.backgroundColor.a, Is.EqualTo(1f).Within(0.01f), "marks are gold when on");

            Click(Toggle);
            yield return null;
            Assert.That(Toggle.text, Is.EqualTo("Highlights: Off"));
            Assert.That(Screen.ClassListContains("letter-highlights-off"), Is.True);
            Assert.That(word.resolvedStyle.backgroundColor.a, Is.EqualTo(0f).Within(0.01f), "marks are hidden when off");

            Click(Toggle);
            yield return null;
            Assert.That(Toggle.text, Is.EqualTo("Highlights: On"));
            Assert.That(Screen.ClassListContains("letter-highlights-off"), Is.False);
        }
    }
}
