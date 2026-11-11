# A Phoenician Journey — Technical Decisions

## Approved prototype stack

| Area | Decision |
| --- | --- |
| Game and build | Unity 6.6 (6000.6.8f1) with C# game logic, the built-in render pipeline, UI Toolkit, and the Input System. Windows desktop build with the Mono backend. A browser build is optional. Changed from Babylon.js, TypeScript, and Vite on 11 November 2026; see [the Unity migration](UNITY_MIGRATION.md). |
| Art handoff | The 3D team builds reusable pieces in Blender, assembles the quay there, and exports one `.glb` for the prototype. Unity imports it with glTFast, and the scene builder generates the harbor scene from it. |
| First playable loop | Walk to the dye stall, dye a swatch, read a shipping order, and select the correct goods, quantities, and destination. |
| Cargo controls | Click to select goods and quantities. The packing grid is optional if the full loop is working and there is time left. |
| Time budget | Two half-days. Build the entire loop with placeholder assets in the first session. Import the quay, integrate the artwork, and test the build on the presentation device in the second. |

The prototype is ready when a new player can find the dyer, use the letter to work out the order, and complete the shipment without help from the team. See [the game vision](GAME_VISION.md) for the workshop concepts and historical references.

## Developer setup

Both programmers install the same Unity version through Unity Hub and open the `unity/` folder. Git LFS stores new binary assets, and Unity Smart Merge handles scene and prefab conflicts. Setup steps, file ownership, and the Blender-to-Unity workflow are in [the Unity migration](UNITY_MIGRATION.md).

The earlier Babylon.js prototype (`src/`, Vite, npm, and the Docker development and presentation containers) still runs. It will be removed once both programmers work in Unity.

The team repository is [alyaafifi/a-phoenician-journey](https://github.com/alyaafifi/a-phoenician-journey).
