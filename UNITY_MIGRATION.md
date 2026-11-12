# Unity migration

**Decision** · The team is moving the prototype from Babylon.js to Unity (11 November 2026, requested by Programmer B, alyaafifi). Unity replaces Babylon.js, Vite, npm, and Docker. Blender stays the art tool and the research documents stay as they are.

**Who this is for** · Both programmers, Programmer A in particular. Read this before you pull and open the project.

## Status

| Step | State |
| --- | --- |
| Unity version | **Unity 6.6, 6000.6.8f1**, pinned in `unity/ProjectSettings/ProjectVersion.txt` |
| Unity project under `unity/`: scripts, scene builder, build script, tests | Done. Compiles with no errors or warnings. |
| Packages | glTFast 6.21.0, Input System 1.20.1, Test Framework 1.8.0. Unity's default extras (Analytics, In-App Purchasing, Version Control, Multiplayer, XR, 2D, Navigation, Timeline) were removed. |
| Harbor scene | Generated from the Sprint 1 export: 7 solid meshes, water without collision, 3 stations, and the player at `SPAWN`. |
| Tests | 23/23 edit-mode and 3/3 play-mode pass, including the whole dye → cargo → ship loop. |
| Windows build | Built and launched without errors (about 97 MB). |
| Web build | Not built. Web Build Support needs about 1 GB to download and 5.6 GB on disk. |
| Removing the Babylon.js code (`src/`, `index.html`, `package.json`, Vite, Docker, `public/`) | Next step, once Programmer A has opened the Unity project. |

Until the Babylon.js code is removed, it still runs, but don't add features to it.

## Set up your machine (once)

1. **Unity Hub** · Install it from [unity.com/download](https://unity.com/download) and sign in. A free Personal license is fine for a student project.
2. **Unity editor** · In Unity Hub, install **Unity 6.6 (6000.6.8f1)** exactly, the version in `unity/ProjectSettings/ProjectVersion.txt`. Everyone must use the same version, because a different version upgrades and rewrites project files. Skip the extra modules and the Visual Studio download unless you need them. On a Mac, the editor builds for macOS out of the box; add **Windows Build Support (Mono)** only if you need to make the Windows build. Add **Web Build Support** (5.6 GB on disk) only once we decide to make a browser build. The editor alone needs about 9 GB of free space.
3. **Code editor** · Use Visual Studio, VS Code with the Unity extension, or Rider. Set it in **Edit ▸ Preferences ▸ External Tools**.
4. **Git LFS** · Run `git lfs install` once. New `.glb`, `.fbx`, `.blend`, image, audio, and font files are stored in LFS (see `.gitattributes`). Files committed before the migration stay as they are.
5. **Unity Smart Merge** · Add this to your Git config so scene and prefab conflicts merge by object, not by line. Replace the path with your editor's `UnityYAMLMerge`:
   - Windows: `C:\Program Files\Unity\Hub\Editor\<version>\Editor\Data\Tools\UnityYAMLMerge.exe`
   - macOS: `/Applications/Unity/Hub/Editor/<version>/Unity.app/Contents/Tools/UnityYAMLMerge`

   ```
   git config merge.unityyamlmerge.name "Unity SmartMerge"
   git config merge.unityyamlmerge.driver "'<path to UnityYAMLMerge>' merge -h -p --force %O %B %A %A"
   git config merge.unityyamlmerge.recursive binary
   ```

6. **Open the project** · In Unity Hub, choose **Add ▸ Add project from disk** and select the `unity/` folder, not the repository root.

## Project layout

| Path | Contents |
| --- | --- |
| `unity/Assets/Art/harbor-landing.glb` | The game-ready Blender export (imported by glTFast). |
| `unity/Assets/Scenes/Harbor.unity` | The harbor scene, **generated** from the export. |
| `unity/Assets/Scripts/Trade/` | Goods, destinations, hold positions, the sample order and its rules, and `TradeState`. Plain C#, no scene needed. |
| `unity/Assets/Scripts/Landing/` | The Blender naming convention and the `Station` component. |
| `unity/Assets/Scripts/Player/FirstPersonWalker.cs` | First-person walking on a `CharacterController`. |
| `unity/Assets/Scripts/UI/CargoScreen.cs` and `unity/Assets/UI/` | The cargo screen and HUD in UI Toolkit, with styles in `Harbor.uss`. |
| `unity/Assets/Scripts/HarborGame.cs` | Runs the scene: HUD, station prompts, the E key, and the station actions. |
| `unity/Assets/Editor/` | The scene builder and the web build script. |
| `unity/Assets/Tests/` | Edit-mode tests (order rules, naming) and a play-mode test of the whole loop. |

## Blender to Unity

The naming convention is unchanged, so existing exports keep working:

| Name in Blender | Behavior in Unity |
| --- | --- |
| `COL_…` | Invisible collider |
| `NOCOL_…` or a name containing `water` | Visible, no collision |
| `SPAWN` (an empty) | Player feet position |
| Name containing `cargo`, `dye`, or `scribe` | That station's footprint |
| Any other mesh | Visible, with a mesh collider |

After a new export:

1. Export the game-ready `.glb` to `unity/Assets/Art/harbor-landing.glb`. Keep metres and the waterline at Blender Z = 0.
2. In Unity, run **Tools ▸ Phoenician Journey ▸ Rebuild Harbor Scene**. It rebuilds `Harbor.unity`: colliders, hidden `COL_` meshes, stations, lighting, the player at `SPAWN` facing the cargo station, and the UI.
3. Press Play and walk the route.

The 90° rotation in the current browser copy was only needed for the Babylon.js camera. Unity turns the player toward the cargo station automatically, so you can export directly from Blender's orientation. glTFast handles the Y-up conversion.

**Don't hand-edit `Harbor.unity`**, because the next rebuild overwrites it. Put hand-made content in prefabs, such as the dye stall dressing or the minigame, and we'll have the builder place them. Or agree with Programmer B on a separate scene that loads additively.

## The dye minigame (Programmer A)

The dye stall opens a UI Toolkit screen. Select a plain swatch, dip it into the prepared bath, and lift it to reveal the color. The player can then return to the harbor with the finished sample.

- `DyeScreen.cs` owns the interaction. `HarborGame.cs` pauses movement while it is open and resumes movement on exit.
- Returning with a finished swatch calls `TradeState.Shared.CompleteDyeTask()` and unlocks the prepared batch. The prototype keeps the existing four-bundle setting. Leaving early preserves the previous cargo state, and replaying cannot duplicate the batch.
- The Harbor play-mode test covers an early exit, two completed attempts, and shipment of the resulting cloth.

## Working together without conflicts

- **Programmer A** owns `Assets/Art/`, dye minigame scripts and UI, and dye stall prefabs.
- **Programmer B** owns `Assets/Scripts/Trade/`, `Assets/Scripts/UI/CargoScreen.cs`, the cargo station, and the scene builder.
- Shared files are `HarborGame.cs` and `TradeState.cs`. Agree before changing them, and keep those changes small.
- Commit `.meta` files with their assets. Never commit `Library/`, `Temp/`, `Logs/`, `UserSettings/`, or `Builds/` (see `unity/.gitignore`).
- Pull before opening Unity, and close Unity before switching branches.

## Run, test, and build

- **Play** · Open `Assets/Scenes/Harbor.unity` and press Play. Click to capture the mouse or drag to look. Use WASD or the arrow keys to walk, Shift to run, E to use a station, R to respawn, and Esc to free the mouse.
- **Tests** · Open **Window ▸ General ▸ Test Runner** and run both **EditMode** (order rules and naming) and **PlayMode** (a full trade loop in the generated scene).
- **Windows build** · Run **Tools ▸ Phoenician Journey ▸ Build Windows**. It writes `unity/Builds/Windows/APhoenicianJourney.exe`. Copy the whole `Windows` folder, but not the `…_BackUpThisFolder_ButDontShipItWithYourGame` folder, to the presentation laptop and run the `.exe`. From a terminal:

  ```
  <Unity editor path> -batchmode -nographics -quit -projectPath unity -executeMethod PhoenicianJourney.Editor.BuildScript.BuildWindows -logFile -
  ```

- **Web build (optional)** · Install Web Build Support first, then run **Tools ▸ Phoenician Journey ▸ Build Web**. It writes `unity/Builds/Web/`, which any static web server can host.
- **Presentation device** · We switched from a browser build to a Windows build because of disk space. If the presentation laptop isn't Windows, we need a macOS build (from a Mac) or the web build. Decide this early.

## What moved where

| Babylon.js prototype | Unity |
| --- | --- |
| `src/landing/config.ts` (speeds, step, slope, wading, respawn) | `FirstPersonWalker` fields in the Inspector |
| `src/landing/landing.ts`, `gltfLanding.ts` (naming, glb import) | `LandingNaming.cs`, `HarborSceneBuilder.cs` (glTFast import in the editor) |
| `src/landing/placeholderLanding.ts` | Not ported; the Blender landing exists now |
| `src/landing/stations.ts` | `Station.cs`, prompt logic in `HarborGame.cs` |
| `src/player/walkController.ts` | `FirstPersonWalker.cs` |
| `src/trade/catalog.ts`, `shipment.ts`, `tradeState.ts` | `TradeCatalog.cs`, `Shipment.cs`, `TradeState.cs` |
| `src/trade/cargoScreen.ts`, `src/style.css` | `CargoScreen.cs`, `Harbor.uss` |
| `src/ui/hud.ts`, `src/main.ts` | `HarborGame.cs` |
| `tests/trade.test.mjs` | `Assets/Tests/EditMode/TradeTests.cs` |
| Docker, Vite dev server | Unity editor Play mode |
| Docker presentation container | Windows build in `unity/Builds/Windows/` (web build optional) |
| `L` reload, `P` placeholders, drag-and-drop `.glb` | Rebuild Harbor Scene after an export |

## Technical choices

- **Built-in render pipeline** for the first pass. It is the simplest setup and works with glTFast's materials. Switching to URP later is possible if the art needs it.
- **Mono scripting backend** for the Windows build. It ships with the editor, so no extra module or C++ toolchain is needed.
- **Unity 6.6 instead of 6.3 LTS.** It's a supported release, not long-term support. We chose it because it was already downloaded. Upgrading later is a one-way change for the whole team.
- **Input System package** for keyboard and mouse.
- **UI Toolkit**, built in C# with one stylesheet, so UI changes show up as readable diffs.
- **A generated scene**, so the scene can be rebuilt from the export and two people never merge the same scene file.

## Open items

- Confirm the presentation device's operating system (Windows build, macOS build, or web build).
- Mouse look, walking feel, and the cargo screen layout still need a hands-on check in the editor. The automated tests drive the screen through code, not the mouse.
- Confirm the batch size, both routes, and both stowage positions (see `CARGO_RESEARCH.md`).
- Remove the Babylon.js code and update `README.md` and `TECHNICAL_DECISIONS.md` once Unity reaches parity.
