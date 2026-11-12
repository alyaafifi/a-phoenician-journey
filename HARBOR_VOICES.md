# Harbor voices · Programmer B

## Direction

When the student first opens his eyes in Byblos, he should hear gentle waves
washing against the shore and occasional distant birdsong. The two loops are
already shared and wired to the first opening-eye frame. They continue under
the conversation and the letter tutorial. Keep them soft enough that the
player can follow the speech.

Give the three Phoenicians distinct voices. The young worker sounds curious,
the dockworker sounds puzzled, and the harbor worker sounds practical and
welcoming. They speak quietly to one another at first. The player should still
be able to make out the words. When the harbor worker turns to the student on
“Come on, lad,” his voice should come forward and sound clear. Keep the same
voice for his aside, direct address, and bottle handoff.

The original request used Morgan Freeman as a reference for the letter
tutorial. The project user approved an original narrator instead. Aim for a
warm, deep, unhurried delivery, with clear pronunciation for a student
audience. The narrator should sound distinct from the three workers.

## Cue order

Use the exact text and cue IDs in [harbor-cues.json](tools/audio/harbor-cues.json).

**Status (12 November 2026):** all ten speech cues are generated and shared. The
cast uses ElevenLabs premade voices, which the free API plan allows: **Liam**
(young worker), **Callum** (dockworker), **Chris** (harbor worker), and **Brian**
(narrator). The steps below still apply for regenerating a line.

| Cue | Delivery | Trigger and spoken content |
| --- | --- | --- |
| `young-worker` | Quiet aside · young, curious | **Listen** after waking. “Who’s this? Have either of you seen him before?” |
| `dockworker` | Quiet aside · puzzled | After **Take his hand**. “I haven’t. Where’d he come from? I didn’t see him get off a boat.” |
| `foreman-aside` | Quiet aside · practical | First part of the harbor worker’s card. “Well, he’s here now, and we’re short a pair of hands.” |
| `foreman-address` | Clear direct speech · same actor | Immediately after the aside on the same card. “Come on, lad. Pick a trade and get to work. The dyer’s on your left. The carpenter’s on your right.” |
| `foreman-offer` | Clear direct speech · same actor | Bottle offer. “And if you want to help on the ship, take this. The order tells us what goes where.” |
| `guide-1` | Warm narrator | Read the parchment from right to left. |
| `guide-2` | Warm narrator | Find the letters using the alphabet key. Say **G, B, L** as three separate letters. |
| `guide-3` | Warm narrator | Look up the word in the cargo glossary. **G, B, L** means **Byblos**. |
| `guide-4` | Warm narrator | Match the first English tile and explain drag-and-drop and select-then-select. |
| `guide-facts` | Warm narrator | Read the three facts on consonants, the ox-head ancestor of A, and tally numbers. |

Generate clean speech without music, ambience, or a baked-in muffling effect.
[StoryAudio.cs](unity/Assets/Scripts/Audio/StoryAudio.cs) applies the quieter
aside mix at runtime. Its starting values are volume `0.47` and low-pass
`1800 Hz` for asides, and volume `1.0` with the filter open for direct speech
and narration. The wave and bird loops run independently at `0.36` and `0.18`.
Adjust by listening so the asides remain intelligible and the move to direct
speech feels natural.

## Generate and import

1. Use an ElevenLabs account with access to the selected voices through its
   Text to Speech API. The last attempt generated both ambience loops, then
   returned `HTTP 402 paid_plan_required` for the first Voice Library voice.
   Try one speech cue with an available voice before generating the rest.
   Keep the API key in your local environment. See [AUDIO_CUES.md](AUDIO_CUES.md)
   for generator setup and [ElevenLabs Voice Library documentation](https://elevenlabs.io/docs/eleven-creative/voices/voice-library)
   for API access.
2. From the repo root, generate one test cue, then the speech set once the
   cast is confirmed. Both commands leave the existing ambience loops alone.

   ```sh
   node tools/audio/generate-elevenlabs.mjs tools/audio/harbor-cues.json --only young-worker
   node tools/audio/generate-elevenlabs.mjs tools/audio/harbor-cues.json --only young-worker,dockworker,foreman-aside,foreman-address,foreman-offer,guide-1,guide-2,guide-3,guide-4,guide-facts
   ```

   The generator skips existing files. If your checkout has local preview WAVs,
   add `--force` to the speech-only command to replace them. Keep the cue
   filenames and 24 kHz mono WAV format. Files go in
   `unity/Assets/Resources/Audio/Harbor/`. Let Unity import them, then include
   the shareable WAVs and their matching `.meta` files. WAVs use Git LFS.
   Record the source and permitted use in [AUDIO_CUES.md](AUDIO_CUES.md).
   The temporary macOS speech recordings are for local preview.
3. If a spoken line changes, update the cue text and the corresponding
   on-screen line in [HarborStory.cs](unity/Assets/Scripts/UI/HarborStory.cs)
   or [LetterScreen.cs](unity/Assets/Scripts/UI/LetterScreen.cs) together.

## Play-through checks

- In Unity, open `Harbor2D` and choose **Begin → Start story**. Hear the
  classroom fade and the waves and birds enter as the eyes begin opening.
- Wait through the worker lines. Confirm the three voices are distinct, the
  asides are audible, and the foreman switches to clear speech at “Come on,
  lad.” Advancing early should replace or stop the outgoing line as appropriate.
- Take and unseal the bottle. Confirm narration on all four guide pages and
  the facts popup. Use **Back** and reopen **Reading guide** and **Alphabet
  facts** to check that the old narration stops before the new clip starts.
- Confirm the ambience continues beneath the speech and that **Skip to harbor**
  starts the ambience too.
