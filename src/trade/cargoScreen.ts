import {
  DESTINATION_IDS,
  DESTINATIONS,
  HOLD_POSITION_IDS,
  HOLD_POSITIONS,
  PRODUCTS,
  unitLabel,
  type DestinationId,
  type HoldPositionId,
  type ProductId,
} from "./catalog";
import { checkShipment, emptyPlan, SAMPLE_ORDER, type ShipmentPlan, type ShipmentProblem } from "./shipment";
import type { TradeState } from "./tradeState";

const GREETING = "Choose a good, set how many, pick where it is going, then click its place in the hold.";

/** What the dockworker says about a problem, pointing back to the letter line. */
export function dockworkerMessage(problem: ShipmentProblem): string {
  const product = PRODUCTS[problem.product];
  const item = product.name.toLowerCase();
  const line = `Line ${problem.line}`;
  switch (problem.field) {
    case "availability":
      return problem.product === "purple-cloth"
        ? `${line} asks for purple cloth, but none is ready. Finish the swatch at the dye stall, then come back.`
        : `${line} asks for more ${item} than we have on the landing.`;
    case "quantity":
      return problem.missing
        ? `${line} needs ${item}. Set how many ${product.unitPlural} to load.`
        : `Read ${line.toLowerCase()} again. That's not the right number of ${product.unitPlural} of ${item}.`;
    case "destination":
      return problem.missing
        ? `${line}: where is the ${item} going? Pick its destination.`
        : `Read ${line.toLowerCase()} again. The ${item} is going somewhere else.`;
    case "stowage":
      return problem.missing
        ? `${line}: the ${item} isn't stowed yet. Select it, then click its place in the hold.`
        : `Read ${line.toLowerCase()} again. The ${item} belongs in a different place in the hold.`;
  }
}

function element<K extends keyof HTMLElementTagNameMap>(
  tag: K,
  options: { className?: string; text?: string; attrs?: Record<string, string> } = {},
  children: (Node | string)[] = [],
): HTMLElementTagNameMap[K] {
  const node = document.createElement(tag);
  if (options.className) {
    node.className = options.className;
  }
  if (options.text !== undefined) {
    node.textContent = options.text;
  }
  for (const [name, value] of Object.entries(options.attrs ?? {})) {
    node.setAttribute(name, value);
  }
  node.append(...children);
  return node;
}

/**
 * The 2D cargo interaction opened from the cargo station. The player reads the letter,
 * sets quantity and destination per good, assigns each good to a place in the hold,
 * and sends the ship. The dockworker points back to the first letter line that is wrong.
 */
export class CargoScreen {
  private readonly root: HTMLElement;
  private plan: ShipmentPlan = emptyPlan();
  private selected: ProductId | null = null;
  private message = GREETING;
  private flaggedLine: number | null = null;
  private onClose: (() => void) | null = null;

  constructor(
    host: HTMLElement,
    private readonly state: TradeState,
  ) {
    this.root = element("div", {
      className: "cargo-screen",
      attrs: { role: "dialog", "aria-modal": "true", "aria-labelledby": "cargo-title", hidden: "" },
    });
    host.append(this.root);
    this.root.addEventListener("keydown", (event) => {
      if (event.key === "Escape") {
        event.preventDefault();
        this.close();
      }
    });
    state.subscribe(() => {
      if (this.isOpen) {
        this.render();
      }
    });
  }

  get isOpen(): boolean {
    return !this.root.hidden;
  }

  open(onClose: () => void): void {
    this.onClose = onClose;
    this.root.hidden = false;
    this.render();
    this.root.querySelector<HTMLElement>("button")?.focus();
  }

  close(): void {
    if (!this.isOpen) {
      return;
    }
    this.root.hidden = true;
    const callback = this.onClose;
    this.onClose = null;
    callback?.();
  }

  private send(): void {
    const problems = checkShipment(SAMPLE_ORDER, this.plan, this.state.available);
    const first = problems[0];
    if (first) {
      this.message = dockworkerMessage(first);
      this.flaggedLine = first.line;
    } else {
      this.state.markShipmentSent();
      this.flaggedLine = null;
    }
    this.render();
    this.root.querySelector<HTMLElement>(first ? ".dockworker" : ".cargo-close")?.focus();
  }

  private update(change: () => void): void {
    change();
    this.flaggedLine = null;
    this.message = GREETING;
    this.render();
  }

  private render(): void {
    const focusKey = (document.activeElement as HTMLElement | null)?.dataset?.focusKey;
    this.root.replaceChildren(
      element("div", { className: "cargo-panel" }, [
        element("header", { className: "cargo-header" }, [
          element("h2", { text: "Loading the ship at Byblos", attrs: { id: "cargo-title" } }),
          element("button", {
            className: "cargo-close",
            text: "Back to the harbor",
            attrs: { type: "button", "data-focus-key": "close" },
          }),
        ]),
        this.state.shipmentSent ? this.renderSent() : this.renderLoading(),
      ]),
    );
    this.root.querySelector(".cargo-close")?.addEventListener("click", () => this.close());
    if (focusKey) {
      this.root.querySelector<HTMLElement>(`[data-focus-key="${focusKey}"]`)?.focus();
    }
  }

  private renderLetter(): HTMLElement {
    return element("section", { className: "cargo-letter", attrs: { "aria-label": "Shipping letter" } }, [
      element("h3", { text: "The shipping letter" }),
      element("p", { className: "cargo-letter-from", text: "To the dockmaster at Byblos:" }),
      element(
        "ol",
        {},
        SAMPLE_ORDER.map((line) =>
          element("li", {
            className: line.line === this.flaggedLine ? "flagged" : "",
            text: line.text,
            attrs: line.line === this.flaggedLine ? { "aria-current": "true" } : {},
          }),
        ),
      ),
    ]);
  }

  private renderLoading(): HTMLElement {
    const goods = element("section", { className: "cargo-goods", attrs: { "aria-label": "Goods on the landing" } }, [
      element("h3", { text: "Goods on the landing" }),
      ...SAMPLE_ORDER.map((line) => this.renderGood(line.product)),
    ]);

    const hold = element("section", { className: "cargo-hold", attrs: { "aria-label": "The ship's hold" } }, [
      element("h3", { text: "The ship's hold" }),
      element("p", {
        className: "cargo-hint",
        text: this.selected
          ? `Click a place in the hold for the ${PRODUCTS[this.selected].name.toLowerCase()}.`
          : "Select a good, then click its place in the hold.",
      }),
      element("div", { className: "ship" }, [
        element("span", { className: "ship-end ship-stern", text: "Stern" }),
        ...HOLD_POSITION_IDS.map((id) => this.renderHoldSlot(id)),
        element("span", { className: "ship-end ship-bow", text: "Bow" }),
      ]),
    ]);

    const dockworker = element(
      "div",
      { className: "dockworker", attrs: { role: "status", tabindex: "-1" } },
      [element("strong", { text: "Dockworker: " }), this.message],
    );
    const send = element("button", {
      className: "cargo-send",
      text: "Load and send the ship",
      attrs: { type: "button", "data-focus-key": "send" },
    });
    send.addEventListener("click", () => this.send());

    return element("div", { className: "cargo-body" }, [
      this.renderLetter(),
      element("div", { className: "cargo-work" }, [goods, hold, dockworker, send]),
    ]);
  }

  private renderGood(productId: ProductId): HTMLElement {
    const product = PRODUCTS[productId];
    const plan = this.plan[productId];
    const available = this.state.availableOf(productId);
    const isSelected = this.selected === productId;

    const select = element("button", {
      className: "good-select",
      attrs: { type: "button", "aria-pressed": String(isSelected), "data-focus-key": `select-${productId}` },
    }, [
      element("span", { className: `good-swatch good-${productId}`, attrs: { "aria-hidden": "true" } }),
      element("span", { className: "good-name", text: product.name }),
      element("span", {
        className: "good-stock",
        text:
          available > 0
            ? `${available} ${unitLabel(product, available)} ready`
            : "Not ready. Finish the swatch at the dye stall.",
      }),
    ]);
    // Selecting never toggles off, so a repeated click can't silently cancel the next placement.
    select.addEventListener("click", () => this.update(() => (this.selected = productId)));

    const minus = element("button", {
      text: "−",
      attrs: { type: "button", "aria-label": `One fewer ${product.unitSingular}`, "data-focus-key": `minus-${productId}` },
    });
    const plus = element("button", {
      text: "+",
      attrs: { type: "button", "aria-label": `One more ${product.unitSingular}`, "data-focus-key": `plus-${productId}` },
    });
    minus.disabled = plan.quantity <= 0;
    plus.disabled = plan.quantity >= available;
    minus.addEventListener("click", () => this.update(() => (plan.quantity = Math.max(0, plan.quantity - 1))));
    plus.addEventListener("click", () => this.update(() => (plan.quantity = Math.min(available, plan.quantity + 1))));
    const quantity = element("div", { className: "good-quantity", attrs: { role: "group", "aria-label": "Quantity" } }, [
      minus,
      element("output", { text: `${plan.quantity} ${unitLabel(product, plan.quantity)}`, attrs: { "aria-live": "polite" } }),
      plus,
    ]);

    const destinations = element(
      "div",
      { className: "good-destinations", attrs: { role: "radiogroup", "aria-label": `Destination for ${product.name.toLowerCase()}` } },
      DESTINATION_IDS.map((id: DestinationId) => {
        const destination = DESTINATIONS[id];
        const chosen = plan.destination === id;
        const button = element("button", {
          className: chosen ? "chosen" : "",
          text: `${destination.name} (${destination.region})`,
          attrs: { type: "button", role: "radio", "aria-checked": String(chosen), "data-focus-key": `dest-${productId}-${id}` },
        });
        button.addEventListener("click", () => this.update(() => (plan.destination = id)));
        return button;
      }),
    );

    const stowed = element("p", {
      className: "good-stowed",
      text: plan.stowage ? `Stowed in the ${HOLD_POSITIONS[plan.stowage].name.toLowerCase()}` : "Not stowed yet",
    });

    return element("article", { className: `good${isSelected ? " selected" : ""}` }, [select, quantity, destinations, stowed]);
  }

  private renderHoldSlot(id: HoldPositionId): HTMLElement {
    const position = HOLD_POSITIONS[id];
    const stowedHere = SAMPLE_ORDER.map((line) => line.product).filter((product) => this.plan[product].stowage === id);
    const slot = element("button", {
      className: `hold-slot hold-${id}`,
      attrs: {
        type: "button",
        "data-focus-key": `hold-${id}`,
        "aria-label": `${position.name}, ${position.location}. ${position.description}${
          stowedHere.length ? ` Holds ${stowedHere.map((p) => PRODUCTS[p].name.toLowerCase()).join(" and ")}.` : " Empty."
        }`,
      },
    }, [
      element("span", { className: "hold-name", text: position.name }),
      element("span", { className: "hold-location", text: position.location }),
      element("span", { className: "hold-description", text: position.description }),
      element(
        "span",
        { className: "hold-contents" },
        stowedHere.map((product) => element("span", { className: `good-swatch good-${product}`, attrs: { title: PRODUCTS[product].name } })),
      ),
    ]);
    slot.disabled = this.selected === null;
    slot.addEventListener("click", () => {
      const product = this.selected;
      if (product) {
        this.update(() => {
          this.plan[product].stowage = id;
          this.selected = null;
        });
      }
    });
    return slot;
  }

  private renderSent(): HTMLElement {
    return element("div", { className: "cargo-body cargo-sent" }, [
      this.renderLetter(),
      element("section", { className: "cargo-work", attrs: { "aria-label": "Shipment sent" } }, [
        element("h3", { text: "The ship sails" }),
        element("p", { className: "dockworker", attrs: { role: "status" } }, [
          element("strong", { text: "Dockworker: " }),
          "Everything matches the letter. Fair winds!",
        ]),
        element(
          "ul",
          { className: "cargo-summary" },
          SAMPLE_ORDER.map((line) => {
            const product = PRODUCTS[line.product];
            const destination = DESTINATIONS[line.destination];
            return element("li", {}, [
              element("strong", {
                text: `${line.quantity} ${unitLabel(product, line.quantity)} of ${product.name.toLowerCase()} to ${destination.name}. `,
              }),
              destination.route,
            ]);
          }),
        ),
        element("p", {
          className: "cargo-note",
          text:
            "What the evidence shows: Byblos was among the kingdoms whose tribute to Assyria included multicoloured " +
            "garments and purple-dyed wool. Two 8th-century BCE Phoenician wrecks off Ashkelon carried wine in jars, " +
            "probably bound for Egypt or Carthage. No record survives of this exact shipment. The routes, quantities, " +
            "and the ship's hold are reconstructed for the game.",
        }),
      ]),
    ]);
  }
}
