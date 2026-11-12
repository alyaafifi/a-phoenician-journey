#!/usr/bin/env node
// Generates the game's voice lines and sound effects with ElevenLabs, from a cue file.
//
//   node tools/audio/generate-elevenlabs.mjs --list-voices
//   node tools/audio/generate-elevenlabs.mjs tools/audio/classroom-cues.json [--only id,id] [--force]
//
// The API key is read from the ELEVENLABS_API_KEY environment variable. Never put it in a
// file in this repository. Existing clips are skipped unless --force is given, so reruns
// don't spend credits. Clips are written as WAV (from PCM) or MP3, depending on outputFormat.

import { mkdir, readFile, writeFile, access } from "node:fs/promises";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const API = "https://api.elevenlabs.io/v1";
const repoRoot = resolve(dirname(fileURLToPath(import.meta.url)), "../..");

function fail(message) {
  console.error(`error: ${message}`);
  process.exit(1);
}

const apiKey = process.env.ELEVENLABS_API_KEY;
if (!apiKey) {
  fail("Set ELEVENLABS_API_KEY in this terminal first (PowerShell: $env:ELEVENLABS_API_KEY = \"...\").");
}

async function call(path, init = {}) {
  const response = await fetch(`${API}${path}`, {
    ...init,
    headers: { "xi-api-key": apiKey, ...(init.body ? { "Content-Type": "application/json" } : {}), ...init.headers },
  });
  if (!response.ok) {
    const detail = await response.text();
    fail(`${init.method ?? "GET"} ${path} returned HTTP ${response.status}: ${detail.slice(0, 500)}`);
  }
  return response;
}

async function listVoices() {
  const body = await (await call("/voices")).json();
  for (const voice of body.voices ?? []) {
    const labels = Object.values(voice.labels ?? {}).join(", ");
    console.log(`${voice.voice_id}  ${voice.name.padEnd(28)} ${labels}`);
  }
}

/** Wraps raw 16-bit PCM in a WAV header so Unity can import it. */
function wav(pcm, sampleRate, channels = 1) {
  const header = Buffer.alloc(44);
  header.write("RIFF", 0);
  header.writeUInt32LE(36 + pcm.length, 4);
  header.write("WAVE", 8);
  header.write("fmt ", 12);
  header.writeUInt32LE(16, 16);
  header.writeUInt16LE(1, 20); // PCM
  header.writeUInt16LE(channels, 22);
  header.writeUInt32LE(sampleRate, 24);
  header.writeUInt32LE(sampleRate * channels * 2, 28);
  header.writeUInt16LE(channels * 2, 32);
  header.writeUInt16LE(16, 34);
  header.write("data", 36);
  header.writeUInt32LE(pcm.length, 40);
  return Buffer.concat([header, pcm]);
}

async function exists(path) {
  try {
    await access(path);
    return true;
  } catch {
    return false;
  }
}

/** Scales 16-bit PCM so its loudest sample reaches the given fraction of full scale. */
function normalize(pcm, targetPeak) {
  let peak = 0;
  for (let i = 0; i + 1 < pcm.length; i += 2) peak = Math.max(peak, Math.abs(pcm.readInt16LE(i)));
  if (peak === 0) return pcm;
  const gain = (targetPeak * 32767) / peak;
  const out = Buffer.alloc(pcm.length);
  for (let i = 0; i + 1 < pcm.length; i += 2) {
    out.writeInt16LE(Math.max(-32768, Math.min(32767, Math.round(pcm.readInt16LE(i) * gain))), i);
  }
  return out;
}

async function generate(cueFile, only, force) {
  const sheet = JSON.parse(await readFile(cueFile, "utf8"));
  const outDir = join(repoRoot, sheet.outputDir);
  await mkdir(outDir, { recursive: true });

  for (const cue of sheet.cues) {
    if (only && !only.includes(cue.id)) continue;
    // Speech and sound effects can use different formats. Sound effects come back at 48 kHz.
    const format = (cue.type === "sfx" ? sheet.sfxOutputFormat : null) ?? sheet.outputFormat ?? "mp3_44100_128";
    const isPcm = format.startsWith("pcm_");
    const extension = isPcm ? "wav" : "mp3";
    const target = join(outDir, `${cue.id}.${extension}`);
    if (!force && (await exists(target))) {
      console.log(`skip  ${cue.id} (already generated; use --force to replace)`);
      continue;
    }

    let response;
    if (cue.type === "speech") {
      const voice = sheet.voices?.[cue.voice];
      if (!voice?.voiceId) fail(`Cue "${cue.id}" needs a voiceId for voice "${cue.voice}". Run --list-voices and add one.`);
      response = await call(`/text-to-speech/${voice.voiceId}?output_format=${format}`, {
        method: "POST",
        body: JSON.stringify({ text: cue.text, model_id: voice.modelId, voice_settings: voice.settings }),
      });
    } else if (cue.type === "sfx") {
      const body = { text: cue.text, duration_seconds: cue.durationSeconds, prompt_influence: cue.promptInfluence };
      if (cue.loop) body.loop = true;
      response = await call(`/sound-generation?output_format=${format}`, { method: "POST", body: JSON.stringify(body) });
    } else {
      fail(`Cue "${cue.id}" has unknown type "${cue.type}".`);
    }

    let audio = Buffer.from(await response.arrayBuffer());
    if (isPcm && cue.normalizePeak) audio = normalize(audio, cue.normalizePeak);
    // Timed effects may be returned as interleaved stereo PCM.
    const sampleRate = isPcm ? Number(format.split("_")[1]) : 0;
    const stereo = cue.type === "sfx" && audio.length === sampleRate * cue.durationSeconds * 4;
    const file = isPcm ? wav(audio, sampleRate, stereo ? 2 : 1) : audio;
    await writeFile(target, file);
    console.log(`wrote ${cue.id} -> ${sheet.outputDir}/${cue.id}.${extension} (${(file.length / 1024).toFixed(0)} KB)`);
  }
}

const args = process.argv.slice(2);
if (args.includes("--list-voices")) {
  await listVoices();
} else {
  const cueFile = args.find((a) => !a.startsWith("--"));
  if (!cueFile) fail("Pass a cue file, for example tools/audio/classroom-cues.json, or --list-voices.");
  const onlyIndex = args.indexOf("--only");
  const only = onlyIndex >= 0 ? args[onlyIndex + 1]?.split(",") : null;
  await generate(resolve(cueFile), only, args.includes("--force"));
}
