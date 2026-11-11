# Sprint 1 · Harbor landing blockout

**Timebox** — One half-day for two programmers working in parallel.

**Goal** — Walk from the upper approach to a rough Byblos landing in the browser and verify the shoreline, scale, and space for the three stations.

## Parallel work

| Owner | Work | Deliverable |
| --- | --- | --- |
| Programmer A · Blender | Block out one section of the southern shore. Use a water plane, a sandy ramp, a flat dry cargo area, and a raised landward slope. Add a boat proxy, a human-height reference, and footprints for the dye stall and scribe's table. Start with 2.5–3 m of clear walking space. Export the rough mesh as soon as the ramp and cargo area are in place. | Editable Blender scene and one `.glb` for browser testing. |
| Programmer B · Babylon.js | Use primitives to set up a test camera, movement, and simple ground collision. Set up the glTF import and a way to swap in the Blender export. Use the placeholders to verify that the browser setup works while the asset is being built. | A browser scene that loads the `.glb` and lets both programmers walk the landing. |

## Shared handoff

1. **At kickoff** — Agree on the asset path, spawn position, and approach direction. Use one Blender unit per metre and put the waterline at Blender `Z = 0`. Use a 1.7 m reference marker to check scale after import and confirm the waterline maps to Babylon.js `Y = 0`.
2. **Midway through the sprint** — Swap in the first `.glb` and walk from the upper approach to the dry cargo area. Check that the ramp feels natural at camera height, the boat sits at the water's edge, and a person can pass each station footprint.
3. **Before wrapping up** — Programmer A adjusts the mesh and Programmer B checks the import and collisions. Re-export and repeat the walk to verify the same scale and alignment.

## Done when

- The browser loads the Blender export at the agreed scale and orientation.
- The upper approach, ramp, and dry cargo area form a continuous walkable route.
- The boat and three station footprints fit without narrowing the path.
- Both programmers have reviewed the same export in the browser.

Sprint 2 extends the tested landing into the bay with the sandstone promontory and El-Yasmine islet. Player flow and interactions come later.
