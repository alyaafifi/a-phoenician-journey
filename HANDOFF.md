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
