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
