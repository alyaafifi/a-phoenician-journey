# A Phoenician Journey — Technical Decisions

## Approved prototype stack

| Area | Decision |
| --- | --- |
| Game and build | Babylon.js for the 3D harbor, TypeScript for game logic, and Vite for local development and the browser build. |
| Art handoff | The 3D team builds reusable pieces in Blender, assembles the quay there, and exports one `.glb` for the prototype. |
| First playable loop | Walk to the dye stall, dye a swatch, read a shipping order, and select the correct goods, quantities, and destination. |
| Cargo controls | Click to select goods and quantities. The packing grid is optional if the full loop is working and there is time left. |
| Time budget | Two half-days. Build the entire loop with placeholder assets in the first session. Import the quay, integrate the artwork, and test the browser build on the presentation device in the second. |

The prototype is ready when a new player can find the dyer, use the letter to work out the order, and complete the shipment without help from the team. See [the game vision](GAME_VISION.md) for the workshop concepts and historical references.

## Proposed developer setup

Skip Docker for the prototype. Both programmers can use the same Node.js version and install from a shared npm lockfile. Once the app is initialized, record the Node.js version in `.nvmrc`, keep `package-lock.json` in Git, and use `npm ci` after cloning. This gives both machines the same dependency versions with minimal setup. The exact Node.js version is open.

Use a private GitHub repository named `a-phoenician-journey` for the team. Confirm the owner account or organization and repository visibility before creating the remote.
