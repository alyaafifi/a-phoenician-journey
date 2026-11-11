import { ArcRotateCamera } from "@babylonjs/core/Cameras/arcRotateCamera";
import { Engine } from "@babylonjs/core/Engines/engine";
import { HemisphericLight } from "@babylonjs/core/Lights/hemisphericLight";
import { Vector3 } from "@babylonjs/core/Maths/math.vector";
import { CreateBox } from "@babylonjs/core/Meshes/Builders/boxBuilder";
import { Scene } from "@babylonjs/core/scene";
import "@babylonjs/core/Materials/standardMaterial";
import "./style.css";

const canvas = document.querySelector<HTMLCanvasElement>("#scene");
if (!canvas) {
  throw new Error("The scene canvas is missing.");
}

const engine = new Engine(canvas, true);
const scene = new Scene(engine);
const camera = new ArcRotateCamera("camera", -Math.PI / 3, Math.PI / 3, 5, Vector3.Zero(), scene);
camera.attachControl(canvas, true);

new HemisphericLight("light", Vector3.Up(), scene);
CreateBox("placeholder", {}, scene);

engine.runRenderLoop(() => scene.render());
window.addEventListener("resize", () => engine.resize());
