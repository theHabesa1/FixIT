# FixIT — Unity Setup Guide (Code-Driven)

Everything is built in **code**. You do almost no Inspector wiring. Each scene
just needs ONE empty GameObject with one script attached.

---

## Step 1 — Create the Unity project

1. Open **Unity Hub** → New Project → **2D (Core)** template → name it `FixIT`.
   (Unity 6 LTS or 2022 LTS both work — the code avoids version-specific APIs.)
2. When the editor opens: **Window > TextMeshPro > Import TMP Essential Resources**
   (one click — required for any text to render).

## Step 2 — Add the scripts

1. In the Project window, find the `Assets` folder.
2. Drag the entire `Assets/Scripts` folder from this project into Unity's `Assets`.
3. Wait for Unity to finish compiling. The Console should show **no errors**.

> `GameManager` auto-creates itself before any scene loads (via
> `RuntimeInitializeOnLoadMethod`), so you never place it manually. You can press
> Play on ANY scene and it just works — handy for testing each mini-game alone.

## Step 3 — Create the 5 scenes

Create 5 empty scenes (**File > New Scene > Empty**, then Save As) named EXACTLY:

| Scene name      | Add this component to an empty GameObject |
|-----------------|-------------------------------------------|
| `RepairShop`    | `ShopBootstrap`                           |
| `LogicGate`     | `LogicGateManager`                        |
| `BinaryDecoder` | `BinaryManager`                           |
| `CircuitTracer` | `CircuitManager`                          |
| `RAMMatcher`    | `RAMManager`                              |

> The **Back Store** is NOT a separate scene — it's a second room built inside
> `RepairShop`. Walk through the right-hand door to enter it.

For each scene:
1. Right-click in Hierarchy → **Create Empty** → name it `Manager`.
2. With it selected, **Add Component** → type the script name → add it.
3. Save the scene.

## Step 4 — Register scenes in Build Settings

**File > Build Settings > Add Open Scenes.** Add all 5, and make sure
**`RepairShop` is at the top (index 0)**. Order of the rest doesn't matter.

## Step 5 — Play

Open `RepairShop`, press **Play**.
- Move with **WASD / arrow keys**.
- Walk up to a customer and press **E** (or click them) to start their repair.
- Each device type loads a different mini-game; difficulty scales with mastery.
- Press **R** any time in a mini-game to return to the shop.

---

## Optional — Pixel fonts (matches the design exactly)

The game runs fine with Unity's default font. To get the retro look:

1. Download **Press Start 2P** and **VT323** TTFs from Google Fonts.
2. In Unity: `Assets/Resources/Fonts/` (create these folders).
3. Import both TTFs there.
4. **Window > TextMeshPro > Font Asset Creator** → generate a TMP Font Asset for
   each. Name them exactly **`PressStart2P`** and **`VT323`**, save under
   `Assets/Resources/Fonts/`.

`Theme.cs` loads them automatically by name if present.

---

## Input note

If the Console says input errors at Play, your project is set to the new Input
System only. Fix: **Edit > Project Settings > Player > Active Input Handling →
"Both"**, then restart the editor.

---

## What each script does

| Script | Role |
|--------|------|
| `Core/GameManager` | Singleton: knowledge, money, current customer. Auto-bootstraps. |
| `Core/KnowledgeSystem` | **Peculiar feature** — FSM per topic (Novice/Intermediate/Expert). |
| `Core/SceneLoader` | Scene transitions + fade. |
| `Core/Theme` | Design colour palette + fonts. |
| `Core/UIFactory` | Builds all UI (canvas, panels, buttons, text) in code. |
| `Core/Sfx` | Procedural beeps — no audio files needed. |
| `Shop/ShopBootstrap` | Builds the whole overworld in code. |
| `Shop/CustomerSpawner` / `CustomerAI` | Spawns + animates clickable customers. |
| `Player/PlayerController` / `PlayerInteract` | WASD movement + E to interact. |
| `UI/HUDController` / `MasteryBoard` / `GameOverScreen` | Self-building HUD, board, result overlay. |
| `LogicGate/*` | Logic Gate puzzle (toggle inputs to hit target output). |
| `BinaryDecoder/*` | Binary→decimal terminal puzzle. |
| `CircuitTracer/*` | Click a path from battery to bulb (DFS validation). |
| `RAMMatcher/*` | Drag RAM into dual-channel slots. |

---

## Knowledge System — for your oral exam

Each CS topic is a **finite state machine** with 3 states (Novice→Intermediate→
Expert). Transitions are deterministic, driven by performance:
- **3 correct answers in a row → promote**
- **2 wrong answers in a row → demote**

The puzzle generators (`GatePuzzleGen`, `BinaryPuzzleGen`, `CircuitManager`,
`RAMManager`) read the current mastery level and pick the difficulty tier. See
`KnowledgeSystem.RecordResult()` for the transition logic.

---

## Submission Checklist (Due June 18, 11:59am)

- [ ] Push code to public GitHub/GitLab
- [ ] Zip `Assets/Scripts/` → `scripts.zip`
- [ ] Export report PDF (max 4 pages)
- [ ] Email to: francesco.tiezzi@unifi.it
- [ ] Subject: `[Game Development Project] YourName`
- [ ] Attach: report.pdf + scripts.zip + repo link
