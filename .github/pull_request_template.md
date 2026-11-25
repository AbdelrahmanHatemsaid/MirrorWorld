## Ticket / Summary
<!-- Reference ticket # and brief description of changes -->

## Review Checklist
- [ ] **1. Builds & tests cleanly** — `dotnet build` (0 warnings, 0 errors) and all xUnit tests pass.
- [ ] **2. Strict <200-line limit & Newspaper Layout** — Every file in `src/` is under 200 lines; public methods sit above private helpers.
- [ ] **3. Self-documenting names & no magic numbers** — Named constants (gravity, jump speed, tile size), clear identifiers, single-purpose methods.
- [ ] **4. Strict module boundaries** — Logic modules never call OpenGL; cross-module links use `MirrorWorld.Core` contracts and `GameEventBus`.
- [ ] **5. GPU resources disposed** — Every new buffer, texture, shader, or FBO has an owner and is disposed before the window closes.
- [ ] **6. Playtested in both worlds** — Verified in both Real World and Mirror World, including at least one failure case.
