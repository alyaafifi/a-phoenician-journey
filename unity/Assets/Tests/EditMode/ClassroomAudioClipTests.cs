using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace PhoenicianJourney.Tests
{
    /// <summary>
    /// Every cue in tools/audio/classroom-cues.json has a generated clip that Unity can load,
    /// with roughly the requested length. Catches missing clips and wrong sample rates.
    /// </summary>
    public sealed class ClassroomAudioClipTests
    {
        [System.Serializable]
        private sealed class CueSheet
        {
            public Cue[] cues;
        }

        [System.Serializable]
        private sealed class Cue
        {
            public string id;
            public string type;
            public float durationSeconds;
        }

        private static CueSheet LoadSheet()
        {
            var path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "tools", "audio", "classroom-cues.json"));
            Assert.That(File.Exists(path), Is.True, $"Missing cue file at {path}");
            return JsonUtility.FromJson<CueSheet>(File.ReadAllText(path));
        }

        [Test]
        public void EveryCueHasAClipOfTheRequestedLength()
        {
            var sheet = LoadSheet();
            Assert.That(sheet.cues, Is.Not.Empty);
            foreach (var cue in sheet.cues)
            {
                var clip = Resources.Load<AudioClip>("Audio/Classroom/" + cue.id);
                Assert.That(clip, Is.Not.Null, $"No clip for {cue.id}. Generate it (AUDIO_CUES.md).");
                if (cue.type == "sfx")
                {
                    Assert.That(clip.length, Is.EqualTo(cue.durationSeconds).Within(0.5f), $"{cue.id} length");
                }
                else
                {
                    Assert.That(clip.length, Is.InRange(3f, 20f), $"{cue.id} length");
                }
            }
        }
    }
}
