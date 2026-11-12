# Audio cues

The game plays local audio files. The shared classroom and harbor ambience clips were generated ahead of time with [ElevenLabs](https://elevenlabs.io) and are tracked with Git LFS. Prompts and settings are in the cue files under `tools/audio/`.

## Classroom (Harbor2D story)

The teacher drones on in a flat, slow voice while a wall clock ticks and traffic passes faintly outside. When the student's eyes close, he yawns and the room muffles and fades to silence before the blackout.

| Cue | Type | Plays | Content |
| --- | --- | --- | --- |
| `clock` | Effect, looping | From the "History class" card until the student falls asleep | Old analog wall clock ticking, one tick per second |
| `traffic` | Effect, looping | Same as `clock`, quieter | Faint distant traffic through a closed window |
| `teacher-1` | Speech | Teacher card 1 | "Now, back to Byblos. They shipped cedar, traded cloth, and sailed as far as Cyprus. We'll get to Cyprus in a minute. First, the timber..." |
| `teacher-2` | Speech | Teacher card 2 | "So where were we? Right. Once the timber reached the harbor, someone still had to load the ship. Now, if you look at the diagram..." |
| `yawn` | Effect | "Your eyes close" | One long sleepy yawn from a teenage boy |

- **Teacher voice:** a middle-aged woman with a flat, tired delivery. The stability is high (0.95), the style is 0 and the speed is 0.85, so she sounds monotone and slow. The voice ID is in `tools/audio/classroom-cues.json`.
- **Mix (in `StoryAudio.cs`):** teacher 1.0, yawn 0.9, clock 0.35, traffic 0.2. The loops start at a random point. The fade to silence takes 2.9 s, with a low-pass filter sweeping down to 400 Hz so the room sounds muffled.
- **Files:** `unity/Assets/Resources/Audio/Classroom/<cue>.wav` (24 kHz mono). A missing file is skipped, so the story still runs silently.
- **Script text:** the spoken lines must match the dialogue in `HarborStory.cs`. If the text changes, update the cue file and regenerate that line.

## Harbor and letter tutorial (Harbor2D story)

Waves and birds start at the first opening-eye frame and continue through the
letter puzzle and harbor. **Skip to harbor** starts the same ambience.

| Cues | Playback |
| --- | --- |
| `waves`, `birds` | Separate looping tracks. Gentle wash against stone, with intermittent distant bird calls. |
| `young-worker`, `dockworker` | Liam (young worker) and Callum (dockworker) speak as the workers confer. Both lines are quieter and low-pass filtered. |
| `foreman-aside`, `foreman-address`, `foreman-offer` | Chris (harbor worker) starts quietly with the others, then speaks clearly when addressing the player and handing over the bottle. |
| `guide-1` through `guide-4`, `guide-facts` | Brian, a deep and unhurried narrator, reads the guide page and alphabet facts when they open. Navigating stops the old clip. |

The exact scripts, voice IDs, and effect prompts are in
[`tools/audio/harbor-cues.json`](tools/audio/harbor-cues.json).
Harbor files go in `unity/Assets/Resources/Audio/Harbor/`. The runtime loads
local files and lets the player proceed with subtitles if a clip is missing.
All five voices are ElevenLabs **premade** voices, which the free API plan can
use. The first cast (Kaelen, Wyatt, Sawyer and a library Callum) came from the
Voice Library, which the free plan rejects with `HTTP 402 paid_plan_required`.
All ten harbor speech clips are generated and shared in the repository.

ElevenLabs sound effects return **stereo** PCM. The generator writes a stereo WAV
header when the returned size matches two channels. The classroom effects were
first saved with a mono header; they are now 24 kHz stereo, which fixes the
traffic loop playing as hiss.

## Generate or regenerate

1. One teammate with an ElevenLabs account creates an API key with Text to Speech, Sound Effects and Voices (read) permissions.
2. Set the key in a terminal session only. Never put it in a file in the repo, in `HANDOFF.md`, or in chat:

   ```
   $env:ELEVENLABS_API_KEY = "..."          # PowerShell
   export ELEVENLABS_API_KEY="..."          # macOS or Linux
   ```

3. List the voices and put the chosen `voiceId` in the cue file:

   ```
   node tools/audio/generate-elevenlabs.mjs --list-voices
   ```

4. Generate the clips. Existing files are skipped. Use `--only teacher-1,yawn` to pick specific cues, and `--force` to replace them:

   ```
   node tools/audio/generate-elevenlabs.mjs tools/audio/classroom-cues.json
   node tools/audio/generate-elevenlabs.mjs tools/audio/harbor-cues.json
   ```

5. Open Unity, play `Harbor2D` from the title, and listen. Adjust the mix in `StoryAudio.cs` if needed.

## License

The ElevenLabs plan that generated the clips sets the terms of use. The free plan requires attribution and limits commercial use. Record the plan here when the clips are generated, and credit ElevenLabs in the game or presentation if the plan requires it.

Harbor wave and bird effects generated with elevenlabs.io on the free plan for
the non-commercial prototype. See [ElevenLabs billing and usage terms](https://elevenlabs.io/docs/overview/administration/billing).
See [Apple’s macOS license agreements](https://www.apple.com/legal/sla/) for the
terms on system voices used in local previews.
