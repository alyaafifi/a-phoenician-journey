# Audio cues

All voice lines and sound effects are generated ahead of time with [ElevenLabs](https://elevenlabs.io) and committed as files (Git LFS). The game never calls ElevenLabs while it runs. The exact prompts and settings live in cue files under `tools/audio/`, so anyone can regenerate a clip and get a matching result.

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
   ```

5. Open Unity, play `Harbor2D` from the title, and listen. Adjust the mix in `StoryAudio.cs` if needed.

## License

The ElevenLabs plan that generated the clips sets the terms of use. The free plan requires attribution and limits commercial use. Record the plan here when the clips are generated, and credit ElevenLabs in the game or presentation if the plan requires it.
