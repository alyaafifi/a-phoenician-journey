// Unit tests for the Sprint 2 shipment rules and the dye-to-cargo contract.
// Node strips the TypeScript types, so these modules run without a build step.
import assert from "node:assert/strict";
import { describe, test } from "node:test";
import { checkShipment, emptyPlan, SAMPLE_ORDER } from "../src/trade/shipment.ts";
import { PREPARED_CLOTH_BUNDLES, STAGED_WINE_JARS, TradeState } from "../src/trade/tradeState.ts";

const READY = { "purple-cloth": PREPARED_CLOTH_BUNDLES, wine: STAGED_WINE_JARS };

function correctPlan() {
  const plan = emptyPlan();
  for (const line of SAMPLE_ORDER) {
    plan[line.product] = { quantity: line.quantity, destination: line.destination, stowage: line.stowage };
  }
  return plan;
}

describe("sample order", () => {
  test("has two goods, two destinations, and two hold positions", () => {
    assert.equal(SAMPLE_ORDER.length, 2);
    assert.equal(new Set(SAMPLE_ORDER.map((line) => line.product)).size, 2);
    assert.equal(new Set(SAMPLE_ORDER.map((line) => line.destination)).size, 2);
    assert.equal(new Set(SAMPLE_ORDER.map((line) => line.stowage)).size, 2);
  });

  test("asks for no more than the landing can supply", () => {
    for (const line of SAMPLE_ORDER) {
      assert.ok(line.quantity <= READY[line.product], `line ${line.line}`);
    }
  });
});

describe("checkShipment", () => {
  test("accepts the shipment the letter describes", () => {
    assert.deepEqual(checkShipment(SAMPLE_ORDER, correctPlan(), READY), []);
  });

  test("flags missing cloth before anything else on its line", () => {
    const problems = checkShipment(SAMPLE_ORDER, correctPlan(), { ...READY, "purple-cloth": 0 });
    assert.deepEqual(problems, [{ line: 1, product: "purple-cloth", field: "availability", missing: true }]);
  });

  test("points to the line with the wrong quantity", () => {
    const plan = correctPlan();
    plan.wine.quantity = 4;
    assert.deepEqual(checkShipment(SAMPLE_ORDER, plan, READY), [
      { line: 2, product: "wine", field: "quantity", missing: false },
    ]);
  });

  test("points to the line with the wrong destination", () => {
    const plan = correctPlan();
    plan["purple-cloth"].destination = "egypt";
    assert.deepEqual(checkShipment(SAMPLE_ORDER, plan, READY), [
      { line: 1, product: "purple-cloth", field: "destination", missing: false },
    ]);
  });

  test("points to the line with the wrong stowage position", () => {
    const plan = correctPlan();
    plan.wine.stowage = "dry-locker";
    assert.deepEqual(checkShipment(SAMPLE_ORDER, plan, READY), [
      { line: 2, product: "wine", field: "stowage", missing: false },
    ]);
  });

  test("reports one problem per line, in letter order, and marks unchosen values", () => {
    const problems = checkShipment(SAMPLE_ORDER, emptyPlan(), READY);
    assert.deepEqual(
      problems.map(({ line, field, missing }) => [line, field, missing]),
      [
        [1, "quantity", true],
        [2, "quantity", true],
      ],
    );
  });

  test("checks destination before stowage on the same line", () => {
    const plan = correctPlan();
    plan.wine.destination = null;
    plan.wine.stowage = null;
    assert.deepEqual(checkShipment(SAMPLE_ORDER, plan, READY), [
      { line: 2, product: "wine", field: "destination", missing: true },
    ]);
  });
});

describe("TradeState dye contract", () => {
  test("cloth stays unavailable until the dye task is completed", () => {
    const state = new TradeState();
    assert.equal(state.dyeTaskComplete, false);
    assert.equal(state.availableOf("purple-cloth"), 0);
    assert.equal(state.availableOf("wine"), STAGED_WINE_JARS);
  });

  test("completing the dye task unlocks the prepared batch once", () => {
    const state = new TradeState();
    let notifications = 0;
    state.subscribe(() => notifications++);

    assert.equal(state.completeDyeTask(), true);
    assert.equal(state.completeDyeTask(), false);
    assert.equal(state.availableOf("purple-cloth"), PREPARED_CLOTH_BUNDLES);
    assert.equal(notifications, 1);
  });

  test("records the shipment once", () => {
    const state = new TradeState();
    let notifications = 0;
    state.subscribe(() => notifications++);
    state.markShipmentSent();
    state.markShipmentSent();
    assert.equal(state.shipmentSent, true);
    assert.equal(notifications, 1);
  });
});
