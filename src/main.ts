import { Ray } from "@babylonjs/core/Culling/ray";
import { Engine } from "@babylonjs/core/Engines/engine";
import { HemisphericLight } from "@babylonjs/core/Lights/hemisphericLight";
import { DirectionalLight } from "@babylonjs/core/Lights/directionalLight";
import { Color3, Color4 } from "@babylonjs/core/Maths/math.color";
import { Vector3 } from "@babylonjs/core/Maths/math.vector";
import type { Mesh } from "@babylonjs/core/Meshes/mesh";
import { Scene } from "@babylonjs/core/scene";
import { registerBuiltInLoaders } from "@babylonjs/loaders/dynamic";
import "@babylonjs/core/Materials/standardMaterial";
import { LANDING_CONFIG } from "./landing/config";
import { loadGltfLanding, MissingAssetError } from "./landing/gltfLanding";
import type { Landing } from "./landing/landing";
import { createPlaceholderLanding, createScaleMarker } from "./landing/placeholderLanding";
import { stationInReach, type StationId } from "./landing/stations";
import { WalkController } from "./player/walkController";
import { CargoScreen } from "./trade/cargoScreen";
import { tradeState } from "./trade/tradeState";
import { Hud } from "./ui/hud";
import "./style.css";

const canvas = document.querySelector<HTMLCanvasElement>("#scene");
const hudRoot = document.querySelector<HTMLElement>("#hud");
const promptRoot = document.querySelector<HTMLElement>("#prompt");
if (!canvas || !hudRoot || !promptRoot) {
  throw new Error("The scene canvas, HUD, or prompt is missing.");
}

// The glTF loader is fetched on demand the first time an export is loaded.
registerBuiltInLoaders();

const engine = new Engine(canvas, true);
const scene = new Scene(engine);
// Right-handed and Y-up like glTF, so exports load without a conversion node.
scene.useRightHandedSystem = true;
scene.collisionsEnabled = true;
scene.clearColor = new Color4(0.62, 0.78, 0.9, 1);

const sky = new HemisphericLight("sky_light", Vector3.Up(), scene);
sky.intensity = 0.75;
sky.groundColor = new Color3(0.45, 0.4, 0.35);
const sun = new DirectionalLight("sun_light", new Vector3(-0.4, -1, 0.5), scene);
sun.intensity = 0.8;

if (import.meta.env.DEV) {
  // Lets the browser console inspect the scene and trade state during development.
  Object.assign(window, { scene, tradeState });
}

const hud = new Hud(hudRoot);
const player = new WalkController(scene, canvas);
const cargoScreen = new CargoScreen(document.body, tradeState);

const params = new URLSearchParams(window.location.search);
const assetUrl = params.get("asset") ?? LANDING_CONFIG.defaultAssetUrl;

let landing: Landing | null = null;
let scaleMarker: Mesh | null = null;
let loading = false;

/** Places the 1.7 m marker on the ground a few metres in front of the spawn point. */
function placeScaleMarker(spawn: Vector3): void {
  scaleMarker?.dispose(false, true);
  scaleMarker = null;
  const base = spawn.add(LANDING_CONFIG.spawnLookDirection.normalizeToNew().scale(3)).add(new Vector3(1.5, 0, 0));
  const hit = scene.pickWithRay(new Ray(base.add(new Vector3(0, 50, 0)), Vector3.Down(), 100), (mesh) => mesh.checkCollisions);
  if (hit?.pickedPoint) {
    base.y = hit.pickedPoint.y;
  }
  scaleMarker = createScaleMarker(scene, "scale_reference", base);
}

function showLanding(next: Landing, status: string, isError = false): void {
  landing?.dispose();
  landing = next;
  const spawn = next.spawn ?? LANDING_CONFIG.spawn;
  player.setSpawn(spawn);
  player.respawn();
  placeScaleMarker(spawn);
  hud.showLanding(next);
  hud.setStatus(status, isError);
}

function usePlaceholder(status: string, isError = false): void {
  showLanding(createPlaceholderLanding(scene), status, isError);
}

/** Loads the Blender export, or keeps the placeholders when no export exists yet. */
async function loadExport(source: string | File): Promise<void> {
  if (loading) {
    return;
  }
  loading = true;
  const name = typeof source === "string" ? source : source.name;
  hud.setStatus(`Loading ${name}…`);
  try {
    const next = await loadGltfLanding(scene, source);
    showLanding(next, `Loaded ${name}.`);
  } catch (error) {
    const message = error instanceof Error ? error.message : String(error);
    if (error instanceof MissingAssetError) {
      if (!landing) {
        usePlaceholder(`${message} Showing placeholders.`);
      } else {
        hud.setStatus(message, true);
      }
    } else {
      console.error(error);
      if (!landing) {
        usePlaceholder(`Could not load ${name}: ${message} Showing placeholders.`, true);
      } else {
        hud.setStatus(`Could not load ${name}: ${message}`, true);
      }
    }
  } finally {
    loading = false;
  }
}

// Station actions. Programmer A replaces the dye entry with the dye minigame launcher.
// While the minigame is being built, the dye stall gives a mock result in development
// builds or with `?mockDye`, so the shipment can be tested end to end.
const allowMockDye = import.meta.env.DEV || params.has("mockDye");
const stationActions: Partial<Record<StationId, { prompt: () => string; run: () => void }>> = {
  cargo: {
    prompt: () => (tradeState.shipmentSent ? "Press E to see the shipment" : "Press E to load the ship"),
    run: () => {
      player.setEnabled(false);
      cargoScreen.open(() => {
        player.setEnabled(true);
        canvas.focus();
      });
    },
  },
  dye: {
    prompt: () =>
      tradeState.dyeTaskComplete
        ? "The purple cloth is ready on the landing"
        : allowMockDye
          ? "Press E to dye a swatch (mock result)"
          : "The dye task is coming in Sprint 2",
    run: () => {
      if (allowMockDye && tradeState.completeDyeTask()) {
        hud.setStatus("Mock dye result: the purple cloth is ready to ship.");
      }
    },
  },
};

let nearbyStation: StationId | null = null;

const updateStationPrompt = (): void => {
  const station =
    landing && !cargoScreen.isOpen
      ? stationInReach(landing.stations, player.state.feet, (id) => stationActions[id] !== undefined)
      : null;
  nearbyStation = station?.id ?? null;
  const text = nearbyStation ? (stationActions[nearbyStation]?.prompt() ?? "") : "";
  if (promptRoot.textContent !== text) {
    promptRoot.textContent = text;
    promptRoot.hidden = text === "";
  }
};

window.addEventListener("keydown", (event) => {
  if (event.repeat || cargoScreen.isOpen) {
    return;
  }
  switch (event.code) {
    case "KeyE":
      if (nearbyStation) {
        stationActions[nearbyStation]?.run();
        updateStationPrompt();
      }
      break;
    case "KeyR":
      player.respawn();
      break;
    case "KeyL":
      void loadExport(assetUrl);
      break;
    case "KeyP":
      usePlaceholder("Showing placeholders. Press L to load the export.");
      break;
  }
});

// Drag a fresh Blender export onto the page to swap it in without copying files.
window.addEventListener("dragover", (event) => event.preventDefault());
window.addEventListener("drop", (event) => {
  event.preventDefault();
  const file = event.dataTransfer?.files[0];
  if (!file) {
    return;
  }
  if (!file.name.toLowerCase().endsWith(".glb")) {
    hud.setStatus(`${file.name} is not a .glb file.`, true);
    return;
  }
  void loadExport(file);
});

if (params.has("placeholder")) {
  usePlaceholder("Showing placeholders. Press L to load the export.");
} else {
  void loadExport(assetUrl);
}

let hudTimer = 0;
scene.onAfterRenderObservable.add(() => {
  updateStationPrompt();
  hudTimer += engine.getDeltaTime();
  if (hudTimer >= 100) {
    hudTimer = 0;
    hud.showPlayer(player.state, engine.getFps());
  }
});

engine.runRenderLoop(() => scene.render());
// Follow the canvas size, which also covers layout changes that do not resize the window.
new ResizeObserver(() => engine.resize()).observe(canvas);
