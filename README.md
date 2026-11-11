# a-phoenician-journey
BIP Project - Byblos

## Unity (current)

The game is moving to Unity. Install **Unity 6.6 (6000.6.8f1)** with Unity Hub and run `git lfs install` once. Then add the `unity/` folder in Unity Hub, open `Assets/Scenes/Harbor.unity`, and press Play.

- **Controls** · Click or drag to look, WASD or the arrow keys to walk, Shift to run, E to use a station, R to respawn, and Esc to free the mouse.
- **Tests** · Open **Window ▸ General ▸ Test Runner** and run the EditMode and PlayMode tests.
- **New Blender export** · Save it as `unity/Assets/Art/harbor-landing.glb`, then run **Tools ▸ Phoenician Journey ▸ Rebuild Harbor Scene**.
- **Windows build** · Run **Tools ▸ Phoenician Journey ▸ Build Windows**. It writes `unity/Builds/Windows/APhoenicianJourney.exe`.

Setup, workflow, and file ownership are in [the Unity migration](UNITY_MIGRATION.md).

## Babylon.js prototype (being retired)

The sections below describe the earlier browser prototype. It still runs, but new work goes into Unity.

## Run the development environment

Start Docker and run the following from the project directory.

```sh
docker compose up --build --watch
```

Open [the local game](http://localhost:5173). The scene loads the harbor landing for a first-person walk-through. Compose syncs source and asset edits and rebuilds when the dependency files change. Use `Ctrl+C` to stop, or `docker compose down` to remove the development container.

## Walk the harbor landing

Click the scene to capture the mouse, or drag to look around. Use WASD or the arrow keys to walk and hold Shift to run. Press R to respawn, L to reload the Blender export, and P to show the placeholder primitives. Esc releases the mouse. The debug panel shows the feet position in Babylon.js and Blender coordinates, the ground height, and the size of the loaded landing.

Place the Blender export at `public/assets/harbor-landing.glb`. If the file is missing, the scene shows placeholder primitives. To test another export, open `/?asset=/assets/<file>.glb` or drop a `.glb` file onto the page. Open `/?placeholder` to start with the primitives.

Use one Blender unit per metre and put the waterline at Blender `Z = 0`. The scene uses glTF's right-handed, Y-up axes, so Blender `(x, y, z)` appears at Babylon.js `(x, z, -y)`. Name meshes in Blender as follows:

| Name | Behavior |
| --- | --- |
| `COL_…` | Invisible collider, such as a wall or barrier |
| `NOCOL_…` or a name containing `water` | Visible, no collision |
| `SPAWN` (an empty) | Player feet position at load and respawn |
| Any other mesh | Visible and collides |
| Name containing `cargo`, `dye`, or `scribe` | Marks that station's footprint, in addition to the rules above |

Spawn, speeds, slope limit, step height, and wading depth are in `src/landing/config.ts`.

## Play the trade loop

Walk to a station and press E when the prompt appears.

1. **Dye stall** · The minigame is in progress. Development builds give a mock result here that makes the purple cloth ready. For the presentation build, add `?mockDye` to the URL to get the same mock.
2. **Cargo station** · Read the shipping letter, set the quantity and destination for each good, select a good and click its place in the ship's hold, then send the ship. The dockworker points to the letter line to fix. Esc or **Back to the harbor** closes the screen.

The order, goods, routes, and hold positions are in `src/trade/`. The sources and reconstruction choices behind them are in [the cargo research](CARGO_RESEARCH.md). The dye minigame reports its result with `tradeState.completeDyeTask()` from `src/trade/tradeState.ts`.

Run the unit tests with `npm test`.

## Run the presentation build

Build and run the static presentation container, then open [the local presentation build](http://localhost:8080).

```sh
docker compose up --build presentation
```

The presentation container serves the compiled game. Rebuild it after changing the source or assets.

For development directly on your machine, use the Node.js version in `.nvmrc`, then run `npm ci` and `npm run dev`.

See [the game vision](GAME_VISION.md) and [the technical decisions](TECHNICAL_DECISIONS.md) for prototype scope and the Blender workflow.
