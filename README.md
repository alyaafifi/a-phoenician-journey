# a-phoenician-journey
BIP Project - Byblos

## Run the development environment

Start Docker and run the following from the project directory.

```sh
docker compose up --build --watch
```

Open [the local game](http://localhost:5173). The starting scene renders a box so you can verify that Babylon.js works. Compose syncs source and asset edits and rebuilds when the dependency files change. Use `Ctrl+C` to stop, or `docker compose down` to remove the development container.

## Run the presentation build

Build and run the static presentation container, then open [the local presentation build](http://localhost:8080).

```sh
docker compose up --build presentation
```

The presentation container serves the compiled game. Rebuild it after changing the source or assets.

For development directly on your machine, use the Node.js version in `.nvmrc`, then run `npm ci` and `npm run dev`.

See [the game vision](GAME_VISION.md) and [the technical decisions](TECHNICAL_DECISIONS.md) for prototype scope and the Blender workflow.
