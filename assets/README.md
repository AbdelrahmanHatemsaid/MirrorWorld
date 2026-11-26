# Mirror World Assets Specification

This folder holds all data-driven visual, shader, and level assets for **Mirror World**.

## 1. Sprite Sheets (`assets/sprites/`)
All sprite sheets use crisp `GL_NEAREST` pixel-art with 32-bit RGBA transparency.

### `tileset.png` (`256 × 128` — 8 cols × 4 rows of `32 × 32` tiles)
| Row | Col 0 | Col 1 | Col 2 | Col 3 | Col 4 | Col 5 | Col 6 | Col 7 |
|---|---|---|---|---|---|---|---|---|
| **0 (Real)** | Solid stone | Ground top | Ground left corner | Ground right corner | Thin platform | Real-only platform (amber) | BG pillar | Mirror line divider |
| **1 (Mirror)** | Mirror solid stone | Mirror ground edge | Mirror left corner | Mirror right corner | Mirror thin platform | Mirror-only platform (cyan) | Mirror BG pillar | Shimmer sparkle |
| **2 (Hazards)** | Spikes (up) | Spikes (down) | Spikes (left) | Spikes (right) | Hazard trim | — | — | — |
| **3 (FX / UI)** | Solid white 32×32 | Radial glow orb | Particle puff | 4-pt star sparkle | — | — | — | — |

### `characters.png` (`192 × 64` — 6 cols × 2 rows of `32 × 32` sprites)
| Row | Col 0 | Col 1 | Col 2 | Col 3 | Col 4 | Col 5 |
|---|---|---|---|---|---|---|
| **0 (Real — Peach)** | Idle 0 | Idle 1 | Walk 0 | Walk 1 | Jump | Fall |
| **1 (Mirror — Cyan)** | Idle 0 | Idle 1 | Walk 0 | Walk 1 | Jump | Fall |

> Note: Row 1 is authored upright so `MirrorTransform` (`scale(1, -1)`) reflects the Mirror character directly during rendering.

### `items.png` (`128 × 64` — 4 cols × 2 rows of `32 × 32` sprites)
- **Row 0:** `(0,0)` Key frame 0 · `(1,0)` Key frame 1 · `(2,0)` Door (locked) · `(3,0)` Door (unlocked)
- **Row 1:** `(0,1)` Real spawn beacon · `(1,1)` Mirror spawn beacon · `(2,1)` Pushable crate · `(3,1)` Mini HUD key icon

### `font.png` (`256 × 96` — 16 cols × 6 rows of `16 × 16` cells)
- Covers ASCII `32` (`' '`) through `127` in row-major order (`col = (code - 32) % 16`, `row = (code - 32) / 16`).
- Pure white glyphs on transparent background for arbitrary `SpriteBatch` color tinting.

---

## 2. GLSL 330 Core Shaders (`assets/shaders/`)
- `sprite.vert` / `sprite.frag`: batched 2D sprite shader (`aPos`, `aTexCoord`, `aColor`, `uProjection`, `uTransform`, `uTexture`).
- `mirror.vert` / `mirror.frag`: fullscreen FBO pass for Mirror World shimmer (`uSceneTexture`, `uTime`, `uMirrorTint`, `uShimmerStrength`, `uGlowHeight`).

---

## 3. Level JSON Schema (`assets/levels/`)
Levels are `24 × 9` tiles per world (`tileSize = 32`, world width `768`, half-height `288`):
- **Real World (`realLayer`):** `row 0` is the top sky; `row 8` is the base floor just above the mirror line.
- **Mirror World (`mirrorLayer`):** `row 0` is the mirror floor just below the mirror line (where inverted gravity pulls upward); `row 8` is the lower edge away from the mirror line.
- Tile codes in `realLayer` / `mirrorLayer`:
  - `0`: empty
  - `1`: ground surface / top tile
  - `2`: solid interior stone
  - `3`: floating thin platform
  - `4`: world-only platform (amber in Real, cyan in Mirror)
- `spikes`: list of `{ "world": "Real" | "Mirror", "col": int, "row": int, "direction": "Up" | "Down" | "Left" | "Right" }`
- `key` / `door`: `{ "id": string, "world": "Real" | "Mirror", "col": int, "row": int }`
