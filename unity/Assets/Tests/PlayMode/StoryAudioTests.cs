using System.Collections;
using NUnit.Framework;
using PhoenicianJourney.Audio;
using UnityEngine;
using UnityEngine.TestTools;

namespace PhoenicianJourney.Tests
{
    /// <summary>The classroom audio starts, plays the teacher, and fades to silence when the student falls asleep.</summary>
    public sealed class StoryAudioTests
    {
        private GameObject host;
        private StoryAudio storyAudio;

        [SetUp]
        public void CreateAudio()
        {
            host = new GameObject("StoryAudio test", typeof(AudioListener));
            storyAudio = host.AddComponent<StoryAudio>();
        }

        [TearDown]
        public void DestroyAudio() => Object.Destroy(host);

        [UnityTest]
        public IEnumerator ClassroomPlaysThenFadesToSilence()
        {
            storyAudio.StartClassroomAmbience();
            storyAudio.PlayTeacher("teacher-1");
            yield return null;
            Assert.That(storyAudio.AmbiencePlaying, Is.True, "clock and traffic should loop in the classroom");
            Assert.That(storyAudio.CurrentVoice, Is.EqualTo("teacher-1"));

            storyAudio.PlayTeacher("teacher-2");
            Assert.That(storyAudio.CurrentVoice, Is.EqualTo("teacher-2"), "a new line replaces the previous one");

            storyAudio.FallAsleep(0.3f);
            yield return new WaitForSecondsRealtime(0.5f);
            Assert.That(storyAudio.AmbiencePlaying, Is.False, "the room should be silent after falling asleep");
            Assert.That(storyAudio.CurrentVoice, Is.Null);
        }

        [UnityTest]
        public IEnumerator StopAllSilencesEverything()
        {
            storyAudio.StartClassroomAmbience();
            storyAudio.PlayTeacher("teacher-1");
            yield return null;
            storyAudio.StopAll();
            Assert.That(storyAudio.AmbiencePlaying, Is.False);
            Assert.That(storyAudio.CurrentVoice, Is.Null);
        }
    }
}
