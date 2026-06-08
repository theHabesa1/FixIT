# FixIT — Retro Repair Shop

A 2D pixel-art game made in **Unity 6 (6000.4.10f1)**. You run a computer repair
shop: fix broken devices by solving CS mini-puzzles (logic gates, binary decoding,
circuit tracing, RAM matching) and sell hardware by fetching the right parts from
the back store. A per-topic **finite-state machine** (Novice → Intermediate →
Expert) scales difficulty and rewards as you improve.

![Build macOS](https://github.com/theHabesa1/FixIT/actions/workflows/build.yml/badge.svg)

## Play it (macOS)

Two ways to get a macOS build:

- **Releases** → download `FixIT-macOS.zip` from the
  [Releases page](https://github.com/theHabesa1/FixIT/releases).
- **Actions** → open the latest [Build (macOS)](https://github.com/theHabesa1/FixIT/actions)
  run → download the `FixIT-macOS` artifact.

Unzip, then run `FixIT.app`. macOS Gatekeeper blocks unsigned apps, so the first
time **right-click the app → Open → Open** (or run
`xattr -dr com.apple.quarantine FixIT.app`).

**Controls:** WASD/arrows move · **E** interact · **R** back to shop · Enter/click on the title screen.

## Build it yourself in the Editor

Open the project in Unity 6, then **File ▸ Build Settings ▸ macOS ▸ Build**.
(See `SETUP_GUIDE.md` for the scene list and how the code-driven setup works.)

## Building on GitHub (CI) — one-time license setup

The workflow in `.github/workflows/build.yml` builds macOS automatically, but Unity
needs a license. Add it once under **Settings ▸ Secrets and variables ▸ Actions**:

**Personal (free) license:**
1. Run the workflow once — it will fail and the log prints a way to get a
   `Unity_v6000.x.alf` request file (or generate it locally with the Unity Hub).
2. Go to <https://license.unity3d.com/manual>, upload the `.alf`, download the
   resulting `.ulf` file.
3. Create a secret named **`UNITY_LICENSE`** and paste the entire contents of the
   `.ulf` file.

**Plus/Pro license (serial):** instead add secrets **`UNITY_EMAIL`**,
**`UNITY_PASSWORD`**, and **`UNITY_SERIAL`**.

Then push to `main` (or run the workflow manually) to produce a build. To cut a
downloadable release, push a tag:

```bash
git tag v1.0
git push origin v1.0
```

## Project layout

- `Assets/Scripts/` — all gameplay code (code-driven; scenes need only one
  component each).
- `Assets/Scenes/` — RepairShop, LogicGate, BinaryDecoder, CircuitTracer, RAMMatcher.
- `SETUP_GUIDE.md` — how the project is wired up.
