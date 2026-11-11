import type { ProductId } from "./catalog";
import type { Availability } from "./shipment";

/**
 * Proposed kickoff contract: one completed dye swatch unlocks a prepared batch of
 * cloth on the landing. Confirm the batch size with Programmer A.
 */
export const PREPARED_CLOTH_BUNDLES = 4;
/** Wine jars already staged on the landing. */
export const STAGED_WINE_JARS = 6;

type Listener = () => void;

/**
 * Shared harbor state for the trade loop. The dye minigame reports its result here
 * and the cargo screen reads it.
 *
 * Contract for the dye minigame (Programmer A):
 * - Call `tradeState.completeDyeTask()` once, when the player finishes the swatch.
 * - Do not call it when the player leaves an attempt unfinished. The cloth then stays
 *   unavailable.
 * - Calling it again after a completed attempt does nothing, so cargo is never duplicated.
 */
export class TradeState {
  private dyeComplete = false;
  private shipped = false;
  private readonly listeners = new Set<Listener>();

  get dyeTaskComplete(): boolean {
    return this.dyeComplete;
  }

  get shipmentSent(): boolean {
    return this.shipped;
  }

  /** Goods ready to load, by product. */
  get available(): Availability {
    return {
      "purple-cloth": this.dyeComplete ? PREPARED_CLOTH_BUNDLES : 0,
      wine: STAGED_WINE_JARS,
    };
  }

  availableOf(product: ProductId): number {
    return this.available[product];
  }

  /** Records a finished dye task. Returns false if it was already recorded. */
  completeDyeTask(): boolean {
    if (this.dyeComplete) {
      return false;
    }
    this.dyeComplete = true;
    this.notify();
    return true;
  }

  markShipmentSent(): void {
    if (!this.shipped) {
      this.shipped = true;
      this.notify();
    }
  }

  subscribe(listener: Listener): () => void {
    this.listeners.add(listener);
    return () => this.listeners.delete(listener);
  }

  private notify(): void {
    for (const listener of this.listeners) {
      listener();
    }
  }
}

export const tradeState = new TradeState();
