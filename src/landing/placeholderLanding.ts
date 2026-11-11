import { StandardMaterial } from "@babylonjs/core/Materials/standardMaterial";
import { Color3, Color4 } from "@babylonjs/core/Maths/math.color";
import { Vector3 } from "@babylonjs/core/Maths/math.vector";
import type { Mesh } from "@babylonjs/core/Meshes/mesh";
import { CreateBox } from "@babylonjs/core/Meshes/Builders/boxBuilder";
import { CreateCylinder } from "@babylonjs/core/Meshes/Builders/cylinderBuilder";
import { CreateGround } from "@babylonjs/core/Meshes/Builders/groundBuilder";
import { VertexBuffer } from "@babylonjs/core/Buffers/buffer";
import { VertexData } from "@babylonjs/core/Meshes/mesh.vertexData";
import { TransformNode } from "@babylonjs/core/Meshes/transformNode";
import type { Scene } from "@babylonjs/core/scene";
import { LANDING_CONFIG } from "./config";
import { applyCollisionRole, computeBounds, type Landing } from "./landing";

/**
 * Ground profile along Babylon Z, as [z, height] points. Land rises toward -Z and
 * the sea lies toward +Z. These primitives stand in for the Blender export.
 */
const PROFILE: readonly (readonly [number, number])[] = [
  [-45, 5], // upper approach
  [-30, 5],
  [-12, 1], // raised landward slope, about 12.5 degrees
  [0, 1], // flat dry cargo area
  [12, -0.8], // sandy ramp into the water, about 8.5 degrees
  [30, -3], // seabed
];

const GROUND_WIDTH = 50;
const GROUND_MIN_Z = PROFILE[0][0];
const GROUND_MAX_Z = PROFILE[PROFILE.length - 1][0];

export function groundHeightAt(z: number): number {
  if (z <= PROFILE[0][0]) {
    return PROFILE[0][1];
  }
  for (let i = 1; i < PROFILE.length; i++) {
    const [z1, h1] = PROFILE[i];
    if (z <= z1) {
      const [z0, h0] = PROFILE[i - 1];
      return h0 + ((z - z0) / (z1 - z0)) * (h1 - h0);
    }
  }
  return PROFILE[PROFILE.length - 1][1];
}

function groundColorAt(height: number, z: number): Color4 {
  if (z > 0) {
    return new Color4(0.86, 0.76, 0.55, 1); // wet and dry sand on the ramp
  }
  if (height <= 1.01) {
    return new Color4(0.78, 0.7, 0.55, 1); // packed cargo area
  }
  return new Color4(0.55, 0.52, 0.38, 1); // landward slope and approach
}

function material(scene: Scene, name: string, color: Color3, alpha = 1): StandardMaterial {
  const mat = new StandardMaterial(name, scene);
  mat.diffuseColor = color;
  mat.specularColor = Color3.Black();
  mat.alpha = alpha;
  return mat;
}

function createGround(scene: Scene, parent: TransformNode): Mesh {
  const ground = CreateGround(
    "placeholder_ground",
    {
      width: GROUND_WIDTH,
      height: GROUND_MAX_Z - GROUND_MIN_Z,
      subdivisionsX: 10,
      subdivisionsY: (GROUND_MAX_Z - GROUND_MIN_Z) * 2,
      updatable: true,
    },
    scene,
  );
  ground.position.z = (GROUND_MIN_Z + GROUND_MAX_Z) / 2;

  const positions = ground.getVerticesData(VertexBuffer.PositionKind);
  const indices = ground.getIndices();
  if (!positions || !indices) {
    throw new Error("The placeholder ground has no geometry.");
  }
  const colors: number[] = [];
  for (let i = 0; i < positions.length; i += 3) {
    const worldZ = positions[i + 2] + ground.position.z;
    const height = groundHeightAt(worldZ);
    positions[i + 1] = height;
    const color = groundColorAt(height, worldZ);
    colors.push(color.r, color.g, color.b, color.a);
  }
  const normals: number[] = [];
  VertexData.ComputeNormals(positions, indices, normals);
  ground.updateVerticesData(VertexBuffer.PositionKind, positions);
  ground.updateVerticesData(VertexBuffer.NormalKind, normals);
  ground.setVerticesData(VertexBuffer.ColorKind, colors);
  ground.refreshBoundingInfo();

  ground.material = material(scene, "placeholder_ground_mat", Color3.White());
  ground.parent = parent;
  return ground;
}

/** A labelled box that marks the space a station occupies. */
function createFootprint(
  scene: Scene,
  parent: TransformNode,
  name: string,
  center: Vector3,
  size: { width: number; depth: number; height: number },
  color: Color3,
): Mesh {
  const box = CreateBox(name, size, scene);
  box.position = new Vector3(center.x, groundHeightAt(center.z) + size.height / 2, center.z);
  box.material = material(scene, `${name}_mat`, color, 0.75);
  box.parent = parent;
  return box;
}

/** A 1.7 m cylinder for checking scale at camera height. */
export function createScaleMarker(scene: Scene, name: string, base: Vector3): Mesh {
  const marker = CreateCylinder(name, { height: LANDING_CONFIG.bodyHeight, diameter: 0.4 }, scene);
  marker.position = base.add(new Vector3(0, LANDING_CONFIG.bodyHeight / 2, 0));
  marker.material = material(scene, `${name}_mat`, new Color3(0.9, 0.25, 0.2));
  return marker;
}

/**
 * Builds the primitive stand-in for the harbor landing: an upper approach, a raised
 * landward slope, a flat dry cargo area, a sandy ramp, water, a boat proxy, a human
 * reference, and footprints for the three stations.
 */
export function createPlaceholderLanding(scene: Scene): Landing {
  const root = new TransformNode("placeholder_landing", scene);

  createGround(scene, root);

  const water = CreateGround("water", { width: GROUND_WIDTH, height: GROUND_MAX_Z }, scene);
  water.position = new Vector3(0, 0, GROUND_MAX_Z / 2);
  water.material = material(scene, "water_mat", new Color3(0.15, 0.42, 0.55), 0.6);
  water.parent = root;

  // Boat proxy pulled up beside the ramp, hull along the shoreline direction.
  const boat = CreateBox("placeholder_boat", { width: 2.6, height: 1.2, depth: 9 }, scene);
  boat.position = new Vector3(7, 0.1, 9);
  boat.rotation.y = Math.PI / 2.4;
  boat.material = material(scene, "boat_mat", new Color3(0.45, 0.3, 0.18));
  boat.parent = root;

  // Station footprints on the dry cargo area. The route between them stays wider than 3 m.
  createFootprint(scene, root, "placeholder_dye_stall", new Vector3(-7, 0, -7), { width: 3, depth: 3, height: 1 }, new Color3(0.5, 0.15, 0.45));
  createFootprint(scene, root, "placeholder_scribe_table", new Vector3(6, 0, -6), { width: 2, depth: 1.2, height: 0.8 }, new Color3(0.55, 0.45, 0.3));
  createFootprint(scene, root, "placeholder_cargo_station", new Vector3(-6, 0, 2), { width: 3, depth: 2, height: 0.9 }, new Color3(0.35, 0.45, 0.6));

  const reference = createScaleMarker(scene, "placeholder_human_reference", new Vector3(2, groundHeightAt(-3), -3));
  reference.parent = root;

  const meshes = root.getChildMeshes();
  for (const mesh of meshes) {
    applyCollisionRole(mesh);
  }
  const bounds = computeBounds(meshes);

  return {
    label: "Placeholder primitives",
    boundsMin: bounds.min,
    boundsMax: bounds.max,
    dispose: () => root.dispose(false, true),
  };
}
