# Sprint 2 · First playable trade loop

**Goal** · Make a purple swatch at the dye stall, return to the harbor, and complete a sample shipment by selecting the goods, quantities, destination, and assigned stowage for each product.

Start with the joint [Sprint 1 walk-through](SPRINT_1.md). Check the ramp, collisions, and station spacing on the same Blender export. The 3D designer can extend the bay in parallel using the [harbor art brief](HARBOR_ART_BRIEF.md). Both programmers can build and test with the current blockout and placeholder UI.

## Programmer A · Purple-dye minigame

- Build a short 2D interaction launched from the dye stall. Give the player one clear task that produces a purple swatch and returns them to the harbor.
- Pass the completed result to the harbor so the cargo interaction can recognize the purple cloth as ready to ship. Verify that leaving an incomplete attempt leaves the cloth unavailable and that repeating a completed attempt does not duplicate cargo.
- Use the approved workshop references for the craft and describe the Byblos stall as a reconstruction. [Game vision](GAME_VISION.md)

## Programmer B · Shipping order and cargo

- Research plausible shipping routes and destination ports for **purple cloth and wine jars** in the c. 800–600 BCE setting. For each product, record a proposed route from Byblos, the destination, the source and its date, and whether the evidence establishes the route or provides a regional comparison. Label routes selected for the game as reconstructed where appropriate.
- Research shipboard stowage for each product. Start with the Ashkelon wrecks for transport jars and Mazarrón 2 for rope, baskets and packing material. Give **each product its own specific place aboard the ship**. A secured jar bay for wine and a separate dry compartment for cloth are starting proposals to review against the evidence. The exact hold layout is a game design choice. [Ashkelon wreck survey](https://www.journals.uchicago.edu/doi/10.2307/4126241), [Mazarrón 2 museum record](https://www.cultura.gob.es/mnarqua/en/colecciones/piezas-seleccionadas/fenicio/barco-2.html)
- Build one sample order with two goods and two destination choices. List the quantity and destination per product so each line can carry its own route. Use two labeled cargo positions in a simple hold diagram. Let the player select a product and its position by clicking. The dockworker should identify which order line to revisit when the player picks an incorrect quantity, destination, or stowage position.
- Keep the letter in plain language for this sprint. The letter's delivery timing, Phoenician sign exercise, and a full packing grid can follow after the basic loop works.

## Integration and acceptance

- At kickoff, agree on how the dye result reaches the cargo screen and whether one swatch represents one bundle or unlocks a prepared batch. Use a mock result to test the shipping interaction while the dye task is in progress.
- Before integration, review Programmer B's short product table together. Each row should name the product, proposed route and destination, stowage position, source, and any reconstruction choice. Confirm both routes and stowage positions before using them in the sample order.
- A first-time player can complete the dye task, return to the harbor, use the order to send the correct quantities to the right destinations, and assign each good to its designated place on the ship. An incorrect quantity, destination, or position produces feedback and the player can retry.
