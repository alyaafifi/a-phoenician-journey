import { LoadAssetContainerAsync } from "@babylonjs/core/Loading/sceneLoader";
import { Vector3 } from "@babylonjs/core/Maths/math.vector";
import type { Scene } from "@babylonjs/core/scene";
import { NAMING } from "./config";
import { applyCollisionRole, computeBounds, type Landing } from "./landing";
import { findStations } from "./stations";

const GLB_MAGIC = 0x46546c67; // "glTF" in little-endian

/** Thrown when there is no export at the requested path, so the caller can fall back. */
export class MissingAssetError extends Error {}

/**
 * Downloads a `.glb` without the browser cache, so a fresh export is picked up on reload.
 * The Vite dev server answers unknown paths with `index.html`, so check the binary header
 * instead of trusting the status code.
 */
async function fetchGlb(url: string): Promise<Uint8Array> {
  const response = await fetch(url, { cache: "no-store" });
  if (!response.ok) {
    throw new MissingAssetError(`No export at ${url} (HTTP ${response.status}).`);
  }
  const bytes = new Uint8Array(await response.arrayBuffer());
  if (bytes.byteLength < 12 || new DataView(bytes.buffer).getUint32(0, true) !== GLB_MAGIC) {
    throw new MissingAssetError(`No binary glTF at ${url}.`);
  }
  return bytes;
}

/**
 * Loads a Blender `.glb` export, applies the collision naming convention, and reads the
 * optional `SPAWN` node. `source` is a URL or a file dropped onto the page.
 */
export async function loadGltfLanding(scene: Scene, source: string | File): Promise<Landing> {
  const data = typeof source === "string" ? await fetchGlb(source) : new Uint8Array(await source.arrayBuffer());
  const label = typeof source === "string" ? source : source.name;

  const container = await LoadAssetContainerAsync(data, scene, { pluginExtension: ".glb", name: label });
  container.addAllToScene();

  for (const mesh of container.meshes) {
    applyCollisionRole(mesh);
  }

  let spawn: Vector3 | undefined;
  const isSpawn = (name: string): boolean => name.toLowerCase() === NAMING.spawnNode;
  const spawnMesh = container.meshes.find((mesh) => isSpawn(mesh.name));
  if (spawnMesh) {
    spawnMesh.isVisible = false;
    spawnMesh.checkCollisions = false;
  }
  const spawnNode = spawnMesh ?? container.transformNodes.find((node) => isSpawn(node.name));
  if (spawnNode) {
    spawnNode.computeWorldMatrix(true);
    spawn = spawnNode.getAbsolutePosition().clone();
  }

  const bounds = computeBounds(container.meshes);
  return {
    label,
    spawn,
    stations: findStations(container.meshes),
    boundsMin: bounds.min,
    boundsMax: bounds.max,
    dispose: () => {
      container.removeAllFromScene();
      container.dispose();
    },
  };
}
