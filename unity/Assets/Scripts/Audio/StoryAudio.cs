using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhoenicianJourney.Audio
{
    /// <summary>
    /// Plays the story's voice lines, effects, and looping ambience. Clips are generated ahead
    /// of time with tools/audio/generate-elevenlabs.mjs and loaded from Resources/Audio/.
    /// A missing clip is skipped, so the story still runs silently before generation.
    /// Cue prompts and settings are in AUDIO_CUES.md and tools/audio/*-cues.json.
    /// </summary>
    public sealed class StoryAudio : MonoBehaviour
    {
        private const string ClassroomFolder = "Audio/Classroom/";

        // Relative mix for the classroom. The teacher sits on top, the room underneath.
        private const float VoiceVolume = 1f;
        private const float EffectVolume = 0.9f;
        private const float ClockVolume = 0.35f;
        private const float TrafficVolume = 0.2f;

        private readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        private readonly HashSet<string> reportedMissing = new HashSet<string>();
        private Channel voice;
        private Channel effects;
        private Channel clock;
        private Channel traffic;
        private Coroutine fade;

        /// <summary>One AudioSource with its own low-pass filter, so it can be muffled separately.</summary>
        private sealed class Channel
        {
            public Channel(Transform parent, string name, bool loop)
            {
                var go = new GameObject(name);
                go.transform.SetParent(parent, false);
                Source = go.AddComponent<AudioSource>();
                Source.playOnAwake = false;
                Source.loop = loop;
                Source.spatialBlend = 0f;
                Filter = go.AddComponent<AudioLowPassFilter>();
                Filter.cutoffFrequency = 22000f;
            }

            public AudioSource Source { get; }
            public AudioLowPassFilter Filter { get; }
            public float BaseVolume { get; set; } = 1f;

            public void Reset()
            {
                Source.Stop();
                Source.clip = null;
                Source.volume = BaseVolume;
                Filter.cutoffFrequency = 22000f;
            }
        }

        /// <summary>The cue currently playing on the voice channel, for tests.</summary>
        public string CurrentVoice { get; private set; }

        public bool AmbiencePlaying => EnsureChannels() && (clock.Source.isPlaying || traffic.Source.isPlaying);

        private void Awake() => EnsureChannels();

        /// <summary>Creates the channels on first use, whichever component's callbacks run first.</summary>
        private bool EnsureChannels()
        {
            if (voice != null)
            {
                return true;
            }

            voice = new Channel(transform, "Voice", loop: false) { BaseVolume = VoiceVolume };
            effects = new Channel(transform, "Effects", loop: false) { BaseVolume = EffectVolume };
            clock = new Channel(transform, "Ambience Clock", loop: true) { BaseVolume = ClockVolume };
            traffic = new Channel(transform, "Ambience Traffic", loop: true) { BaseVolume = TrafficVolume };
            return true;
        }

        /// <summary>Starts the ticking clock and the faint traffic outside, unless already playing.</summary>
        public void StartClassroomAmbience()
        {
            EnsureChannels();
            StopFade();
            StartLoop(clock, "clock");
            StartLoop(traffic, "traffic");
        }

        /// <summary>Plays a teacher line, cutting off the previous one.</summary>
        public void PlayTeacher(string cue)
        {
            EnsureChannels();
            StopFade();
            voice.Reset();
            CurrentVoice = null;
            var clip = Clip(cue);
            if (clip == null)
            {
                return;
            }

            voice.Source.clip = clip;
            voice.Source.Play();
            CurrentVoice = cue;
        }

        /// <summary>
        /// The student yawns, then the room muffles and fades to silence over
        /// <paramref name="seconds"/>, matching the closing eyes and the blackout.
        /// </summary>
        public void FallAsleep(float seconds)
        {
            EnsureChannels();
            var yawn = Clip("yawn");
            if (yawn != null)
            {
                effects.Reset();
                effects.Source.PlayOneShot(yawn);
            }

            StopFade();
            fade = StartCoroutine(MuffleAndFade(seconds));
        }

        /// <summary>Silences everything, for example when the player skips ahead or leaves the scene.</summary>
        public void StopAll()
        {
            EnsureChannels();
            StopFade();
            voice.Reset();
            effects.Reset();
            clock.Reset();
            traffic.Reset();
            CurrentVoice = null;
        }

        private void OnDisable()
        {
            if (voice != null)
            {
                StopAll();
            }
        }

        private void StartLoop(Channel channel, string cue)
        {
            if (channel.Source.isPlaying)
            {
                return;
            }

            channel.Reset();
            var clip = Clip(cue);
            if (clip == null)
            {
                return;
            }

            channel.Source.clip = clip;
            // Start loops at a random point so repeated visits don't sound identical.
            channel.Source.time = Random.Range(0f, clip.length * 0.9f);
            channel.Source.Play();
        }

        private IEnumerator MuffleAndFade(float seconds)
        {
            var fading = new[] { voice, clock, traffic };
            var startVolumes = new float[fading.Length];
            for (var i = 0; i < fading.Length; i++)
            {
                startVolumes[i] = fading[i].Source.volume;
            }

            for (var t = 0f; t < seconds; t += Time.unscaledDeltaTime)
            {
                // Ease in, so the room is still clear while the eyes start to close.
                var k = Mathf.SmoothStep(0f, 1f, t / seconds);
                for (var i = 0; i < fading.Length; i++)
                {
                    fading[i].Source.volume = Mathf.Lerp(startVolumes[i], 0f, k);
                    fading[i].Filter.cutoffFrequency = Mathf.Lerp(22000f, 400f, Mathf.Sqrt(k));
                }

                yield return null;
            }

            voice.Reset();
            clock.Reset();
            traffic.Reset();
            CurrentVoice = null;
            fade = null;
        }

        private void StopFade()
        {
            if (fade != null)
            {
                StopCoroutine(fade);
                fade = null;
            }
        }

        private AudioClip Clip(string cue)
        {
            if (clips.TryGetValue(cue, out var cached))
            {
                return cached;
            }

            var clip = Resources.Load<AudioClip>(ClassroomFolder + cue);
            if (clip == null && reportedMissing.Add(cue))
            {
                Debug.Log($"[StoryAudio] No clip for \"{cue}\" yet. Generate it with tools/audio/generate-elevenlabs.mjs.");
            }

            clips[cue] = clip;
            return clip;
        }
    }
}
