import { UniversalCamera } from "@babylonjs/core/Cameras/universalCamera";
import { Ray } from "@babylonjs/core/Culling/ray";
import { Vector3 } from "@babylonjs/core/Maths/math.vector";
import type { AbstractMesh } from "@babylonjs/core/Meshes/abstractMesh";
import { Mesh } from "@babylonjs/core/Meshes/mesh";
import type { Scene } from "@babylonjs/core/scene";
import "@babylonjs/core/Collisions/collisionCoordinator";
import "@babylonjs/core/Culling/ray";
import { LANDING_CONFIG } from "../landing/config";

const MOVE_KEYS: Record<string, readonly [number, number]> = {
  KeyW: [0, 1],
  ArrowUp: [0, 1],
  KeyS: [0, -1],
  ArrowDown: [0, -1],
  KeyA: [-1, 0],
  ArrowLeft: [-1, 0],
  KeyD: [1, 0],
  ArrowRight: [1, 0],
};

export interface WalkState {
  /** Feet position in world metres. */
  readonly feet: Vector3;
  readonly grounded: boolean;
  /** Height of the ground under the player, if any is within reach. */
  readonly groundY: number | null;
  readonly speed: number;
}

/**
 * First-person walking with a mouse-look camera.
 *
 * The body collider is an ellipsoid that starts at step height above the feet, so it
 * only hits walls and obstacles. The feet follow the ground with a downward ray, which
 * keeps slopes and ramps from sliding the player and makes movement independent of
 * the frame rate.
 */
export class WalkController {
  readonly camera: UniversalCamera;
  private readonly body: Mesh;
  private readonly pressed = new Set<string>();
  private readonly minWalkableNormalY = Math.cos((LANDING_CONFIG.maxWalkableSlopeDegrees * Math.PI) / 180);
  private verticalVelocity = 0;
  private grounded = false;
  private groundY: number | null = null;
  private speed = 0;
  private spawnPoint = LANDING_CONFIG.spawn.clone();
  private enabled = true;

  constructor(
    private readonly scene: Scene,
    canvas: HTMLCanvasElement,
  ) {
    const { bodyHeight, bodyRadius, stepHeight } = LANDING_CONFIG;

    this.body = new Mesh("player_body", scene);
    this.body.isPickable = false;
    this.body.checkCollisions = false;
    const halfHeight = (bodyHeight - stepHeight) / 2;
    this.body.ellipsoid = new Vector3(bodyRadius, halfHeight, bodyRadius);
    this.body.ellipsoidOffset = new Vector3(0, stepHeight + halfHeight, 0);

    this.camera = new UniversalCamera("walk_camera", Vector3.Zero(), scene);
    this.camera.minZ = 0.05;
    this.camera.fov = 1.1;
    this.camera.inertia = 0;
    this.camera.angularSensibility = 1500;
    // Movement comes from this controller; the camera only handles mouse look.
    this.camera.inputs.clear();
    this.camera.inputs.addMouse();
    this.camera.attachControl(canvas, true);

    canvas.addEventListener("click", () => {
      if (this.enabled && document.pointerLockElement !== canvas) {
        // Some embedded browsers refuse pointer lock; dragging still turns the camera.
        void canvas.requestPointerLock()?.catch?.(() => undefined);
      }
    });
    window.addEventListener("keydown", (event) => {
      if (this.enabled && (event.code in MOVE_KEYS || event.code.startsWith("Shift"))) {
        this.pressed.add(event.code);
        event.preventDefault();
      }
    });
    window.addEventListener("keyup", (event) => this.pressed.delete(event.code));
    window.addEventListener("blur", () => this.pressed.clear());

    scene.onBeforeRenderObservable.add(() => this.update(scene.getEngine().getDeltaTime() / 1000));
    this.respawn();
  }

  get state(): WalkState {
    return { feet: this.body.position, grounded: this.grounded, groundY: this.groundY, speed: this.speed };
  }

  /**
   * Pauses walking and mouse look, for example while a 2D screen is open. Gravity keeps
   * running so the player stays on the ground.
   */
  setEnabled(enabled: boolean): void {
    if (enabled === this.enabled) {
      return;
    }
    this.enabled = enabled;
    this.pressed.clear();
    const canvas = this.scene.getEngine().getRenderingCanvas();
    if (enabled && canvas) {
      this.camera.attachControl(canvas, true);
    } else {
      this.camera.detachControl();
      if (document.pointerLockElement) {
        document.exitPointerLock();
      }
    }
  }

  /** Sets where `respawn()` places the player, such as the export's `SPAWN` node. */
  setSpawn(feet: Vector3): void {
    this.spawnPoint = feet.clone();
  }

  respawn(): void {
    this.body.position.copyFrom(this.spawnPoint);
    this.verticalVelocity = 0;
    this.grounded = false;
    this.syncCamera();
    this.camera.setTarget(this.camera.position.add(LANDING_CONFIG.spawnLookDirection));
  }

  private isWalkSurface = (mesh: AbstractMesh): boolean =>
    mesh !== this.body && mesh.checkCollisions && mesh.isEnabled();

  /** Casts down from step height above `feet` and returns the first walkable hit. */
  private probeGround(feet: Vector3, reach: number): { y: number; walkable: boolean } | null {
    const { stepHeight } = LANDING_CONFIG;
    const origin = new Vector3(feet.x, feet.y + stepHeight, feet.z);
    const hit = this.scene.pickWithRay(new Ray(origin, Vector3.Down(), stepHeight + reach), this.isWalkSurface);
    if (!hit?.hit || !hit.pickedPoint) {
      return null;
    }
    const normal = hit.getNormal(true);
    const walkable = !normal || Math.abs(normal.y) >= this.minWalkableNormalY;
    return { y: hit.pickedPoint.y, walkable };
  }

  private movementInput(): { x: number; z: number; run: boolean } {
    let x = 0;
    let z = 0;
    for (const code of this.pressed) {
      const axis = MOVE_KEYS[code];
      if (axis) {
        x += axis[0];
        z += axis[1];
      }
    }
    const length = Math.hypot(x, z);
    const run = this.pressed.has("ShiftLeft") || this.pressed.has("ShiftRight");
    return length > 0 ? { x: x / length, z: z / length, run } : { x: 0, z: 0, run };
  }

  private update(rawDt: number): void {
    const dt = Math.min(rawDt, 0.1);
    if (dt <= 0) {
      return;
    }
    const cfg = LANDING_CONFIG;
    const feet = this.body.position;
    const start = feet.clone();

    // Horizontal movement relative to where the camera looks.
    const input = this.movementInput();
    if (input.x !== 0 || input.z !== 0) {
      const forward = this.camera.getDirection(this.scene.useRightHandedSystem ? new Vector3(0, 0, -1) : Vector3.Forward());
      forward.y = 0;
      forward.normalize();
      const right = this.camera.getDirection(Vector3.Right());
      right.y = 0;
      right.normalize();
      const speed = input.run ? cfg.runSpeed : cfg.walkSpeed;
      const move = forward.scale(input.z * speed * dt).addInPlace(right.scale(input.x * speed * dt));
      this.body.moveWithCollisions(move);
      // moveWithCollisions only handles walls; the ground probe below sets the height.
      feet.y = start.y;

      // Block steep climbs, deep water, and walking off into nothing while grounded.
      const ahead = this.probeGround(feet, cfg.stepHeight);
      const tooSteep = ahead !== null && !ahead.walkable && ahead.y > start.y;
      const tooDeep = ahead !== null && ahead.y < cfg.minGroundY;
      if (tooSteep || tooDeep) {
        feet.x = start.x;
        feet.z = start.z;
      }
    }

    // Vertical movement: follow the ground when it is within step reach, otherwise fall.
    const ground = this.probeGround(feet, this.grounded ? cfg.stepHeight : 0);
    if (ground && this.verticalVelocity <= 0) {
      feet.y = ground.y;
      this.verticalVelocity = 0;
      this.grounded = true;
    } else {
      this.verticalVelocity -= cfg.gravity * dt;
      const fall = -this.verticalVelocity * dt;
      const landing = this.probeGround(feet, fall);
      if (landing) {
        feet.y = landing.y;
        this.verticalVelocity = 0;
        this.grounded = true;
      } else {
        feet.y -= fall;
        this.grounded = false;
      }
    }

    const below = this.probeGround(feet, 50);
    this.groundY = below ? below.y : null;
    this.speed = Math.hypot(feet.x - start.x, feet.z - start.z) / dt;

    if (feet.y < cfg.respawnBelowY) {
      this.respawn();
      return;
    }
    this.syncCamera();
  }

  private syncCamera(): void {
    const feet = this.body.position;
    this.camera.position.set(feet.x, feet.y + LANDING_CONFIG.eyeHeight, feet.z);
  }
}
