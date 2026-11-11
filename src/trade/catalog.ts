/**
 * Goods, destinations, and hold positions for the Sprint 2 shipment.
 * Sources and reconstruction notes for each entry are in CARGO_RESEARCH.md.
 *
 * This module and the other pure trade modules use only erasable TypeScript, so
 * Node can run their tests without a build step.
 */

export type ProductId = "purple-cloth" | "wine";
export type DestinationId = "kition" | "egypt";
export type HoldPositionId = "dry-locker" | "jar-bay";

export interface Product {
  readonly id: ProductId;
  readonly name: string;
  readonly unitSingular: string;
  readonly unitPlural: string;
}

export interface Destination {
  readonly id: DestinationId;
  readonly name: string;
  readonly region: string;
  /** Proposed route from Byblos, in plain language. */
  readonly route: string;
}

export interface HoldPosition {
  readonly id: HoldPositionId;
  readonly name: string;
  readonly location: string;
  readonly description: string;
}

export const PRODUCTS: Readonly<Record<ProductId, Product>> = {
  "purple-cloth": { id: "purple-cloth", name: "Purple cloth", unitSingular: "bundle", unitPlural: "bundles" },
  wine: { id: "wine", name: "Wine", unitSingular: "jar", unitPlural: "jars" },
};

export const DESTINATIONS: Readonly<Record<DestinationId, Destination>> = {
  kition: {
    id: "kition",
    name: "Kition",
    region: "Cyprus",
    route: "North-west across open water to the Phoenician town on Cyprus's south-east coast, about 200 km.",
  },
  egypt: {
    id: "egypt",
    name: "Egypt",
    region: "Nile Delta",
    route: "South along the Levantine coast or straight across open water to the Nile Delta, about 450 km.",
  },
};

export const HOLD_POSITIONS: Readonly<Record<HoldPositionId, HoldPosition>> = {
  "dry-locker": {
    id: "dry-locker",
    name: "Dry locker",
    location: "stern",
    description: "A covered space raised on brushwood, away from bilge water.",
  },
  "jar-bay": {
    id: "jar-bay",
    name: "Jar bay",
    location: "amidships",
    description: "Jars stand in rows, packed with brushwood and tied with rope.",
  },
};

export const DESTINATION_IDS = Object.keys(DESTINATIONS) as DestinationId[];
export const HOLD_POSITION_IDS = Object.keys(HOLD_POSITIONS) as HoldPositionId[];

export function unitLabel(product: Product, quantity: number): string {
  return quantity === 1 ? product.unitSingular : product.unitPlural;
}
