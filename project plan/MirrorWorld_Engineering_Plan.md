# MIRROR WORLD (OpenTK C#) — 2-Week Master Plan

**Software Engineering & Clean Architecture Specification · 2D OpenGL Platformer Edition**

A **2D dual-world platformer**: everything your character does is mirrored in a second world below it, where gravity is flipped and the level is different. Collect the key and reach the door by satisfying **both worlds at once**. Scoped for **two weeks** and engineered around **SOLID C# (<200-line modules, Newspaper Layout)**, a clean Git workflow and headless xUnit tests.

| | |
|---|---|
| **Owner** | You (individual project) |
| **Scope** | 2 weeks · 16 tickets · 4 levels |
| **Stack** | C# .NET 8 + OpenTK 4 · OpenGL 3.3 Core · GLSL 330 + FBO · xUnit |

---

## Core Concept: Same Input, Flipped Physics

One keyboard drives **two characters**. The Real World (top) and Mirror World (bottom) are stacked around a mirror line using two `glViewport`s. A **MirrorTransform** (`scale(1, -1)`) reflects the world, and gravity is inverted below the line, so a jump sends the mirror self the other way. Horizontal input stays the same in both worlds. Spikes, platforms and keys differ per world: the key may exist only below, the door only above.

```
+--------------------------------------------+
| REAL WORLD                                 |
|   o                          [door]        |
| ======== ground ====  ^^^  ==== ground ===  |
| - - - - - - - - mirror line - - - - - - - -|
| ======== ground ====  ===  ==== ground ===  |
|   o  (upside down)     (key)               |
| MIRROR WORLD                               |
+--------------------------------------------+
```

- **Win:** collect the key, then reach the door.
- **Lose:** either character dies.

---

## 1. Module Ownership & Subsystem Architecture

Even solo, each subsystem owns one namespace and folder, connected only by shared interfaces, the `GameEventBus` and the reusable GL layer in `src/Rendering/`, so every ticket stays small and every rule testable.

```
MirrorLogic      World               Player              Game (UI)
reflection       JSON levels,        input DI,           menu, HUD,
matrix,          tilemap,            platformer          state machine
WorldPair sync   collision, hazards, physics,
                 key & door          characters
---------------------------------------------------------------
Shared Contracts, Event Bus & GL Rendering Layer (src/Core/ + src/Rendering/)
IGameSystem | IInputProvider | IHazard | IInteractable | IGameScreen | ShaderProgram | SpriteBatch
```

| Module | Namespace | Planned classes | Skills practiced |
|---|---|---|---|
| **Core** | `MirrorWorld.Core` | `GameContext`, `GameEventBus`, `GameWindow`, `GameConstants`, `InputCommand`, `IGameSystem` | Interfaces & contracts, Observer (event bus), IDisposable lifecycle |
| **Rendering** | `MirrorWorld.Rendering` | `ShaderProgram`, `Texture2D`, `SpriteBatch`, `Camera2D`, `ViewportLayout`, `FrameBuffer`, `MirrorShimmerPass` | OpenGL pipeline: batching, orthographic matrices, viewports, FBO shader pass |
| **MirrorLogic** | `MirrorWorld.MirrorLogic` | `MirrorTransform`, `WorldPair`, `MirrorSyncService` | Linear algebra (reflection matrix), Facade, pure functions |
| **World** | `MirrorWorld.World` | `LevelLoader`, `LevelData`, `TileMap`, `CollisionResolver`, `Spike`, `Key`, `Door`, `WinCondition` | Data-driven JSON DTOs, AABB collision, Polymorphism (`IHazard`, `IInteractable`) |
| **Player** | `MirrorWorld.Player` | `Character`, `PhysicsSystem`, `IInputProvider`, `KeyboardInputProvider`, `CharacterRenderer` | Input abstraction (DI), platformer physics, logic vs. view separation |
| **Game** | `MirrorWorld.Game` | `GameStateManager`, `MenuScreen`, `PlayingScreen`, `HudRenderer`, `BitmapFont` | State pattern (`IGameScreen`), UI rendering |

> **Architectural proof:** physics, mirror maths, collision and win/lose rules run headlessly on 2D `(X, Y)` coordinates, while OpenGL is isolated in `src/Rendering/`. xUnit tests exercise every rule in milliseconds **without opening a window**.

---

## 2. Clean C# Solution Structure & SOLID Contracts

### 2.1 Repository Layout (`MirrorWorld.sln`)

```
MirrorWorld/
├── assets/
│   ├── levels/            # level1_first_reflection.json ... level4_crossroads.json
│   ├── shaders/           # sprite.vert/frag, mirror.frag
│   └── sprites/           # tileset.png, characters.png, items.png, font.png
├── src/
│   ├── Program.cs         # Wires systems, render passes & disposal order
│   ├── Core/              # Interfaces/, Events/ (GameEventBus, KeyCollectedEvent,
│   │                      #   LevelOutcomeEvent), GameContext, GameWindow
│   ├── Rendering/         # ShaderProgram, Texture2D, SpriteBatch, Camera2D,
│   │                      #   ViewportLayout, FrameBuffer, MirrorShimmerPass
│   ├── MirrorLogic/       # MirrorTransform, WorldPair, MirrorSyncService
│   ├── World/             # LevelLoader, TileMap, Collision/, Hazards/, Interactables/
│   ├── Player/            # Character, PhysicsSystem, Input/, Rendering/
│   └── Game/              # Screens/, UI/ (HudRenderer, BitmapFont)
└── tests/
    └── MirrorWorld.Tests/ # xUnit · target 25+ tests, no GL context needed
```

### 2.2 Enforced Clean Code Rules

1. **Single Responsibility & Headless Logic.** Never couple game maths to GL calls. Input goes through `IInputProvider`; all OpenGL lives in `Rendering/` so every rule is unit-testable.
2. **Strict <200 Lines & Newspaper Layout.** Every `.cs` file in `src/` stays under 200 lines: public API first, private helpers below, self-documenting names, no noisy comments.
3. **Polymorphism over Switch Chains.** `IHazard`, `IInteractable` and `IGameScreen` replace giant switch chains with small focused classes.
4. **EventBus & Safe GPU Lifecycle.** `KeyCollectedEvent` and `LevelOutcomeEvent` decouple modules. Every VBO/VAO/FBO/texture/shader is `IDisposable`, freed before the window closes.

### 2.3 Core C# Interface Contracts (`src/Core/Interfaces/`)

```csharp
public interface IGameSystem { void Init(GameContext c); void Update(float dt, GameContext c); void Render(GameContext c); }
public interface IInputProvider { InputCommand Poll(); }            // MoveX, Jump, Restart
public interface IHazard { WorldId World { get; } bool Kills(Box2 characterBounds); }
public interface IInteractable
{
    string Id { get; }
    WorldId World { get; }
    bool IsTriggeredBy(Box2 characterBounds);
    void OnTrigger(GameContext context, GameEventBus eventBus);    // Key publishes KeyCollectedEvent
}
public interface IGameScreen { IGameScreen Update(float dt, GameContext c); void Render(GameContext c); }
```

### 2.4 The Mirror Maths (`src/MirrorLogic/MirrorTransform.cs`)

```csharp
// Reflect about the mirror line (y = 0 in world space): one matrix draws AND simulates the mirror world
Matrix4 reflect = Matrix4.CreateScale(1f, -1f, 1f);
Vector2 mirrorPosition = (new Vector4(realPosition.X, realPosition.Y, 0f, 1f) * reflect).Xy;
float mirrorGravity = -realGravity;          // physics flips with the world; horizontal input stays the same
```

> **Graphics-course tie-in:** the mirror is literally a transformation matrix. Vertical flip = `scale(1, -1)`, applied in homogeneous coordinates. The same matrix draws the mirror sprites and maps positions, so the report can show that the *maths* and the *game* are one thing.

---

## 3. Solo Git Workflow & Self-Review Playbook

Direct pushes to `main` are disabled in GitHub settings. Every ticket gets an isolated branch, a green `dotnet build && dotnet test`, and a Pull Request that you review as if it were a teammate's. With only two weeks, keep PRs small: one ticket, one PR, merged the same day.

### 3.1 Branch Naming & Commit Convention

```
Pattern: <type>/<scope>-<feature>

feat/mirror-world-pair
feat/gl-stacked-viewports
feat/world-key-door
feat/player-physics
test/core-unit-tests
fix/collision-ceiling-stick
```

Conventional commit messages:

```
feat(mirror): add MirrorTransform
feat(gl): batch quads in SpriteBatch
feat(world): add Key and Door
test(player): cover inverted gravity
fix(collision): stop sticking on ceilings
```

### 3.2 The Daily 5-Step Git Routine

```bash
# Step 1: Switch to main & pull latest
git checkout main
git pull origin main

# Step 2: Create a fresh branch for ONE ticket
git checkout -b feat/mirror-world-pair

# Step 3: Verify build & tests pass locally
dotnet build && dotnet test

# Step 4: Stage & commit with a clean message
git add .
git commit -m "feat(mirror): add WorldPair"

# Step 5: Push & open a Pull Request on GitHub
git push -u origin feat/mirror-world-pair
```

### 3.3 Solo Review Routine (10 minutes per PR)

| Step | What to do | What you are checking |
|---|---|---|
| **1. Read the diff** | Review the PR line by line in GitHub, not the IDE, after a short break. | Naming, magic numbers, files near 200 lines. |
| **2. Run the checklist** | Tick every item in section 3.4 below. | Build, tests, boundaries, style. |
| **3. Playtest in both worlds** | Launch the game and exercise the ticket, including one failure case. | Mirror bugs hide in the world you are not watching. |
| **4. Squash & merge** | Squash-merge with a conventional commit title, then delete the branch. | Keeps history readable for the report. |

### 3.4 Pull Request Review Checklist (`.github/pull_request_template.md`)

| Check | What must be true |
|---|---|
| **1. Builds & tests cleanly** | `dotnet build` (0 warnings, 0 errors) and all xUnit tests pass. |
| **2. Strict <200-line limit & Newspaper Layout** | Every file in `src/` is under 200 lines; public methods sit above private helpers. |
| **3. Self-documenting names & no magic numbers** | Named constants (gravity, jump speed, tile size), clear identifiers, single-purpose methods. |
| **4. Strict module boundaries** | Logic modules never call OpenGL; cross-module links use Core contracts and `GameEventBus`. |
| **5. GPU resources disposed** | Every new buffer, texture, shader or FBO has an owner and is disposed before the window closes. |

> **Test strategy:** target **25+ xUnit tests** by Day 7 and keep them green: reflection maths (double flip returns identity), inverted gravity, jump and landing, collision sliding, `WorldPair` sync, hazard kill zones, key-then-door win rule and level JSON parsing. None of them need an OpenGL context.

---

## 4. 2-Week Sprint Plan (Tickets #1 – #16)

### Week 1: Foundation, Platformer Physics & Mirror Core

**Sprint 1 goal:** a playable prototype by Day 7: one keyboard drives two stacked worlds, the mirror self falls and jumps with inverted gravity, and a level loads from JSON.

| ID | Branch | Deliverable & C# classes | Clean code focus |
|---|---|---|---|
| ☐ **#1** | `feat/core-skeleton` | `GameWindow` (OpenTK), `GameContext`, `GameEventBus`, `IGameSystem`, `GameConstants`. | Interfaces & contracts |
| ☐ **#2** | `feat/gl-shader-sprites` | `ShaderProgram`, `Texture2D` (StbImageSharp), `SpriteBatch` (batched quads), *sprite.vert/frag*. | Resource wrapping & IDisposable |
| ☐ **#3** | `feat/gl-stacked-viewports` | `Camera2D` orthographic matrix + `ViewportLayout`: two `glViewport`s (Real on top, Mirror below) and the mirror-line quad. | Multi-viewport rendering |
| ☐ **#4** | `feat/input-provider` | `IInputProvider`, `KeyboardInputProvider`, `InputCommand` struct (move, jump, restart). | Dependency injection & SRP |
| ☐ **#5** | `feat/world-level-loader` | `TileMap`, `LevelLoader`, `LevelData` DTOs: real layer, mirror layer, spawns, key, door. | Data-driven JSON DTOs |
| ☐ **#6** | `feat/player-physics` | `Character` + `PhysicsSystem` (gravity, jump, ground check, gravity-sign parameter) + `CollisionResolver` (axis-separated AABB). | Logic vs. view separation |
| ☐ **#7** | `feat/mirror-world-pair` | `MirrorTransform` (`scale(1,-1)`), `WorldPair`, `MirrorSyncService`: one command drives both characters, mirror gravity inverted. | Pure functions & Facade |
| ☐ **#8** | `test/core-unit-tests` | xUnit: double flip = identity, gravity sign, collision sliding, `WorldPair` sync, level JSON parsing. | Automated unit testing |

### Week 2: Puzzle Rules, Visuals, Levels & Delivery

**Sprint 2 goal:** the puzzle works end to end (hazards, cross-world key and door, win/lose), the mirror looks great, four levels are playable, and the report is ready.

| ID | Branch | Deliverable & C# classes | Clean code focus |
|---|---|---|---|
| ☐ **#9** | `feat/world-hazards` | `IHazard`, `Spike` (declared per world in JSON); touching one publishes `LevelOutcomeEvent` (lose). | Polymorphism (`IHazard`) |
| ☐ **#10** | `feat/world-key-door` | `Key`, `Door` : `IInteractable`, `WinCondition`; a key in the Mirror World unlocks the door in the Real World. | Loose coupling via event bus |
| ☐ **#11** | `feat/gl-mirror-shimmer` | `FrameBuffer` + *mirror.frag*: tint and wave shimmer on the Mirror World, glow on the mirror line. | Multi-pass shader pipeline |
| ☐ **#12** | `feat/game-states-hud` | `GameStateManager` (Menu → Playing → Outcome, [R] restart), `BitmapFont` + `HudRenderer` ("Level 3 - key 0/1"). | High-level state machine |
| ☐ **#13** | `feat/levels-1-to-4` | Author *level1–level4.json* (see section 5); world-only platforms via separate per-world layers. | Content pipeline |
| ☐ **#14** | `feat/sprites-art-pass` | Textured tiles, key, door and character sprites; the mirror sprite is drawn with the same reflection matrix. | Data-driven rendering |
| ☐ **#15** | `docs/readme-report` | *README.md* (build/run) + report section: reflection maths, architecture diagram, screenshots. | Communicating the design |
| ☐ **#16** | `fix/final-polish` | Bug bash, balance pass, verify all GL resources disposed before window close, tag *v1.0*, 60-second demo clip. | Release hygiene |

---

## 5. Pacing, Levels & Scope Control

### 5.1 Fourteen-Day Pacing

| Day | Tickets | What must be true at the end |
|---|---|---|
| **Days 1–2** | #1 – #2 | Window opens and a textured quad is drawn through your own shader and batch. |
| **Day 3** | #3 – #4 | Two stacked viewports with the mirror line, plus keyboard input. |
| **Day 4** | #5 | A level loads from JSON and renders in both worlds. |
| **Days 5–6** | #6 | Character walks, jumps and collides (real world first, then gravity-sign parameter). |
| **Day 7** | #7 – #8 | **MILESTONE A:** playable mirror prototype, tests green, first screenshot for the report. |
| **Day 8** | #9 – #10 | Spikes, key, door, win and lose. |
| **Days 9–10** | #11 – #12 | Mirror shimmer shader, then menu, HUD and restart flow. |
| **Days 11–12** | #13 – #14 | Four levels authored and the art pass applied. |
| **Day 13** | #15 | README and report written while the game is stable. |
| **Day 14** | #16 | **MILESTONE B:** final polish, tag v1.0, demo clip recorded. |

### 5.2 Level Progression (one new idea per level)

| Lvl | Name | New idea | What it teaches |
|---|---|---|---|
| 1 | **First Reflection** | Gravity flip | No hazards. Reach the door and learn that a jump sends the mirror self the other way. |
| 2 | **Different Ground** | World-only hazards | Spikes exist in one world only, so a safe jump above can land on spikes below. |
| 3 | **Key Below** | Cross-world key | The key is only in the Mirror World and the door is in the Real World (matches your reference image). |
| 4 | **Crossroads** | World-only platforms | Platforms that exist in one world only; both characters must land together. Finale. |

### 5.3 Scope Safety Net

Cut in this order if time runs short: **#14** art pass (coloured quads are fine) → **level 4** → **#12** HUD font (show text in the window title) → **#15** report polish.

Never cut: stacked viewports (#3), physics (#6), mirror sync (#7), key and door (#10) and the mirror shimmer shader (#11); they are the heart of the idea and your graphics showcase.

**Stretch goals (only after #16):** particle bursts on death, a pushable box (as in your reference image), sound effects, and a delay rule where the mirror self repeats your moves one second late.

> **Day 7 & Day 14 checkpoints (30 minutes each):** review open PRs, merge to `main`, verify `dotnet build && dotnet test` (0 warnings, all passing) and files under 200 lines, then playtest the full build and write down the three most confusing moments to fix next.
