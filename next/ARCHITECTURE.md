# STROOP Next — architecture (iteration 1)

A ground-up rewrite, built depth-first: **Mario tab + Map tab only**, on a core that is
meant to be refined many times. This document is the *specification* of that core.
Everything here is expected to change; the point is to make each concept explicit so
that it can be argued about and refined.

```
next/
  src/StroopNext.Core        net10.0, no UI, no OS calls      ← the specification lives here
  src/StroopNext.Platform    net10.0, emulator discovery + process memory (Windows, Linux, macOS)
  src/StroopNext.Rendering   net10.0, Skia 2D map + OpenGL 3.3 3D map, offscreen tools
  src/StroopNext.Desktop     net10.0, Avalonia host: Windows, Linux, macOS
  src/StroopNext.App         net10.0-windows, the former Windows Forms host (to be removed)
  tests/StroopNext.Core.Tests xunit, synthetic RAM + real savestates (if present)
```

Dependencies point one way:
- `Desktop` and `App` → `Platform`, `Rendering` → `Core`.
- Core never knows about processes, UI threads, UI toolkits or graphics APIs.
- Rendering never knows about a UI toolkit.

**Cross-platform rules:**
- **Process memory** (`Platform`): `IProcessMemory` has three implementations.
  - Windows: `ReadProcessMemory`, `VirtualQueryEx`, dbghelp.
  - Linux: `/proc/<pid>/maps` and `/proc/<pid>/mem`.
  - macOS: `task_for_pid` and `mach_vm_*`.

  Locating the RAM base is OS-independent: symbol, then profile (Windows only), then a
  signature scan.
- **2D map:** Skia draws on the host's canvas.
  - Under Avalonia that canvas may be GPU-backed. The renderer then keeps its background
    cache on the GPU (`SkiaMapRenderer.GpuContext`) and draws the textured level itself.
  - Under Windows Forms (CPU canvas), the textured level is rendered by our own offscreen
    OpenGL context (`GlTopDown`).
- **2D drawing runs on Avalonia's render thread.** Building happens on the UI thread, so
  every scene is built and drawn under the surface's lock (`SkiaSurface.SyncRoot`).
- **3D map:** the OpenGL renderers use OpenTK's bindings. Under Avalonia they are loaded
  from the `OpenGlControlBase` context (`GlSurface`), which is WGL on Windows (not ANGLE: the
  shaders need desktop GL 3.3), GLX/EGL on Linux and OpenGL on macOS.

## 1. Memory

| Concept | Responsibility |
|---|---|
| `RamImage` | Read-only view of one RDRAM copy in **native word-swapped layout** (how Mupen and `.st` files store it). All reads go through it: `U8/S8` use `addr ^ 3`, `U16/S16` use `addr ^ 2`, 32-bit reads are direct. Out-of-range reads return 0; `IsValid(addr, size)` / `IsPointer(addr)` exist so models never dereference garbage. |
| `IRamSource` | Something that can fill a buffer with RDRAM and accept writes: a live emulator process, a savestate file, a test fixture. That is the *only* abstraction the platform layer implements. |
| `RamPatch` | A batch of writes expressed in N64 terms (`WriteF32(addr, v)`, `WriteS16`, …). It is translated into the swapped layout once, and applied atomically by the session. Commands (Mario tab buttons, map drag/teleport) *produce* patches; they never write directly. |
| `N64` | Address helpers (`0x80xxxxxx` ⇄ RAM offset). |

Design choice: the snapshot keeps the swapped layout instead of normalising to big-endian
— no per-frame conversion of 8 MB, and `.st` files and process memory are byte-identical.

`RdramSignature` recognises the start of RDRAM (the four exception vectors). The Windows
platform layer uses it to *verify* every RAM-base candidate — `CORE_RDRAM` symbol, then
per-build profile, then a generic memory scan that prefers the copy that is changing — so a
wrong `ramStart` can never be attached silently.

## 2. Frames and the session

* `GameFrame` = one consistent RAM snapshot + its detected `GameVersion` + resolved
  `Sm64Layout` + lazily-built models (`Mario`, `Objects`, `Surfaces`). **Every** value shown
  anywhere is a pure function of a frame. No component reads memory "live".
* `GameFrame.Previous` is the last frame whose **global timer differed** (the previous
  *game* frame, not the previous poll). This gives every variable a meaningful Δ even when
  STROOP polls faster than the game runs, or the emulator is paused.
* `FramePool` recycles 8 MB buffers with reference counting (no LOH churn at 60 Hz).
  A frame keeps its `Previous` alive while it is held.
* `GameSession` owns the source and a **background polling thread**; the UI *pulls* the
  newest frame on its own render tick (`AcquireLatest`). Writes (`Apply(RamPatch)`) are
  serialised with reads and trigger an immediate re-poll.
* Polling is **timer-driven**:
  - each poll first reads the 4-byte global timer (`IRamSource.TryReadRange`);
  - the 8 MB copy happens only when the game advanced, after a write, or at `IdleRefreshRate`
    while paused;
  - an idle refresh that finds identical RAM publishes nothing, so views never redraw an
    unchanged game.
* A **generation counter** makes a poll that started before an attach or detach drop its
  frame, so a dead source can never be published again.
* `SessionDiagnostics` exposes the invariants that leak tests and `--bench` check:
  - pooled and outstanding buffers;
  - full reads;
  - skipped polls;
  - unchanged refreshes.
* `SampleTrack<T>`: per-game-frame samples (e.g. Mario position) recorded on the polling
  thread, so history is complete even when the UI drops frames. The map path layer uses it.

## 3. Game knowledge

* `GameVersion` (US/JP/SH/EU), detected from the u32 at `0x802F0000`. Unknown ROM → `null`,
  not "US".
* `Versioned<T>`: an address known per version. A missing entry is *explicitly* unknown;
  variables depending on it display `n/a` instead of silently reading address 0.
* `Sm64Addresses` (versioned table) → `Sm64Layout` (resolved for one version).
* Models are immutable readers built from a frame: `MarioState`, `ObjectList` (process
  group traversal, slot index from pool address), `SurfaceTable` (cached across frames,
  keyed by list pointer + count + content hash), `ActionTable` (names from the existing
  `MarioActions.xml`).

## 4. Variables (the shared vocabulary)

`Variable` = stable `Id` (`mario.x`) + label + group + `ValueKind` + a reader
`GameFrame → T?` + an optional writer `(GameFrame, T, RamPatch)` + a `ValueFormat<T>` that
formats *and parses*. Variables are declared in code, in catalogs (`MarioVariables`):
strongly typed, refactorable, and computed variables (de facto speed, fall height, …) are
just readers composing models — there is no separate "special variable" mechanism.

Built-in capabilities every UI gets for free: formatting, parsing/editing, Δ vs previous
game frame (`TryReadNumber`), `n/a` handling, id-based persistence.

## 5. Modules (tabs)

```csharp
interface IStroopModule { string Id; string Title; ModuleView CreateView(ModuleContext ctx); }
abstract class ModuleView : UserControl { abstract void OnFrame(GameFrame frame); }
```
`ModuleContext` gives a module everything it may depend on — the session, the shared
`Selection`, its own `SettingsScope` — and nothing global. Only the visible module receives
frames. Modules are registered explicitly in `Program.cs` (no reflection).

`Selection` is the first cross-module service (map click → selected object); it is how
future tabs (Object, Triangle) will cooperate with the map.

## 6. Settings

`Setting<T>(key, label, default)` declared by whoever owns the behaviour (a map layer,
a module); values live in a JSON `SettingsStore` under a scope (`map.layers.floors.alpha`).
UIs are generated from the declarations (`SettingsEditor`), so models never build controls.

## 7. Map

```
GameFrame ──► IMapLayer.Build(frame, MapView, MapScene) ──► MapScene (primitives) ──► SkiaMapRenderer
                                                                 └──► hit-testing (hover / click)
```
* `MapView`: centre, scale (px/unit), rotation, follow mode; pure world ⇄ screen math.
* `MapScene`: retained list of primitives in *world* coordinates (polygon, polyline, circle,
  arrow, marker, label), each with an optional `Hit` payload (`TriangleRef`, `ObjectRef`, …).
* Layers are pure and UI-free: they declare `Setting`s and emit primitives. Rendering, hit
  testing, tooltips and settings UI are generic.
* Iteration-1 layers: grid, floors, walls (with Mario's 50-unit wall hitbox), ceilings,
  objects (hitbox circles + facing), Mario (hitbox, facing / moving / intended yaw), path
  (per-game-frame history), camera.
* `MapScene` has two parts.
  - **`Background`**: level geometry and the grid. Every input of the background is declared
    with `KeyBackground`: geometry version, settings, Mario's Y if a filter or shading uses it.
  - **`Items`**: everything live.

  The renderer keeps one opaque raster of the background, larger than the view by
  `MapView.CullMarginPx`. It reuses that raster with a pixel offset while the key, zoom,
  rotation and size are unchanged. Correctness rule: a layer that forgets to declare an input
  of a background primitive shows stale content. When in doubt, add to `Items`.
* The App draws scenes with plain SkiaSharp into a GDI bitmap (`SkiaCanvas`), deliberately
  avoiding `SkiaSharp.Views.WindowsForms` (only .NET Framework builds, drags in OpenTK 3).
  The same renderer could be hosted by Avalonia (which renders with Skia) later.

## 8. Map 3D

```
GameFrame ──► IMapLayer3D.Build3D(Map3DContext) ──► Scene3D ──► MeshBuffers (tessellation) ──► GlMapRenderer (OpenGL 3.3)
                                                       └──► Picking (rays: hover, select, teleport)
```
* The same layer classes implement `IMapLayer3D` (partial classes in `StandardLayers3D.cs`)
  with the same settings: 2D and 3D are two projections of one model.
* `Camera3D`: orbit (SM64 yaw convention, north-up by default like the 2D map) or an
  explicit eye (game camera). `DetachFromEye` converts the game view into the equivalent
  orbit. Projection and ray math use System.Numerics (depth range 0..1).
* `Scene3D` primitives are semantic: `Triangle3D`, `Cylinder3D`, `Line3D`, `Arrow3D`,
  `Point3D`.
  - **Background:** per-layer **sections** that persist across frames.
    `BeginBackground(owner, key)` returns false when the key is unchanged, and the previous
    primitives are reused with zero allocation. `BackgroundVersion` tells the renderer when
    to re-upload.
  - **Items:** live, rebuilt every frame.
* `MeshBuffers` tessellates into four reusable vertex streams: opaque, transparent, lines and
  points. Triangle outlines come from barycentric coordinates in the shader.
* `GlMapRenderer` passes, in order:
  1. opaque, with depth write;
  2. lines;
  3. translucent, without depth write;
  4. points;
  5. X-ray: live items drawn with `GL_GREATER` depth at low alpha;
  6. hover.

  It also applies distance fog. It only needs a current context: the same code serves the
  `GLControl` view and the offscreen tools (`OffscreenGl`, never shown or focused).

## 9. The game scene from RAM (`StroopNext.Core/Graphics`)

Ported from the `feature/map-3d-moteur` branch and kept UI-free.

**Data flow:**
`GfxMemory` (segmented reads on the snapshot, no copy) → `GameSceneBuilder` (graph nodes,
objects, extras) → `F3DInterpreter` → `GfxMeshBuilder` → `GameScene` (level mesh, instances,
textures, UV animations, sky) → `GameVisualsLayer` puts it in `Scene3D.GameVisuals` →
`GameSceneRenderer` (App) draws it with OpenGL.

**Invariants:**
- The level is rebuilt only when its key changes.
- Object meshes are cached by (display list, layer, incoming `F3DState`). The state is
  compared structurally, never by reference.
- An object that derives a new state (fading env alpha) has its layer states restored after
  it is drawn. Otherwise every later object would miss the cache on every frame.
- `GfxInstance` flags:
  - `IsMario` means "emitted while drawing the real Mario", so it includes his held object
    and excludes mirror Mario;
  - `Level` (paintings) follows the level option and is repeated in the PU copies.
- Transient meshes (rippling painting, carpet) are streamed every frame and never cached.
- Garbage RAM is bounded everywhere:
  - 16k graph nodes for the level, and 4096 per object;
  - 200k F3D commands per top-level run;
  - 4096 MIPS steps;
  - textures up to 256×256, and every count is clamped.
- MIPS leaf calls write to a private overlay, never to RAM.

**GPU side:**
- The level is one VBO.
- Object meshes live in a compacting arena; per-frame meshes go to a stream VBO.
- Per-instance mat4 is at attribute locations 6–9. With GL 3.3 there are no base-instance
  draws, so the offset is set with `VertexAttribPointer`.
- Draws are sorted by phase, then by texture.

## Known shortcomings (to revisit in iteration 2)

* Translucent triangles are not sorted (depth writes are off for them). Overlapping wall
  prisms can blend in a different order from one angle to another.
* No text labels in 3D (names are shown in hover tooltips).
* Variables are code-only; a user "watch list" format referencing variable ids is not done.
* Object names: behaviour → name mapping is read from the legacy `ObjectAssociations.xml`.
