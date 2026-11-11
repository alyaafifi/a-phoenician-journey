import { Vector3 } from "@babylonjs/core/Maths/math.vector";

/**
 * Shared Sprint 1 landing settings. Agree on these values at kickoff and change
 * them here only, so both programmers walk the same route.
 *
 * Coordinates are Babylon.js world metres. The scene uses a right-handed,
 * Y-up system so glTF coordinates load without a conversion node:
 *   Babylon (x, y, z) = Blender (x, z, -y)
 *   Blender (x, y, z) = Babylon (x, -z, y)
 * The waterline is Babylon Y = 0, which is Blender Z = 0.
 * The sea lies toward Babylon +Z (Blender -Y) and the land rises toward -Z (Blender +Y).
 */
export const LANDING_CONFIG = {
  /** Default Blender export. Put the file at `public/assets/harbor-landing.glb`. */
  defaultAssetUrl: `${import.meta.env.BASE_URL}assets/harbor-landing.glb`,

  /** Feet position on the upper approach. A `SPAWN` node in the export overrides it. */
  spawn: new Vector3(0, 5, -36),
  /** The player starts looking down the approach toward the sea. */
  spawnLookDirection: new Vector3(0, 0, 1),

  /** Height of the scale reference marker and the player body. */
  bodyHeight: 1.7,
  eyeHeight: 1.6,
  bodyRadius: 0.3,

  /** Walking and running speed in metres per second. */
  walkSpeed: 2.2,
  runSpeed: 5,

  gravity: 9.81,
  /** Ledges up to this height are stepped onto; anything taller blocks the player. */
  stepHeight: 0.35,
  /** Surfaces steeper than this cannot be climbed. */
  maxWalkableSlopeDegrees: 40,
  /** The player can wade down to this ground height below the waterline. */
  minGroundY: -0.5,
  /** Falling below this height returns the player to the spawn point. */
  respawnBelowY: -10,
} as const;

/**
 * Mesh naming convention for the Blender export.
 * - `COL_` prefix: invisible collider, such as a wall or barrier.
 * - `NOCOL_` prefix or a name containing `water`: visible, no collision.
 * - Node named `SPAWN`, such as a Blender empty: player feet position.
 * - Every other mesh is visible and collides.
 */
export const NAMING = {
  colliderPrefix: "col_",
  noCollisionPrefix: "nocol_",
  waterMarker: "water",
  spawnNode: "spawn",
} as const;
