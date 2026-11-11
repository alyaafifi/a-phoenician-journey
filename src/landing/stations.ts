import type { AbstractMesh } from "@babylonjs/core/Meshes/abstractMesh";
import type { Vector3 } from "@babylonjs/core/Maths/math.vector";

export type StationId = "cargo" | "dye" | "scribe";

/** A station footprint on the ground plan, as a world-space rectangle in X and Z. */
export interface Station {
  readonly id: StationId;
  readonly minX: number;
  readonly maxX: number;
  readonly minZ: number;
  readonly maxZ: number;
}

/**
 * Station meshes are found by name, case-insensitively, in both the Blender export and
 * the placeholders: a name containing `cargo`, `dye`, or `scribe` marks that footprint.
 * Several meshes with the same keyword are merged into one footprint.
 */
const STATION_KEYWORDS: readonly (readonly [StationId, RegExp])[] = [
  ["cargo", /cargo/i],
  ["dye", /dye/i],
  ["scribe", /scribe/i],
];

/** How close, in metres, the player must stand to a footprint's edge to use it. */
export const STATION_REACH = 1.5;

export function findStations(meshes: readonly AbstractMesh[]): Station[] {
  const found = new Map<StationId, { minX: number; maxX: number; minZ: number; maxZ: number }>();
  for (const mesh of meshes) {
    const match = STATION_KEYWORDS.find(([, pattern]) => pattern.test(mesh.name));
    if (!match || mesh.getTotalVertices() === 0) {
      continue;
    }
    mesh.computeWorldMatrix(true);
    const box = mesh.getBoundingInfo().boundingBox;
    const current = found.get(match[0]);
    found.set(match[0], {
      minX: Math.min(current?.minX ?? Infinity, box.minimumWorld.x),
      maxX: Math.max(current?.maxX ?? -Infinity, box.maximumWorld.x),
      minZ: Math.min(current?.minZ ?? Infinity, box.minimumWorld.z),
      maxZ: Math.max(current?.maxZ ?? -Infinity, box.maximumWorld.z),
    });
  }
  return [...found].map(([id, bounds]) => ({ id, ...bounds }));
}

/** Distance in X and Z from a point to the footprint rectangle; zero when inside it. */
export function distanceToStation(station: Station, point: Vector3): number {
  const dx = Math.max(station.minX - point.x, 0, point.x - station.maxX);
  const dz = Math.max(station.minZ - point.z, 0, point.z - station.maxZ);
  return Math.hypot(dx, dz);
}

/** The closest station within reach of the player's feet, among those that have an action. */
export function stationInReach(
  stations: readonly Station[],
  feet: Vector3,
  hasAction: (id: StationId) => boolean,
): Station | null {
  let best: Station | null = null;
  let bestDistance = STATION_REACH;
  for (const station of stations) {
    if (!hasAction(station.id)) {
      continue;
    }
    const distance = distanceToStation(station, feet);
    if (distance <= bestDistance) {
      best = station;
      bestDistance = distance;
    }
  }
  return best;
}
