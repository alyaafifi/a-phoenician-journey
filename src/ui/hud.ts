import type { Vector3 } from "@babylonjs/core/Maths/math.vector";
import type { Landing } from "../landing/landing";
import type { WalkState } from "../player/walkController";

const format = (value: number): string => value.toFixed(2);

/** Babylon world coordinates printed as Blender coordinates, for comparing with the scene file. */
const toBlender = (v: Vector3): string => `${format(v.x)}, ${format(-v.z)}, ${format(v.y)}`;
const toBabylon = (v: Vector3): string => `${format(v.x)}, ${format(v.y)}, ${format(v.z)}`;

/** Debug overlay with the loaded landing, player position, and controls. */
export class Hud {
  private readonly status: HTMLElement;
  private readonly landing: HTMLElement;
  private readonly player: HTMLElement;

  constructor(root: HTMLElement) {
    root.innerHTML = `
      <h1>A Phoenician Journey · landing blockout</h1>
      <p data-hud="status" role="status"></p>
      <dl data-hud="landing"></dl>
      <dl data-hud="player"></dl>
      <p class="hud-help">
        Click or drag to look around · WASD or arrows to walk · Shift to run · R respawn ·
        L reload export · P placeholders · drop a .glb to swap · Esc releases mouse
      </p>`;
    this.status = root.querySelector<HTMLElement>('[data-hud="status"]')!;
    this.landing = root.querySelector<HTMLElement>('[data-hud="landing"]')!;
    this.player = root.querySelector<HTMLElement>('[data-hud="player"]')!;
  }

  setStatus(message: string, isError = false): void {
    this.status.textContent = message;
    this.status.classList.toggle("hud-error", isError);
  }

  showLanding(landing: Landing): void {
    const size = landing.boundsMax.subtract(landing.boundsMin);
    this.landing.innerHTML = `
      <dt>Landing</dt><dd></dd>
      <dt>Size (m)</dt><dd>${format(size.x)} × ${format(size.y)} × ${format(size.z)}</dd>
      <dt>Height range</dt><dd>${format(landing.boundsMin.y)} to ${format(landing.boundsMax.y)} m</dd>
      <dt>Spawn</dt><dd>${landing.spawn ? "SPAWN node" : "config default"}</dd>`;
    // Set the label as text so a dropped file name cannot inject markup.
    this.landing.querySelector("dd")!.textContent = landing.label;
  }

  showPlayer(state: WalkState, fps: number): void {
    const ground = state.groundY === null ? "none" : `${format(state.groundY)} m`;
    this.player.innerHTML = `
      <dt>Feet, Babylon</dt><dd>${toBabylon(state.feet)}</dd>
      <dt>Feet, Blender</dt><dd>${toBlender(state.feet)}</dd>
      <dt>Ground</dt><dd>${ground}${state.grounded ? "" : " · airborne"}</dd>
      <dt>Speed</dt><dd>${format(state.speed)} m/s · ${fps.toFixed(0)} fps</dd>`;
  }
}
