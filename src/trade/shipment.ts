import type { DestinationId, HoldPositionId, ProductId } from "./catalog";

/** One line of the shipping letter. Each line carries its own quantity, route, and stowage. */
export interface OrderLine {
  readonly line: number;
  readonly product: ProductId;
  readonly quantity: number;
  readonly destination: DestinationId;
  readonly stowage: HoldPositionId;
  /** Plain-language wording shown in the letter. */
  readonly text: string;
}

/** What the player has chosen for one product on the cargo screen. */
export interface LinePlan {
  quantity: number;
  destination: DestinationId | null;
  stowage: HoldPositionId | null;
}

export type ShipmentPlan = Record<ProductId, LinePlan>;
export type Availability = Readonly<Record<ProductId, number>>;

export type ProblemField = "availability" | "quantity" | "destination" | "stowage";

/** The first thing wrong with a line, so the dockworker can point the player back to it. */
export interface ShipmentProblem {
  readonly line: number;
  readonly product: ProductId;
  readonly field: ProblemField;
  /** True when the player has not chosen a value yet, as opposed to choosing a wrong one. */
  readonly missing: boolean;
}

/**
 * The sample order for Sprint 2: one letter, two goods, two destinations.
 * The routes are reconstructed; see CARGO_RESEARCH.md.
 */
export const SAMPLE_ORDER: readonly OrderLine[] = [
  {
    line: 1,
    product: "purple-cloth",
    quantity: 2,
    destination: "kition",
    stowage: "dry-locker",
    text: "Send two bundles of purple cloth to Kition on Cyprus. Keep them in the dry locker at the stern.",
  },
  {
    line: 2,
    product: "wine",
    quantity: 3,
    destination: "egypt",
    stowage: "jar-bay",
    text: "Send three jars of wine to Egypt. Stand them in the jar bay amidships.",
  },
];

export function emptyPlan(): ShipmentPlan {
  return {
    "purple-cloth": { quantity: 0, destination: null, stowage: null },
    wine: { quantity: 0, destination: null, stowage: null },
  };
}

/**
 * Checks the plan line by line, in letter order. Within a line the checks run from
 * availability to quantity, destination, and stowage, so the dockworker raises one
 * clear problem at a time.
 */
export function checkShipment(
  order: readonly OrderLine[],
  plan: ShipmentPlan,
  available: Availability,
): ShipmentProblem[] {
  const problems: ShipmentProblem[] = [];
  for (const line of order) {
    const chosen = plan[line.product];
    const base = { line: line.line, product: line.product };
    if (available[line.product] < line.quantity) {
      problems.push({ ...base, field: "availability", missing: available[line.product] === 0 });
    } else if (chosen.quantity !== line.quantity) {
      problems.push({ ...base, field: "quantity", missing: chosen.quantity === 0 });
    } else if (chosen.destination !== line.destination) {
      problems.push({ ...base, field: "destination", missing: chosen.destination === null });
    } else if (chosen.stowage !== line.stowage) {
      problems.push({ ...base, field: "stowage", missing: chosen.stowage === null });
    }
  }
  return problems;
}
