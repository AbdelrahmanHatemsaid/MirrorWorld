# Mirror World

A **2D dual-world OpenGL platformer** built in **C# (.NET 8)** and **OpenTK 4** (OpenGL 3.3 Core).

One keyboard drives **two characters** stacked around a mirror line:
- **Real World (top):** normal gravity and layout.
- **Mirror World (bottom):** reflected via `scale(1, -1)` with inverted gravity and a different level layout.

Collect the key and reach the door by satisfying **both worlds at once**.

## Tech Stack
- **Runtime:** .NET 8 (`C#`)
- **Graphics:** OpenTK 4 (OpenGL 3.3 Core, GLSL 330, FBO post-processing)
- **Testing:** xUnit (headless unit tests with no OpenGL context required)

## Repository Layout
```text
MirrorWorld/
├── assets/
│   ├── levels/              # JSON levels (level1_first_reflection.json ... level4_crossroads.json)
│   ├── shaders/             # GLSL shaders (sprite.vert/frag, mirror.frag)
│   └── sprites/             # Textures and bitmap font
├── project plan/            # Engineering specification (MirrorWorld_Engineering_Plan.md/.pdf)
├── src/
│   ├── Program.cs           # Entry point and system wiring
│   ├── Core/                # Shared interfaces, GameContext, GameEventBus, GameWindow
│   ├── Rendering/           # OpenGL abstractions, SpriteBatch, Camera2D, ViewportLayout, FBO
│   ├── MirrorLogic/         # MirrorTransform, WorldPair, MirrorSyncService
│   ├── World/               # LevelLoader, TileMap, Collision, Hazards, Interactables
│   ├── Player/              # Character, PhysicsSystem, IInputProvider, CharacterRenderer
│   └── Game/                # GameStateManager, Screens, HUD & BitmapFont
└── tests/
    └── MirrorWorld.Tests/   # Headless xUnit test suite
```

## Build & Test
```bash
dotnet build
dotnet test
dotnet run --project src/MirrorWorld.csproj
```

## Engineering Workflow
- Direct pushes to `main` are disabled; every ticket uses a dedicated branch (`feat/...`, `test/...`, `fix/...`, `docs/...`) and a Pull Request.
- Every `.cs` file in `src/` stays under **200 lines** using Newspaper Layout (public API first, private helpers below).
- Core logic modules remain decoupled from OpenGL via `MirrorWorld.Core` interfaces and `GameEventBus`.
- See [MirrorWorld_Engineering_Plan.md](project%20plan/MirrorWorld_Engineering_Plan.md) for the full 2-week sprint plan (Tickets #1–#16).
