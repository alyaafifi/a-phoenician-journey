# Project handoff

## Rules for every human and AI contributor

1. Read this file and check the working tree before starting. Use the most recent entries to see what changed, what was verified, and what still needs attention.
2. Before **every** commit, add or finalize one entry that describes exactly what is going into that commit. Stage `HANDOFF.md` with the change and review the staged diff against the entry. If the staged scope changes, update the entry before committing.
3. Record who did the work, including the AI assistant or human contributor, who requested or approved it, and the actual Git author and committer if they differ. Use the name from Git for Git attribution. Keep AI assistance separate from Git authorship.
4. Record when the entry was prepared and the Git author date and time in ISO 8601 with a time zone offset. While work is pending, mark the Git author date and identity as pending. When the commit is authorized, choose the author timestamp before staging the final entry and use that same timestamp with `git commit --date`. Git records the committer timestamp and commit hash when it creates the commit. Use the commit subject and author timestamp to find the entry in Git.
5. Explain the purpose, changed files, technical behavior, decisions and rationale, and any meaningful tradeoffs. State what was verified, how it was verified, and the result. Note any remaining limitations or follow-up with enough detail for the next contributor to resume.
6. Preserve earlier entries. Add new entries above them. Keep passwords, tokens, private keys, and environment variable values out of the log.
7. Obtain explicit user authorization before committing or pushing. When authorization is pending, leave the entry marked **Pending** and the changes in the working tree.

## Entry template

```markdown
### Proposed or final commit subject

- Status: Pending / Ready for this commit
- Recorded at: YYYY-MM-DDTHH:MM:SS±HH:MM
- Work by: Human contributor(s) and AI assistant(s)
- Requested or approved by: Name or role and approved scope
- Git author: Configured name, or pending
- Git committer: Configured name if different, or pending
- Git author date: ISO 8601 with offset, or pending
- Purpose: What prompted the change and what the commit delivers
- Changes: Files, implementation details, and behavior
- Decisions: Choices, rationale, and meaningful tradeoffs
- Verification: Commands or manual steps, results, and gaps
- Follow-up: Remaining work, risks, or decisions
```

## Entries

### Add an optional Highlights toggle and tip to the letter puzzle

- **Status:** Ready for this commit
- **Recorded at:** 2026-11-13T12:09:03+02:00
- **Work by:** alyaafifi (Programmer B) with Claude Code (Claude Opus 5.5)
- **Requested or approved by:** alyaafifi asked to make the glossary highlights optional with a button at the top left, so players can choose the difficulty, and to show a tip pointing at the button and highlighting it at the end of the reading guide and facts. alyaafifi authorized the commit and push to `main`.
- **Git author:** alyaafifi
- **Git committer:** alyaafifi
- **Git author date:** 2026-11-13T12:09:03+02:00
- **Purpose:** Programmer A's glossary highlights (`fd8e41c`) make the letter puzzle easier. This lets players turn them off for a harder challenge and tells them the option exists.
- **Changes:**
  - `unity/Assets/Scripts/UI/LetterScreen.cs` (Programmer A's file):
    - **The toggle:** a **Highlights: On / Off** button (`letter-highlight-toggle`) at the top left, on by default. It sits on the screen rather than in `content`, so it stays active when `content` is disabled. Turning it off adds `letter-highlights-off` to the screen and updates the glossary note.
    - **The tip:** `ShowHighlightTip()` dims the screen, brings the button to the front with a pulsing border (class toggled every 550 ms), and shows a "Tip · Highlights" panel with a pointer arrow and **Got it**.
    - **When the tip shows:** after **Start decoding** on the facts popup, so after the opening guide and after every full replay of **Reading guide**. Opening **Alphabet facts** on its own doesn't show it.
  - `unity/Assets/UI/Harbor.uss` adds the toggle styles (gold when on, muted when off) and an off rule that removes the gold background and underline from `.letter-glossary-word`. It also adds the dimmed tip backdrop, the panel below the button, a rotated-square arrow, and the spotlight and pulse states.
  - `unity/Assets/Scripts/AssemblyInfo.cs` lets the play-mode test assembly use internal classes such as `LetterScreen`.
  - `unity/Assets/Tests/PlayMode/LetterHighlightTests.cs` adds two tests. The first checks that the tip appears only after the four guide pages and the facts, spotlights the button above the backdrop, and closes with Got it. The second checks that the toggle changes the label and screen class, and that the marks go from gold (alpha 1) to transparent and back, using the real `Harbor.uss`.
- **Decisions:**
  - Highlights default to on, so the puzzle plays as before unless the player chooses the harder mode.
  - The preference isn't saved between sessions, so the next player at a shared presentation machine starts with help on.
  - The glossary itself stays available when highlights are off; only the marks on the parchment are hidden.
  - The tip has no narration, because there's no generated clip for it. The subtitle text carries it.
  - The player can try the toggle while the tip is open.
- **Verification:**
  - Batch-mode tests with Unity 6000.6.8f1: edit-mode 24/24 and play-mode 7/7, including the two new tests.
  - The 2D story Windows build succeeded at 138.8 MB and launched with no errors in its player log.
  - The first run failed with exit code 198 ("No valid Unity Editor license found") because the Unity Hub sign-in had expired. It passed after alyaafifi signed in again.
  - Gaps: the placement over the background art and the strength of the pulse were reviewed by alyaafifi in the build, not on the presentation screen size.
- **Technical skills needed to continue this work:**
  - UI Toolkit: building elements in code, `EnableInClassList` and `ToggleInClassList`, `BringToFront`, `schedule.Execute().Every()`, `SetEnabled` and how it greys out child elements, and USS specificity, `rotate`, `scale` and transitions.
  - Game UX for optional difficulty settings and onboarding tips.
  - C# assembly visibility with `InternalsVisibleTo` and assembly definitions.
  - Unity Test Framework play-mode tests with a runtime `UIDocument` and `PanelSettings`, sending `NavigationSubmitEvent` to press buttons, and checking `resolvedStyle`.
  - Unity licensing for batch mode (Hub sign-in and Personal license).
- **Follow-up:**
  - Programmer A should review the change to `LetterScreen.cs`.
  - Optionally generate a narrator line for the tip with `tools/audio/harbor-cues.json`.
  - Check the button position and the tip on the presentation display.
  - Decide whether the tip should show on every full guide replay or only the first time.

### Make the purple dye stall a timed cloak dip

- **Status:** Pending
- **Recorded at:** 2026-11-13T11:19:33+01:00
- **Work by:** Codex
- **Requested or approved by:** Project user requested mouse dragging, a Phoenician-style cape, a seven-zone timing meter, retry on red, and lighter or darker purple depending on the lift time.
- **Git author:** Pending
- **Git committer:** Pending
- **Git author date:** Pending
- **Changes:** `DyeScreen.cs` and `Harbor.uss` add a mouse-draggable wool cloak, vat entry and lift detection, instructions, the timing meter, and seven results. The opening instructions use the clean vat backdrop with the enlarged cloak already visible beside the basket. Three illustrated stars show the current rating in gold and hold that rating on the result. A successful result still unlocks the existing purple cloth shipment. `HarborLoopTests.cs` covers a failed dip, a successful dip, and repeat play through the trade loop.
- **Decisions:** The five-second sweep uses a cosine easing curve with its highest speed in the center. Red, orange, yellow, and green use 24%, 14%, 9%, and 6% of the bar respectively on each applicable side. Lifting the cloak or releasing the mouse stops the meter. Reaching the end automatically fails the attempt. The cloak is a simple wool mantle inspired by the setting.
- **Artwork:** ImageGen edited the existing vat scene to replace the hand, sleeve, and wool swatch with matching wood grain. It also generated a separate transparent sprite of a plain wool shoulder cloak with broad folds and a small bronze clasp in the same 2D style. The two assets are `dye-vat.png` and `dye-cloak.png`.
- **Verification:** The Unity Play Mode trade loop passed, including a red result, a successful early lift, and cargo delivery. An isolated Harbor2D Play Mode smoke test used synthetic pointer events to pick up, dip, and lift the cloak and verified scores for all seven bands. A follow-up run passed both tests and checked the opening cloak layout and earned star states for every score. Manual Game view playtesting remains open.

### Keep the final parchment line above the English tiles

- **Status:** Pending
- **Recorded at:** 2026-11-13T10:59:12+01:00
- **Work by:** Codex
- **Requested or approved by:** Project user approved reducing tile padding and preventing the tile bank from covering the end of the parchment.
- **Git author:** Pending
- **Git committer:** Pending
- **Git author date:** Pending
- **Purpose:** Keep the final line's drop box reachable at the bottom of the letter.
- **Changes:** `LetterScreen.cs` updates the parchment's bottom inset when the English tile bank is laid out. `Harbor.uss` reduces vertical tile padding while keeping the text size.
- **Verification:** A Unity Play Mode smoke test checked the gap between panels and the final drop box after scrolling at 1366- and 800-unit widths. The test also completed the letter puzzle.
- **Follow-up:** Check the layout and mouse dragging in the Unity Game view.

### Add the student's hand-grab frame

- **Status:** Ready for this commit
- **Recorded at:** 2026-11-13T10:53:04+01:00
- **Work by:** Codex
- **Requested or approved by:** Project user requested the missing frame from the supplied storyboard and asked to match the student's clothing across the scene. The project user authorized the commit and push.
- **Git author:** anthonymerhy
- **Git committer:** anthonymerhy
- **Git author date:** 2026-11-13T10:53:04+01:00
- **Purpose:** Show the student actually taking the worker's hand before the standing crew shot.
- **Changes:** `HarborStory.cs` shows `hand-grab.jpg` after **Take his hand**, then **Continue** advances to the standing crew and resumes the dockworker's dialogue. The frame uses page 11 of the supplied storyboard as its base, with an ImageGen edit to match the student's dark navy sweatshirt and ribbed cuff from the bottle shot.
- **Verification:** Inspected the original page and the wardrobe edit. An isolated Unity Play Mode smoke test passed the transition from the offered hand to the hand grab and then the standing crew. Checked the working diff.

### Highlight glossary terms on the Phoenician letter

- **Status:** Ready for this commit
- **Recorded at:** 2026-11-13T10:48:21+01:00
- **Work by:** Codex
- **Requested or approved by:** Project user requested highlights for glossary terms in the letter minigame, requested exact glossary matches, and authorized the commit and push.
- **Git author:** anthonymerhy
- **Git committer:** anthonymerhy
- **Git author date:** 2026-11-13T10:48:21+01:00
- **Purpose:** Help players identify which words they can look up.
- **Changes:** `LetterScreen.cs` groups the right-to-left glyphs by word and highlights matches from the existing glossary, including the exact glossary stems in four prefixed forms. The attached letters stay visible outside the highlight. `Harbor.uss` gives matches a gold background and dark underline.
- **Verification:** The isolated Unity Play Mode test verified that every highlighted glyph sequence exactly matches a glossary entry and that the attached letter on the ship term sits outside the highlight. The test also completed the walkthrough and puzzle. Checked the working diff.
- **Follow-up:** Review highlight contrast and line wrapping in the Unity Game view.

### Generate the harbor voice-overs and fix stereo classroom effects

- **Status:** Ready for this commit
- **Recorded at:** 2026-11-12T15:49:43+02:00
- **Work by:** alyaafifi (Programmer B) with Claude Code (Claude Opus 5.5)
- **Requested or approved by:** alyaafifi asked to follow `HARBOR_VOICES.md` and fix the harbor voice-overs, generate them all with ElevenLabs, run the result, and commit and push to `main`.
- **Git author:** alyaafifi
- **Git committer:** alyaafifi
- **Git author date:** 2026-11-12T15:49:43+02:00
- **Purpose:** Replace the subtitle-only harbor dialogue and letter tutorial with real voices, as Programmer A asked in `HARBOR_VOICES.md`, and fix a playback bug in the classroom effects.
- **Changes:**
  - `tools/audio/harbor-cues.json` recasts all four voices with ElevenLabs **premade** voices, which the free API plan allows:
    - **Liam** (`TX3LPaxmHKxFdv7VOQHJ`, young and energetic) as the young worker;
    - **Callum** (`N2lVS1w4EtoT3dr4eOWO`, husky, middle-aged) as the dockworker;
    - **Chris** (`iP95p4xoKVk53GoZ742B`, down-to-earth) as the harbor worker;
    - **Brian** (`nPczCjzI2devNBz1zQrb`, deep and comforting) as the narrator.

    The text, models and voice settings are unchanged. The earlier Voice Library voices returned `HTTP 402 paid_plan_required`, and the dockworker ID had 19 characters, one short.
  - `unity/Assets/Resources/Audio/Harbor/` adds the ten speech clips with their `.meta` files, 24 kHz mono WAV in Git LFS: `young-worker` 2.9 s, `dockworker` 4.8 s, `foreman-aside` 2.6 s, `foreman-address` 5.9 s, `foreman-offer` 4.3 s, `guide-1` 9.1 s, `guide-2` 20.7 s, `guide-3` 12.3 s, `guide-4` 12.8 s, `guide-facts` 23.6 s. The existing `waves` and `birds` were left as they were.
  - `unity/Assets/Resources/Audio/Classroom/clock.wav`, `traffic.wav` and `yawn.wav` get corrected WAV headers: **24 kHz stereo**, where earlier versions had 24 kHz mono and then 48 kHz mono. The audio samples are unchanged. Sample-pair analysis showed interleaved stereo. The traffic's two channels differ, so as mono it played as hiss. The lengths are still exactly 10, 15 and 3 s. Programmer A's generator already detects stereo effects for future runs.
  - `unity/Assets/Editor/BuildScript.cs` adds **Tools ▸ Phoenician Journey ▸ Build Windows (2D story)** (`BuildWindowsStory`), which builds `Assets/Scenes/Harbor2D.unity` to `Builds/WindowsStory/APhoenicianJourney.exe`. `Build()` now takes an optional scene. The 3D `Build Windows` is unchanged.
  - `AUDIO_CUES.md`, `HARBOR_VOICES.md` and `SOURCES.md` name the generated cast, drop the note that harbor speech was unavailable, and record the stereo finding.
- **Decisions:**
  - Premade voices instead of Voice Library voices, so a free account can generate and regenerate every line. Each voice was matched to the direction in `HARBOR_VOICES.md`, and all four sound different from each other and from Alice, the teacher.
  - Fixed the classroom headers locally instead of regenerating, so no credits were spent.
  - Added a separate 2D build instead of switching the default build, so the 3D build stays available, as decided on 12 November.
- **Verification:**
  - Checked every clip's sample rate, length, peak and RMS.
  - The stereo diagnosis compared differences between left/right sample pairs and neighbouring frames, for all five effect files.
  - Batch-mode tests with Unity 6000.6.8f1: edit-mode 24/24 and play-mode 5/5.
  - The 2D story Windows build succeeded at 137.5 MB and launched in a window with no errors in its player log. alyaafifi played it.
  - Gaps: the mix wasn't fine-tuned against `HARBOR_VOICES.md`'s checks (aside clarity, the move to clear speech, ambience level) beyond the play-through.
- **Technical skills needed to continue this work:**
  - ElevenLabs voice types (premade, Voice Library and cloned) and which ones each plan can use through the API, plus listing voices and choosing settings.
  - PCM and WAV structure: channels, sample rate, byte rate, block align, interleaved stereo, and checking an audio file's format from its data.
  - Unity audio import, `AudioSource` mixing and low-pass filtering in `StoryAudio`.
  - Unity build pipeline: `BuildPlayerOptions`, scene lists, and `MenuItem` editor scripts.
  - Node.js for the generator, plus terminal environment variables for the API key.
- **Follow-up:**
  - Tune the aside volume (0.47) and low-pass (1800 Hz), and the wave and bird levels in `StoryAudio.cs`, after listening.
  - Decide whether `Harbor2D` should become the default build scene.
  - Delete the ElevenLabs API key that was exposed in a terminal on 12 November, and create a new one.
  - Record the ElevenLabs plan and attribution in `AUDIO_CUES.md` before the presentation.

### Document the harbor voice direction for Programmer B

- **Status:** Ready for this commit
- **Recorded at:** 2026-11-12T14:37:51+01:00
- **Work by:** Codex
- **Requested or approved by:** Project user requested a Programmer B voice brief based on the prior harbor and narrator direction, and authorized the commit and push.
- **Git author:** anthonymerhy
- **Git committer:** anthonymerhy
- **Git author date:** 2026-11-12T14:37:51+01:00
- **Purpose:** Give Programmer B the creative and technical details needed to produce shareable harbor dialogue and tutorial narration.
- **Changes:** `HARBOR_VOICES.md` documents the wake-up soundscape, three worker voices, aside-to-direct transition, original narrator, ten speech cues, generation and import workflow, and Unity listening checks.
- **Decisions:** Reuse the existing cue sheet, ambience loops, and runtime mixing. Try a single speech cue before generating the full set.
- **Verification:** Checked the brief against the runtime and cue sheet, verified all ten speech cue IDs and repository links, and checked the staged diff for whitespace.
- **Follow-up:** Programmer B can choose accessible voices, generate shareable speech, and verify the mix in Unity.

### Play the illustrated purple dye sequence from the Byblos stall

- **Status:** Ready for this commit
- **Recorded at:** 2026-11-12T14:35:48+01:00
- **Work by:** Codex
- **Requested or approved by:** Project user approved the five-step flow and asked to wire it to the harbor’s purple dye area. Project user authorized the commit and push.
- **Git author:** anthonymerhy
- **Git committer:** anthonymerhy
- **Git author date:** 2026-11-12T14:35:48+01:00
- **Purpose:** Let players learn the source and value of purple dye while helping the Byblos dyer test a prepared bath.
- **Changes:** The left hotspot in `HarborStory.cs` now covers the shell basket. `DyeScreen.cs` and `Harbor.uss` show a dyer’s card over five ImageGen frames in `Assets/Resources/Harbor2D`: murex, plain wool, selected wool, dipped wool, and purple reveal. The existing completion callback still unlocks the prepared cloth batch. The 3D harbor uses the same screen, and its trade-loop test now advances through the murex introduction.
- **Decisions:** Reuse the five reviewed frames and keep controls and dialogue in UI Toolkit. The dialogue places the player at a prepared bath and explains the source, labor, and value without staging a recipe. Preserve each texture’s proportions during import.
- **Verification:** In an isolated Unity 6000.6.8f1 copy, the 2D smoke test verified the basket hotspot bounds, cancellation and reopen, the artwork and control at each stage, and the cloth-ready banner. The 3D trade loop and station discovery passed. The 3D spawn grounding assertion varied at the uncapped batch frame rate while the player stayed at the same position. All four targeted tests passed in a temporary 60 fps simulation run.
- **Follow-up:** Review mouse input and the dialogue card in the Unity Game view.

### Prepare harbor ambience and letter narration

- **Status:** Ready for this commit
- **Recorded at:** 2026-11-12T14:35:21+01:00
- **Work by:** Codex
- **Requested or approved by:** Project user requested waves and birds, distinct harbor voices with quieter worker asides, and approved an original voice for the letter walkthrough. Project user authorized the commit and push.
- **Git author:** anthonymerhy
- **Git committer:** anthonymerhy
- **Git author date:** 2026-11-12T14:35:21+01:00
- **Purpose:** Make the transition into Byblos audible and narrate the first minigame.
- **Changes:** `StoryAudio.cs` adds wave and bird loops and separate clear and muffled voice playback. `HarborStory.cs` starts the ambience when the student begins opening his eyes and plays the worker cues. `LetterScreen.cs` starts and stops narration with its guide and facts pages. `tools/audio/harbor-cues.json` defines the twelve cues, and the cue sheet and source register document them.
- **Decisions:** Keep subtitles and use a local-clip workflow. The intended ElevenLabs cast is Sawyer for narration and Kaelen, Callum, and Wyatt for the three workers. Keep the wave and bird loops independent from voice playback.
- **Verification:** The isolated Unity Play Mode run passed the classroom audio and letter-flow tests. The harbor cue sheet parses as JSON and the generator passes Node's syntax check.
- **Follow-up:** Waves and birds generated successfully. Corrected their WAV headers to preserve stereo playback at the requested 16 and 21 seconds, and updated the generator to recognize that PCM layout. ElevenLabs rejected the first Voice Library request on the free API plan. With the project user’s approval, generated ten temporary speech clips locally with macOS voices (Eddy, Rocko, Daniel, Grandpa). Verified the WAV files and Unity metadata, and launched Harbor2D at its title screen. The temporary speech stays local under Apple’s system-voice usage terms. This commit includes the two ElevenLabs ambience WAVs, with attribution in the cue sheet. Generate shareable speech when suitable ElevenLabs access is available, then review the mix in Unity.

### Add ElevenLabs classroom audio to the 2D story

- **Status:** Ready for this commit
- **Recorded at:** 2026-11-12T14:09:02+02:00
- **Work by:** alyaafifi (Programmer B) with Claude Code (Claude Opus 5.5)
- **Requested or approved by:** alyaafifi asked for classroom audio from ElevenLabs: the teacher reading the script in a boring, monotone voice, a ticking clock, faint traffic outside, and the student's yawn before he falls asleep. alyaafifi authorized this commit and the push to the new `classroom-audio` branch.
- **Git author:** alyaafifi
- **Git committer:** alyaafifi
- **Git author date:** 2026-11-12T14:09:02+02:00
- **Purpose:** Make the boredom of the classroom audible, so falling asleep and waking in Byblos lands in the `Harbor2D` story.
- **Changes:**
  - `unity/Assets/Scripts/Audio/StoryAudio.cs` is a new component with four channels (voice, effects, clock loop, traffic loop), each with its own low-pass filter.
    - `StartClassroomAmbience()` starts both loops at a random point.
    - `PlayTeacher(cue)` replaces the current line.
    - `FallAsleep(seconds)` plays the yawn, then lowers the volume and sweeps the low-pass filter from 22 kHz to 400 Hz on the voice and loops, so the room sounds muffled.
    - `StopAll()` silences everything.
    - The mix is teacher 1.0, yawn 0.9, clock 0.35 and traffic 0.2.
    - Clips load from `Resources/Audio/Classroom/`. A missing clip is skipped with one log message, so the story still runs silently.
  - `unity/Assets/Scripts/UI/HarborStory.cs` (Programmer A's file) gets a few added calls and no changes to existing behaviour or text. It adds the `StoryAudio` component when enabled. The title and menu stop all audio. The "History class" card starts the ambience. The two teacher cards play `teacher-1` and `teacher-2`. "Your eyes close" calls `FallAsleep(2.9f)`, matching the existing 1.1 s + 1 s + 0.8 s eyelids and blackout. The one-line methods it touches became block bodies.
  - `unity/Assets/Resources/Audio/Classroom/` holds `teacher-1.wav` (10.4 s) and `teacher-2.wav` (9.8 s), 24 kHz mono, and `clock.wav` (10 s), `traffic.wav` (15 s) and `yawn.wav` (3 s), 48 kHz mono. They are stored in Git LFS.
  - `tools/audio/classroom-cues.json` holds the exact spoken text, which matches `HarborStory.cs`, and the effect prompts, durations, loudness targets and output formats. The teacher voice is Alice (`Xb7hH8MSUJpSbSDYk0k2`, British, middle-aged educator) with stability 0.95, style 0, speed 0.85 and no speaker boost, for a flat, slow delivery.
  - `tools/audio/generate-elevenlabs.mjs` is a dependency-free Node script with `--list-voices`, generation from a cue file, `--only` and `--force`. It skips existing clips to save credits, wraps PCM in WAV, normalizes effects, and reads the key only from `ELEVENLABS_API_KEY`.
  - `AUDIO_CUES.md` is the cue sheet: what plays when, voice and mix settings, regeneration steps, and the license note. `SOURCES.md` now registers the ElevenLabs audio.
  - Tests: `Tests/EditMode/ClassroomAudioClipTests.cs` checks that every cue has a loadable clip of the requested length. `Tests/PlayMode/StoryAudioTests.cs` checks that the ambience and teacher play, that a new line replaces the old one, that falling asleep ends in silence, and that `StopAll()` works.
  - `unity/ProjectSettings/PackageManagerSettings.asset` is a project settings file Unity created; it was untracked.
- **Decisions:**
  - Generated once and committed, so the game never calls ElevenLabs and ships no key.
  - WAV instead of MP3, so the loops have no encoder gap at the loop point.
  - The sound effects come back at 48 kHz even when 24 kHz is requested. The first files had a 24 kHz header and would have played at half speed and an octave too low. The headers were corrected locally and the clock (peak 0.04) was normalized. The generator now requests 48 kHz for effects and normalizes them, with no extra credits spent.
  - The audio sits in its own component, so Programmer A's story script only gains one-line cue calls.
  - Programmer A's two new commits on `main` (`efd41ce` and `829b345`) were merged in before committing. They don't touch the classroom or the sleep timing.
- **Verification:**
  - Checked the generated files: durations, peaks, and the clock's tick spacing, which confirmed 48 kHz.
  - Batch-mode compile on Windows 11 with Unity 6000.6.8f1 had no errors or warnings.
  - Edit-mode tests passed 24/24 and play-mode tests passed 5/5.
  - The tests ran before Programmer A's two latest commits were merged. Those commits change other parts of `HarborStory.cs`, so they don't overlap; I didn't recompile after the merge because Unity was open in the editor.
  - Gaps: alyaafifi hasn't yet listened in the Game view for mix, voice and timing.
- **Technical skills needed to continue this work:**
  - The ElevenLabs API: text to speech with voice IDs, models and voice settings; sound generation with duration, prompt influence and loop; and output formats and sample rates.
  - Digital audio basics: PCM, WAV headers, sample rates, peak and RMS levels, normalization, and loop points.
  - Unity audio: `AudioSource` (loop, `PlayOneShot`, volume, time), `AudioLowPassFilter`, `Resources.Load<AudioClip>`, and import settings.
  - C# coroutines and fades with `Time.unscaledDeltaTime`, plus component lifecycle (`Awake`, `OnDisable`, lazy setup).
  - Node.js scripting with `fetch`, Buffers and environment variables, and handling secrets safely.
  - Unity Test Framework edit-mode and play-mode tests.
- **Follow-up:**
  - Listen in Unity: `Harbor2D` ▸ Play ▸ Begin ▸ Start story. Tune the volumes in `StoryAudio.cs`, or regenerate a line with `--only <cue> --force`.
  - Record the ElevenLabs plan and attribution terms in `AUDIO_CUES.md`.
  - The API key used for generation was exposed in a terminal session. Delete it in ElevenLabs and create a new one for future runs.
  - If Programmer A changes the teacher's text in `HarborStory.cs`, update `tools/audio/classroom-cues.json` and regenerate that line.
  - Merge `classroom-audio` into `main` after review.

### Add translation instructions and a project source register

- **Status:** Ready for this commit
- **Recorded at:** 2026-11-12T13:06:46+01:00
- **Work by:** Codex
- **Requested or approved by:** Project user requested an instruction popup, alphabet facts, and a structured source register for the repository. The project user authorized the commit and push.
- **Git author:** anthonymerhy
- **Git committer:** anthonymerhy
- **Git author date:** 2026-11-12T13:06:46+01:00
- **Purpose:** Teach players how to read the letter and use the reference tools, share alphabet facts, and give contributors one place to find project references.
- **Changes:** `LetterScreen.cs` and `Harbor.uss` add a four-step guide highlighting the parchment, alphabet key, word references, and English tile bank. The guide decodes GBL into Byblos and demonstrates the first English match. It ends with three alphabet facts before play. The reference panel can reopen the guide and facts. `SOURCES.md` records alphabet, letter, harbor, craft, cargo, art, and setup sources with their uses and limits. The README and letter notes link to the register.
- **Decisions:** Disable puzzle interaction while a popup is open and focus its action button. Use a worked example from the first line to distinguish identifying letters from translating a word.
- **Verification:** An isolated Unity Play Mode smoke test verified guide navigation, highlights, glossary expansion, facts before play, completing the puzzle with keyboard input, reopening the facts, and proceeding to the harbor. Checked source coverage against the existing historical documents and verified repository links.
- **Follow-up:** Check popup layout with the mouse in the Unity Game view.

### Restore the helping-hand storyboard frame

- **Status:** Ready for this commit
- **Recorded at:** 2026-11-12T13:06:46+01:00
- **Work by:** Codex
- **Requested or approved by:** Project user requested the reaching-hand shot between the workers leaning over the student and standing upright, then approved an ImageGen reconstruction to replace the blurry preview. The project user authorized the commit and push.
- **Git author:** anthonymerhy
- **Git committer:** anthonymerhy
- **Git author date:** 2026-11-12T13:06:46+01:00
- **Purpose:** Show how the student gets to his feet before the standing crew shot.
- **Changes:** `HarborStory.cs` inserts a **Take his hand** beat between the young worker’s question and the dockworker’s reply. `helping-hand.jpg` is a 1672 × 940 reconstruction of the shot from page 7 of the supplied storyboard.
- **Decisions:** Use the sharp wake-up shot as the base and the original storyboard preview as a pose reference. Keep the established crew, viewpoint, and harbor lighting.
- **Verification:** Visually checked the reconstructed image and confirmed its dimensions. A Unity Play Mode smoke test imported the replacement and advanced from the young worker’s question to the helping-hand texture and then to the standing crew texture. Checked the working diff.
- **Follow-up:** Review the shot alongside the surrounding frames in the Unity Game view.

### Add the Phoenician letter translation minigame

- **Status:** Ready for this commit
- **Recorded at:** 2026-11-12T12:17:49+01:00
- **Work by:** Codex
- **Requested or approved by:** Project user requested a drag-and-drop English translation with a Phoenician alphabet reference, approved using a sourced draft, and limited implementation to the letter minigame. The project user authorized the commit and push.
- **Git author:** anthonymerhy
- **Git committer:** anthonymerhy
- **Git author date:** 2026-11-12T12:17:49+01:00
- **Purpose:** Make decoding the bottle's letter the first minigame in the illustrated story.
- **Changes:** `HarborStory.cs` opens `LetterScreen.cs` after unsealing the bottle. The new screen displays right-to-left Phoenician, an alphabet key and cargo glossary, and English tiles matched to all eight lines. It supports drag-and-drop and tile-then-line selection. Added illustrated bottle and letter frames with modern sweatshirt cuffs, the licensed Phoenician font, and `LETTER_TRANSLATION.md` with sources.
- **Decisions:** Require the complete translation before revealing the harbor. Use the agreed cloth and cedar order in the letter. Cargo integration is a separate change, so the current cargo interface continues to use its existing cloth and wine order.
- **Verification:** Unity compiled the scripts. A Play Mode smoke test in an isolated project copy verified the artwork and font imports, rejected a wrong match, completed all eight lines with keyboard events, and opened the harbor.
- **Follow-up:** Review the reconstructed wording with a Phoenician specialist, try mouse dragging and check the visual layout in the Unity Game view, and connect the cedar order to cargo when requested.

### Refine the classroom and wake-up transition

- **Status:** Ready for this commit
- **Recorded at:** 2026-11-12T11:44:46+01:00
- **Work by:** Codex
- **Requested or approved by:** Project user requested matching teacher shots and slower sleep and wake beats, and approved fading blurred frames into the sharp originals. The project user authorized the commit and push.
- **Git author:** anthonymerhy
- **Git committer:** anthonymerhy
- **Git author date:** 2026-11-12T11:44:46+01:00
- **Purpose:** Keep the teacher composition consistent and show the student's vision coming into focus in Byblos.
- **Changes:** `HarborStory.cs` uses the second teacher shot for both dialogue beats, extends the sleep and wake sequence from 3.3 to 5.4 seconds, and fades a downsampled copy of each wake-up shot over the original. The wake-up phase lasts 2.5 seconds after the blackout.
- **Decisions:** Use the existing artwork at every step. Generate the blurred copies at runtime and release them when each shot ends or the scene is disabled. Preserve all dialogue and button text.
- **Verification:** Unity recompiled and reloaded the project assembly after the blur implementation. Checked the dialogue and button strings against the previous version. Checked the final timing values and working diff.
- **Follow-up:** Play through the transition in the Unity Game view to judge the timing and strength of the blur.

### Add the illustrated 2D harbor story prototype

- **Status:** Ready for this commit
- **Recorded at:** 2026-11-12T11:14:20+01:00
- **Work by:** Codex
- **Requested or approved by:** Project user requested a first 2D implementation from the designer’s storyboard and authorized the commit and push.
- **Git author:** anthonymerhy
- **Git committer:** anthonymerhy
- **Git author date:** 2026-11-12T11:14:20+01:00
- **Purpose:** Play the storyboard opening and the existing trade loop from an illustrated harbor in Unity.
- **Changes:** `Assets/Scenes/Harbor2D.unity` adds a separate playable scene. `HarborStory.cs` implements title, classroom, blink, wake-up, bottle and order, then offers purple dye on the left, cedar carpentry on the right, and loading and trade routes at the ship. The scene reuses `DyeScreen`, `CargoScreen`, and one session-local trade state. Added setup and play instructions to `README.md` and scene styles to `Harbor.uss`. The frames in `Assets/Resources/Harbor2D` come from the supplied storyboard PDF and JPG, including the occupied classroom, teacher cutout and menu panel. The bottle and pointer icons are from the supplied icons PDF.
- **Decisions:** Keep students visible in all classroom shots and hold the two closing-eyelid frames for 950 and 850 ms before a 650 ms blackout. The teacher rambles about Phoenician history before the student dreams of life in Byblos. The harbor workers wonder who the newcomer is, then ask him to pick a trade. Use temporary dialogue and button-based plank fitting. The ship provides the order and route descriptions from the existing cargo data. Keep the existing build target while the 2D prototype is reviewed. Add a scene-local EventSystem and Input System UI module for mouse input.
- **Verification:** Unity compiled the revised scene and imported the supplied crew frame. In Play mode, clicked through the title and menu and confirmed that seated students remain visible behind the teacher during her lecture. Checked the revised asset references, station assignments, and working diff.
- **Follow-up:** Play through the slower eyelid transition and revised harbor dialogue, then continue to a successful shipment. Replace annotated storyboard close-ups and confirm dialogue before presentation. Confirm the default build scene and any remaining settings icons separately.

### Build the Unity purple-dye minigame

- **Status:** Ready for this commit
- **Recorded at:** 2026-11-12T11:14:04+01:00
- **Work by:** Codex
- **Requested or approved by:** Project user approved implementing the select, dip and lift interaction and treating the swatch as a sample that unlocks a prepared cloth batch. Project user authorized the commit and push.
- **Git author:** anthonymerhy
- **Git committer:** anthonymerhy
- **Git author date:** 2026-11-12T11:14:04+01:00
- **Purpose:** Replace the Unity dye stall's mock result with a playable Sprint 2 interaction.
- **Changes:** `DyeScreen.cs` and `Harbor.uss` provide the UI Toolkit screen and swatch reveal. `HarborGame.cs` opens the screen at the dye stall, pauses movement, and sends the completed result to the shared trade state. Updated the Unity instructions and extended the existing Harbor play-mode test for cancellation and repeat completion.
- **Decisions:** Reuse the existing harbor UI document and four-bundle trade-state setting. Return to the harbor after showing the finished swatch. Closing before the swatch is lifted does not unlock cargo.
- **Verification:** Unity 6000.6.8f1 compiled on Apple Silicon macOS. Passed 23 EditMode and 3 PlayMode tests, including early exit, replay, and shipment in the harbor loop. Checked the working diff for whitespace.
- **Follow-up:** Play through dye and cargo interactively on the Mac. Check keyboard and mouse focus, the swatch reveal, and movement after closing the screen.

### Port the harbor and cargo loop to Unity

- **Status:** Ready for this commit
- **Recorded at:** 2026-11-11T16:14:02+02:00
- **Work by:** alyaafifi (Programmer B) with Claude Code (Claude Opus 5.5)
- **Requested or approved by:** alyaafifi decided to move the prototype from Babylon.js to Unity, asked for `UNITY_MIGRATION.md` so Programmer A is up to date, and authorized the commit and push after the project compiled and its tests passed.
- **Git author:** alyaafifi
- **Git committer:** alyaafifi
- **Git author date:** 2026-11-11T16:14:02+02:00
- **Purpose:** Switch the game engine to Unity, keep everything Sprint 1 and Sprint 2 delivered, and give Programmer A a clear path to set up and continue in Unity.
- **Changes:**
  - `unity/` is a new Unity 6.6 (6000.6.8f1) project. Its packages are glTFast 6.21.0, Input System 1.20.1, Test Framework 1.8.0, uGUI, and the IDE integrations. The project uses the Input System only (`activeInputHandler: 1`), and analytics and performance reporting are off.
  - `Assets/Scripts/Trade/` (`TradeCatalog.cs`, `Shipment.cs`, `TradeState.cs`) is a line-by-line C# port of the goods, destinations, hold positions, sample order, `Check()` rules, dockworker messages, and the dye-to-cargo contract. `TradeState.Shared` resets when play starts.
  - `Assets/Scripts/Landing/` holds `LandingNaming.cs`, the unchanged Blender convention (`COL_`, `NOCOL_`/`water`, `SPAWN`, `cargo`/`dye`/`scribe`), and `Station.cs`, a footprint component with a cached bounds and a reach of 1.5 m.
  - `Assets/Scripts/Player/FirstPersonWalker.cs` is first-person movement on a `CharacterController`, using the prototype's values: 1.7 m body, 1.6 m eye, 0.35 m step, 40° slope limit, 2.2 and 5 m/s, wading limit at −0.5 m, and respawn below −10 m. You look around with the cursor captured, or by dragging. `SetMovementEnabled()` pauses movement and frees the cursor.
  - `Assets/Scripts/UI/CargoScreen.cs` and `Assets/UI/Harbor.uss` rebuild the cargo screen in UI Toolkit: the letter with the flagged line, goods cards, the stern and amidships hold diagram, dockworker feedback, the sent summary, and the evidence note. Its public actions double as test hooks. `HarborTheme.tss` imports the default runtime theme.
  - `Assets/Scripts/HarborGame.cs` runs the HUD, station prompts, E and R keys, and station actions. The dye mock runs in the editor, in development builds, or with `mockDye` in the URL.
  - `Assets/Editor/HarborSceneBuilder.cs` (**Tools ▸ Phoenician Journey ▸ Rebuild Harbor Scene**) generates `Assets/Scenes/Harbor.unity` from `Assets/Art/harbor-landing.glb`, a Git LFS copy of the Sprint 1 browser export. It adds mesh colliders by name, hides `COL_` meshes, adds stations, places the player at `SPAWN` facing the cargo station, and sets up lighting and UI. It also sets the build scene list and baked collision meshes.
  - `Assets/Editor/BuildScript.cs` adds **Build Windows** (Mono, `Builds/Windows/APhoenicianJourney.exe`) and **Build Web** (Gzip with decompression fallback).
  - `Assets/Tests/` has 23 edit-mode tests (the 12 ported trade tests plus dockworker wording and naming) and 3 play-mode tests: grounded at spawn, stations found, and the full loop.
  - `unity/.gitignore` excludes `Library`, `Temp`, `Logs`, `UserSettings`, `Builds`, and IDE files. The root `.gitattributes` adds Unity Smart Merge for `.unity`, `.prefab`, and `.asset` files, and Git LFS for new binary art, audio, and fonts. Files committed earlier are unchanged.
  - `UNITY_MIGRATION.md` covers status, machine setup (Unity version, Git LFS, Smart Merge), layout, the Blender-to-Unity workflow, the dye minigame contract in C#, file ownership, run, test and build steps, a mapping from each prototype file to its Unity replacement, technical choices, and open items. `TECHNICAL_DECISIONS.md` and `README.md` now describe Unity as the current stack, with the Babylon.js prototype being retired.
- **Decisions:**
  - Unity 6.6 instead of 6.3 LTS, because 6.6 was already downloaded and the code uses nothing that differs between them. All teammates must use 6000.6.8f1.
  - A Windows build instead of a web build, because this machine had too little disk space for Web Build Support (5.6 GB). The web build stays available in the build script.
  - The harbor scene is generated, not hand-edited, so a new Blender export only needs a rebuild and nobody merges scene files.
  - The built-in render pipeline and the Mono backend keep the setup small.
  - Removed Unity's default extras: Analytics, In-App Purchasing, Version Control, Multiplayer Center, XR legacy input, 2D Sprite/Tilemap, AI Navigation, and Timeline, along with the purchasing leftover `Assets/Resources/BillingMode.json`.
  - The Babylon.js code stays in this commit, so the old build still runs until Programmer A has moved over.
- **Verification:**
  - Batch-mode compile on Windows 11 with Unity 6000.6.8f1 had no errors or warnings, after fixing two Unity 6.6 deprecations.
  - Rebuilding the scene reported 7 solid meshes, 0 hidden colliders, 1 mesh without collision, and 3 stations, with the spawn at (0.00, 2.40, −17.00) facing 351°.
  - Edit-mode tests passed 23/23 and play-mode tests passed 3/3, both before and after removing the default packages. One test failed at first because NUnit's `Has.Count` doesn't work on arrays; I fixed it.
  - The Windows build succeeded at 95.4 MB, after retrying an internal build-system error caused by a leftover backend process. The built `.exe` ran for 15 seconds with no errors in its player log.
  - Gaps: I didn't play by hand with keyboard and mouse, so the look sensitivity, walking feel and cargo-screen layout are only checked by code and the log. The web build and macOS weren't tested.
- **Technical skills needed to continue this work:**
  - Unity: the editor, Unity Hub version management, scenes and GameObjects, components and `MonoBehaviour` lifecycle, the Inspector, prefabs, packages and `manifest.json`, assembly definitions, Player settings, and building for Windows and Web.
  - C#: classes and enums, nullable value types, events, LINQ, collections, and `readonly` and `const`.
  - Unity physics and movement: `CharacterController` (step offset, slope limit, `Move`), `MeshCollider`, bounds, and frame-rate-independent movement with `Time.deltaTime`.
  - Unity Input System: `Keyboard.current` and `Mouse.current`, cursor lock, and active input handling.
  - UI Toolkit: `UIDocument`, `PanelSettings`, theme style sheets, building VisualElements in code, USS styling (flexbox, with no CSS grid), focus, and `SetEnabled`.
  - glTF and Blender: glTFast import, node naming, metres and Y-up, and re-exporting `.glb`.
  - Editor scripting: `MenuItem`, `EditorSceneManager`, `AssetDatabase`, `PrefabUtility`, `BuildPipeline`, and batch-mode `-executeMethod`.
  - Testing: the Unity Test Framework (NUnit, edit-mode and play-mode, `UnityTest` coroutines) and command-line `-runTests` with XML results.
  - Git for Unity: `.meta` files, `.gitignore`, Git LFS, Unity Smart Merge, and file ownership to avoid scene conflicts.
- **Follow-up:**
  - Programmer A sets up Unity 6000.6.8f1, Git LFS, and Smart Merge (see `UNITY_MIGRATION.md`), opens `unity/`, and builds the dye minigame against `TradeState.Shared.CompleteDyeTask()`.
  - Play-test by hand in the editor and tune the look sensitivity.
  - Confirm the presentation device's operating system.
  - Remove the Babylon.js code and Docker setup in a later commit, once both programmers work in Unity.
  - Confirm the cloth batch size, routes, and stowage from `CARGO_RESEARCH.md`.
  - Disk space on this machine is low, at about 1.7 GB free.

### Add the Sprint 2 shipping order and cargo loop

- **Status:** Ready for this commit
- **Recorded at:** 2026-11-11T13:31:35+02:00
- **Work by:** alyaafifi (Programmer B, Sprint 2 shipping order and cargo track) with Claude Code (Claude Opus 5.5)
- **Requested or approved by:** alyaafifi requested the Programmer B part of `SPRINT_2.md` and this handoff entry, and authorized the commit and push.
- **Git author:** alyaafifi
- **Git committer:** alyaafifi
- **Git author date:** 2026-11-11T13:31:35+02:00
- **Purpose:** Deliver the Programmer B track of Sprint 2. This covers sourced routes and stowage for purple cloth and wine, a sample order with two goods and two destinations, and a cargo screen. On that screen the player sets the quantity and destination for each good and assigns its place in the hold, and the dockworker points to the letter line to fix.
- **Changes:**
  - `CARGO_RESEARCH.md` is the product table for review with Programmer A. For each good it gives the proposed route and destination, the stowage position, sources and dates, and whether the evidence establishes the fact, provides a regional comparison, or is reconstructed. It also explains the hold layout and records limits and open questions.
    - Purple cloth goes to Kition (Cyprus) and is stowed in the dry locker at the stern. Sources: RINAP 1 Tiglath-pileser III 32 (Byblos tribute with multicoloured garments and purple wool), Livius on Kition (Tyrian resettlement in the second half of the 9th c. BCE), and Mazarrón 2 (esparto pannier and filler).
    - Wine goes to Egypt (Nile Delta) and is stowed in the jar bay amidships. Sources: the Ashkelon wrecks, *AJA* 2002 (wine "destined for either Egypt or Carthage"), with WHOI and OXREP, plus Mazarrón 2 (filler and esparto rope).
  - `src/trade/catalog.ts` defines the goods, destinations with route text, and the two hold positions.
  - `src/trade/shipment.ts` holds the sample order, with quantity, destination, and stowage on each line. `checkShipment()` returns the first problem on each line, in letter order: availability, then quantity, destination, and stowage. It marks whether a value is missing or wrong.
  - `src/trade/tradeState.ts` is the shared trade state and the dye-to-cargo contract. `completeDyeTask()` unlocks a prepared batch of 4 cloth bundles exactly once. An unfinished attempt leaves the cloth unavailable, and repeating a completed attempt doesn't duplicate cargo. Six wine jars are staged. `markShipmentSent()` records the shipment once.
  - `src/trade/cargoScreen.ts` is the 2D cargo dialog. It shows the letter, a card per good (stock, quantity stepper, destination choice, stowage status), and a ship diagram with stern, dry locker, jar bay and bow. You click a good, then a hold position, to place it. The dockworker message highlights the letter line to fix. A sent screen shows the routes and an evidence note. The dialog uses buttons, ARIA roles and labels, keyboard focus, and Esc to close.
  - `src/landing/stations.ts` finds station footprints by mesh name (`cargo`, `dye`, `scribe`) in both the Blender export and the placeholders, and finds the closest station within 1.5 m of the player's feet. `Landing` now exposes `stations`.
  - `src/main.ts` adds an E-key prompt and actions for stations. The cargo station opens the cargo screen. The dye stall gives a mock dye result in development builds or with `?mockDye`, until Programmer A's minigame replaces that entry. Station keys are ignored while the screen is open, and `tradeState` is exposed in development.
  - `src/player/walkController.ts` adds `setEnabled()` to pause walking and mouse look and release pointer lock while the screen is open. Pointer lock failures are now ignored.
  - `index.html`, `src/style.css`, and `src/ui/hud.ts` add the prompt element, the cargo screen styles, and E in the controls help.
  - `tests/trade.test.mjs` and `package.json` (`npm test`, using Node's built-in test runner on the type-stripped trade modules) add 12 tests for the order rules and the dye contract.
  - `README.md` documents the trade loop, station naming, the mock, and the tests.
- **Decisions:**
  - Stowage and destination are specified in the plain-language letter, so each wrong choice traces back to one line. The dockworker reports one problem at a time and names the line without giving the answer.
  - Proposed kickoff contract: one completed swatch unlocks a prepared batch. This avoids repeating the dye task for each bundle. Programmer A still needs to confirm it.
  - The destination is "Egypt (Nile Delta)" and not a named harbour, because no source here names the Delta port.
  - The trade logic stays free of DOM and Babylon.js so it can be tested without a browser or build step.
  - Clicking a good always selects it and never toggles it off. An earlier version auto-selected the flagged good, so the player's next click deselected it and the hold click did nothing.
- **Verification:**
  - `npm run typecheck` and `npm run build` passed. Vite still shows the existing bundle size warning. `npm test` passed 12 of 12 tests.
  - In the Claude desktop browser pane, against the dev server with Programmer A's `harbor-landing.glb`, I walked from the spawn to the cargo footprint and saw the "Press E to load the ship" prompt.
    - With no dye result, sending the shipment flagged line 1 with the dye stall message.
    - At the dye stall, the mock unlocked 4 bundles. Pressing E again did not add more.
    - Wrong quantity, missing or wrong destination, and missing or wrong stowage each flagged the correct line. The wine stepper stopped at 6.
    - The correct shipment showed the sent screen. Reopening the screen showed the sent state, and Esc closed it.
    - W did not move the player while the screen was open, and walking resumed after closing.
    - Fixed a horizontal overflow at about 680 px wide caused by panel padding.
  - Gaps: tested with local Node 22.19 and the running dev server, not the presentation container. Not tested with a screen reader or on a touch device. The full text of the Ashkelon article was not available, only its abstract.
- **Technical skills needed to continue this work:**
  - Historical research method: telling primary sources (Assyrian royal inscriptions in RINAP/ORACC, excavation reports, museum records) from secondary summaries; dating sources; labelling claims as established, regional comparison, or reconstruction; and citing them accessibly.
  - TypeScript with strict typing: union types, `Record` maps, readonly data, and erasable-only syntax so Node can strip the types for tests.
  - Building UI with the DOM API without a framework: creating elements, event handling, re-rendering while keeping focus, and dialog accessibility (ARIA roles, `aria-pressed` and `aria-checked`, focus management, Esc to close).
  - CSS layout: Grid and Flexbox, `box-sizing`, responsive breakpoints, and custom properties.
  - Babylon.js: reading mesh names and world bounding boxes from a glTF `AssetContainer`, render-loop observers, and pausing camera input with `attachControl` and `detachControl`.
  - Game state design: a small observable store, idempotent state changes, and an explicit contract between minigames.
  - Testing: `node:test` and `node:assert`, testing pure functions, and browser checks driven by scripted key events.
  - Git and the handoff workflow, plus coordinating interface contracts with another programmer.
- **Follow-up:**
  - Review `CARGO_RESEARCH.md` with Programmer A, and confirm both routes, both stowage positions, and the batch size of 4.
  - Programmer A replaces the `dye` entry in `stationActions` in `src/main.ts` with the minigame launcher and calls `tradeState.completeDyeTask()` on success.
  - Read the full Ashkelon article before modelling the 3D hold.
  - Later sprints: letter delivery timing, the Phoenician sign exercise, a packing grid, and touch controls.

### Document the Sprint 2 trade loop and harbor art brief

- **Status:** Ready for this commit
- **Recorded at:** 2026-11-11T12:13:30+01:00
- **Work by:** Codex
- **Requested or approved by:** Project user requested the sourced art brief, the Sprint 2 plan, and the commit and push.
- **Git author:** anthonymerhy
- **Git committer:** anthonymerhy
- **Git author date:** 2026-11-11T12:13:30+01:00
- **Purpose:** Let both programmers start the trade loop in parallel while the 3D designer extends the bay.
- **Changes:** `SPRINT_2.md` assigns the purple-dye minigame to Programmer A and route and stowage research and cargo interactions to Programmer B. It links to `HARBOR_ART_BRIEF.md`, which documents the Byblos evidence, landscape build order, recommended visual style, asset references, and Blender-to-browser handoff.
- **Decisions:** Use one sample order with two goods and two destination choices. Give each product a route entry and a designated place aboard with a simple click target. The art brief recommends a sandy working landing and grounded low-poly art, and identifies documented features, regional comparisons, and design choices.
- **Verification:** Checked both documents' local links, reviewed the sprint plan against the game vision and technical decisions, and checked the staged scope and whitespace. This commit contains documentation.
- **Follow-up:** Complete the joint Sprint 1 walk-through, agree on the dye-to-cargo handoff, and review the route and stowage evidence before finalizing the sample order.

### Add the Sprint 1 harbor blockout and browser asset

- **Status:** Ready for this commit
- **Recorded at:** 2026-11-11T11:38:14+01:00
- **Work by:** Codex
- **Requested or approved by:** Project user, acting as Programmer A, approved the starter scene and asked to record, commit, and push the combined preview.
- **Git author:** anthonymerhy
- **Git committer:** anthonymerhy
- **Git author date:** 2026-11-11T11:38:14+01:00
- **Purpose:** Give both programmers an editable Blender landing and make it load by default in the existing Babylon.js walk-through.
- **Changes:** `art/sprint-1/harbor-landing.blend` is the editable scene and `art/sprint-1/harbor-landing.glb` is its direct export. The blockout contains the shore and ramp, sandstone shoulder, water, boat, three station proxies, a 1.7 m scale reference, and a spawn reference. `public/assets/harbor-landing.glb` is the browser-ready copy served at the loader's default URL.
- **Decisions:** The Blender scene uses metres and opens in perspective with its waterline at Z = 0. The browser copy renames the spawn reference to `SPAWN` and rotates the export 90° around glTF Y so the camera faces the landing and sea along Babylon +Z. The browser starts at (0, 2.4, -17) in metres. The source and direct export retain their original orientation.
- **Verification:** Reopened the scene in Blender, reimported the direct GLB, and checked its scale and waterline in Babylon.js. The local browser loaded the browser-ready GLB and reported the expected `SPAWN` and 2.4 m ground height. `npm run build` passed with the existing chunk-size warning, and the production output contains a byte-for-byte copy of the browser asset.
- **Follow-up:** Both programmers still need to walk the route together and confirm ramp, collision, and station spacing. After Blender edits, regenerate the browser copy with the same orientation and spawn convention. The direct export alone uses different axes and a different spawn name.

### Add the Babylon.js landing walk-through and glTF import

- **Status:** Ready for this commit
- **Recorded at:** 2026-11-11T12:28:44+02:00
- **Work by:** alyaafifi (Programmer B, Sprint 1 Babylon.js track) with Claude Code (Claude Opus 5.5)
- **Requested or approved by:** alyaafifi requested the Programmer B part of `SPRINT_1.md` and this handoff entry, and authorized the commit and push.
- **Git author:** alyaafifi
- **Git committer:** alyaafifi
- **Git author date:** 2026-11-11T12:30:18+02:00
- **Purpose:** Deliver the Programmer B track of Sprint 1, a browser scene that both programmers can walk and that loads the Blender `.glb` in place of placeholder primitives.
- **Changes:**
  - `src/landing/config.ts` holds the shared landing settings to confirm at kickoff: the default asset path (`public/assets/harbor-landing.glb`), the spawn at Babylon `(0, 5, -36)` looking toward +Z (the sea), a 1.7 m body, 1.6 m eye height, walking at 2.2 m/s and running at 5 m/s, gravity, a 0.35 m step height, a 40° slope limit, a wading limit at Y = −0.5 m, and respawn below Y = −10 m. It also defines the mesh naming convention.
  - `src/landing/placeholderLanding.ts` builds the primitive stand-in for the landing. A height-profiled ground mesh forms the upper approach (Y = 5), a raised landward slope of about 12.5°, a flat dry cargo area (Y = 1), and a sandy ramp of about 8.5° into the water. The scene also has a translucent water plane at Y = 0, a boat proxy, a 1.7 m human reference, and footprints for the dye stall, scribe's table, and cargo station. The route between the footprints is wider than 3 m.
  - `src/landing/gltfLanding.ts` fetches a `.glb` with `cache: "no-store"` and checks the binary glTF header, because the Vite dev server answers missing files with `index.html`. It loads the asset into an `AssetContainer`, applies the naming convention, and reads an optional `SPAWN` node. Disposing the container removes the landing cleanly when you swap.
  - `src/landing/landing.ts` defines the shared `Landing` interface, the naming-convention roles, and the world-bounds calculation.
  - `src/player/walkController.ts` handles first-person walking. A `UniversalCamera` keeps only its mouse input, using pointer lock on click or dragging. An invisible body mesh moves with `moveWithCollisions`. Its ellipsoid starts at step height above the feet, so it collides only with walls and obstacles. A downward ray sets the feet height every frame, which handles stepping, snapping to slopes, blocking steep climbs and deep water, falling under gravity, and landing. Movement uses metres per second and is independent of frame rate.
  - `src/ui/hud.ts` is a debug overlay showing the loaded landing, its size and height range, the spawn source, the feet position in Babylon and Blender coordinates, the ground height, speed, FPS, and the controls.
  - `src/main.ts` sets up the scene. It is right-handed and Y-up, has sky and sun lights, and registers the glTF loaders on demand. On startup it loads the export and falls back to the placeholders if no export exists. It adds the keys R (respawn), L (reload export), and P (placeholders), drag-and-drop for `.glb` files, and the URL parameters `?asset=` and `?placeholder`. A 1.7 m scale marker sits beside the spawn. A `ResizeObserver` keeps the canvas size in sync. During development, the scene is available as `window.scene` for console inspection.
  - `index.html` and `src/style.css` make the canvas fill the viewport and add the HUD element.
  - `public/assets/.gitkeep` creates the folder for the export.
  - `README.md` documents the controls, how to swap assets, the axis mapping, and the naming convention.
- **Decisions:**
  - Use `scene.useRightHandedSystem = true` so glTF coordinates load 1:1 (the loader's `__root__` has an identity transform). As a result, Blender `(x, y, z)` maps to Babylon `(x, z, −y)`, and the waterline at Blender Z = 0 maps to Babylon Y = 0.
  - The walk uses a custom ray-plus-ellipsoid controller instead of `FreeCamera` gravity. Built-in camera gravity slides down slopes, uses per-frame speeds, and does not settle until the camera moves.
  - Colliders are named in Blender (`COL_`, `NOCOL_`, `water`, `SPAWN`), so Programmer A can control collision without code changes.
  - Programmer A should confirm the asset path, spawn, and approach direction. These are the agreed kickoff items, and the values in `config.ts` are proposals.
- **Verification:**
  - `npm run build` (typecheck and Vite build) passed. `npm audit` reported 0 vulnerabilities. Vite still shows the existing warning that the bundle is larger than 500 kB.
  - The walk-through was tested in the Claude desktop browser pane against the Vite dev server, by sending key events and reading the HUD and scene state.
  - On the placeholders, walking from the spawn down the slope, across the cargo area, and down the ramp followed the ground at 5 m/s. The player stopped at the wading limit (ground Y = −0.50). Strafing into the dye stall stopped at x = −5.20, which is the box edge plus the 0.3 m radius.
  - Dragging the mouse turned the camera in the expected direction.
  - A generated test `.glb` (ground at Y = 2, a `COL_wall` box, a `water_plane`, and a `SPAWN` node at (0, 2, −10)) loaded with an identity `__root__`. The spawn landed exactly at (0, 2, −10). The invisible wall blocked the player at z = −0.55, and the water did not collide.
  - P and L swapped landings without leaving stale meshes. A missing export fell back to the placeholders with a status message.
  - The test `.glb` was deleted and is not part of this commit.
  - Gaps: tested on Windows 11 with local Node.js 22.19 instead of the pinned Node 24 or Docker, because Docker is not installed on this machine. Pointer lock is blocked inside the embedded browser pane, so click-to-lock still needs a manual check in Chrome. A real Blender export has not been tested yet.
- **Technical skills needed to continue this work:**
  - TypeScript (strict mode, ES modules, classes, async/await) and the Vite dev server and `public/` asset serving.
  - Babylon.js 9: `Scene`, `Engine`, `UniversalCamera` input management, `AbstractMesh.moveWithCollisions` and ellipsoid colliders, `Scene.pickWithRay` with predicates, `AssetContainer` and `LoadAssetContainerAsync`, dynamic loader registration (`@babylonjs/loaders/dynamic`), `StandardMaterial`, and vertex data and colors.
  - 3D math: vectors, handedness (left-handed vs. right-handed), the axis conversion from Blender Z-up to glTF Y-up, surface normals and slope angles, and frame-rate-independent movement using delta time.
  - The glTF 2.0 / `.glb` format: the binary header, node and mesh naming, and how the Blender glTF exporter converts axes and units.
  - Browser APIs: Pointer Lock, keyboard events (`KeyboardEvent.code`), drag-and-drop `File` handling, `fetch` cache control, and `ResizeObserver`.
  - Blender collaboration: metric scale, empties for markers, and naming collider meshes for export.
  - Git, the handoff workflow, and Docker Compose for the shared environment.
- **Follow-up:**
  - Confirm the asset path, spawn, and approach direction with Programmer A, and ask them to add a `SPAWN` empty and any `COL_` barriers.
  - Swap in the first real export and run the midpoint walk-through. Adjust `walkSpeed`, `stepHeight`, and `maxWalkableSlopeDegrees` after that walk.
  - Check click-to-lock mouse look in desktop Chrome. Touch controls for the presentation device are not built yet.

### Plan parallel work for the harbor landing blockout

- **Status:** Ready for this commit
- **Recorded at:** 2026-11-11T11:11:32+01:00
- **Work by:** Codex
- **Requested or approved by:** Project user requested the two-programmer Sprint 1 plan, handoff update, commit, and push.
- **Git author:** anthonymerhy
- **Git committer:** anthonymerhy
- **Git author date:** 2026-11-11T11:11:32+01:00
- **Purpose:** Let both programmers start the harbor landing blockout in the same half-day.
- **Changes:** `SPRINT_1.md` defines parallel Blender and Babylon.js work, a shared scale and asset handoff, and browser acceptance checks. This commit contains planning documentation.
- **Decisions:** Use one short section of shore as the first integration target. The browser track starts with primitives while the Blender track prepares the first export. Both tracks meet at a midpoint walk-through so slope and spacing can be adjusted early.
- **Verification:** Reviewed the plan against the approved Blender-to-Babylon.js handoff and the landing blockout scope. Checked the staged diff for scope and whitespace. The browser checks are Sprint 1 deliverables.
- **Follow-up:** Assign the two tracks and agree on the asset path, spawn position, and approach direction at kickoff. Extend the bay after the landing passes the joint browser check.

### Set up the Babylon.js Docker environment

- **Status:** Ready for this commit
- **Recorded at:** 2026-11-11T10:55:12+01:00
- **Work by:** Codex
- **Requested or approved by:** Project user requested Docker and explicitly approved both the development and presentation containers.
- **Git author:** anthonymerhy
- **Git committer:** anthonymerhy
- **Git author date:** 2026-11-11T10:58:47+01:00
- **Purpose:** Give both programmers a reproducible environment for the two-half-day browser prototype and a way to verify the compiled presentation build.
- **Changes:** `package.json`, `package-lock.json`, `tsconfig.json`, `index.html`, and `src/` define a TypeScript and Vite starter that renders a placeholder box with Babylon.js. `.nvmrc` and the Docker build use Node.js 24.22.2. `Dockerfile` installs locked dependencies with `npm ci`, runs Vite for development, and serves the compiled output in a separate unprivileged Nginx image. `compose.yaml` exposes development on localhost port 5173 and presentation on localhost port 8080. Compose Watch syncs edits and rebuilds on package file changes. `.dockerignore` excludes Git data, local dependencies, Blender source and backups, and local environment files. `README.md` and `TECHNICAL_DECISIONS.md` document both run modes.
- **Decisions:** Keep the two modes in one Dockerfile with separate targets. Keep development dependencies in the build and development stages and copy only the static Vite output into the presentation stage. Pin npm package versions and preserve the lockfile so both programmers install the same dependencies.
- **Verification:** `npm run build` and the Docker build passed. `npm audit` reported zero known dependency vulnerabilities. Chrome rendered the placeholder box from the compiled build. Both containers served the project on their localhost ports, and a temporary source edit reached the development server through Compose Watch. Verified on the local Apple Silicon Mac. Before committing, reran the build, checked the Compose configuration, and verified both HTTP endpoints. Vite reports a bundle size warning, and the build succeeds.
- **Follow-up:** The harbor import, gameplay, and minigames are still to be built. The second programmer still needs to run the setup on their machine after it is available on GitHub.

### Document the pre-commit handoff workflow

- **Status:** Ready for this commit
- **Recorded at:** 2026-11-11T10:55:12+01:00
- **Work by:** Codex
- **Requested by:** Project user, to make handoffs readable by humans and other AI assistants and mandatory before each commit.
- **Git author:** anthonymerhy
- **Git committer:** anthonymerhy
- **Git author date:** 2026-11-11T10:58:46+01:00
- **Changes:** `HANDOFF.md` records the rules, entry template, and pending project work. `AGENTS.md` directs coding assistants to the handoff before work and reinforces the existing authorization requirements.
- **Decisions:** Use one Markdown log with an entry in the same commit as the work it describes. Record the AI contributor separately from Git authorship. Use the commit subject and author timestamp to find the entry in history.
- **Verification:** Checked the rules and template for attribution, time zone, technical detail, verification results, and how to handle pending work.
- **Follow-up:** Use this workflow for the Docker environment commit and subsequent work.
