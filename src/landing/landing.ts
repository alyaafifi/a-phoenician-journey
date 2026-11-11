import type { AbstractMesh } from "@babylonjs/core/Meshes/abstractMesh";
import { Vector3 } from "@babylonjs/core/Maths/math.vector";
import { NAMING } from "./config";

/** A loaded harbor landing: either the placeholder primitives or a Blender export. */
export interface Landing {
  /** Shown in the HUD, such as "Placeholder" or the file name. */
  readonly label: string;
  /** Feet spawn position from the export, if it defines one. */
  readonly spawn?: Vector3;
  /** World-space bounds of the visible geometry. */
  readonly boundsMin: Vector3;
  readonly boundsMax: Vector3;
  dispose(): void;
}

export type MeshRole = "collider" | "visual" | "solid";

/** Classifies a mesh by the naming convention in `NAMING`. */
export function meshRole(name: string): MeshRole {
  const lower = name.toLowerCase();
  if (lower.startsWith(NAMING.colliderPrefix)) {
    return "collider";
  }
  if (lower.startsWith(NAMING.noCollisionPrefix) || lower.includes(NAMING.waterMarker)) {
    return "visual";
  }
  return "solid";
}

/** Applies the collision convention to a mesh and returns its role. */
export function applyCollisionRole(mesh: AbstractMesh): MeshRole {
  const role = meshRole(mesh.name);
  mesh.checkCollisions = role !== "visual" && mesh.getTotalVertices() > 0;
  if (role === "collider") {
    mesh.isVisible = false;
  }
  return role;
}

/** Computes the world bounds of the visible meshes that have geometry. */
export function computeBounds(meshes: readonly AbstractMesh[]): { min: Vector3; max: Vector3 } {
  const min = new Vector3(Number.POSITIVE_INFINITY, Number.POSITIVE_INFINITY, Number.POSITIVE_INFINITY);
  const max = new Vector3(Number.NEGATIVE_INFINITY, Number.NEGATIVE_INFINITY, Number.NEGATIVE_INFINITY);
  let found = false;
  for (const mesh of meshes) {
    if (mesh.getTotalVertices() === 0 || !mesh.isVisible) {
      continue;
    }
    mesh.computeWorldMatrix(true);
    const box = mesh.getBoundingInfo().boundingBox;
    min.minimizeInPlace(box.minimumWorld);
    max.maximizeInPlace(box.maximumWorld);
    found = true;
  }
  return found ? { min, max } : { min: Vector3.Zero(), max: Vector3.Zero() };
}
