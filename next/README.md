# STROOP Next

A ground-up rewrite of STROOP, built **depth-first**: only the **Mario** and **Map** tabs, on a
core designed to be refined many times. See [ARCHITECTURE.md](ARCHITECTURE.md) for the
specification of that core. This README covers how to use it.

It lives next to the existing STROOP and shares nothing with it except two game tables
(`STROOP/Config/MarioActions.xml` and `ObjectAssociations.xml`), which are embedded directly.

## Build, run, test

STROOP Next runs on **Windows, Linux and macOS**. The app is `StroopNext.Desktop`, built on
Avalonia. The former Windows Forms host (`StroopNext.App`) still builds on Windows and shares
everything else; it will be removed once the Desktop app is validated.

```
cd next
dotnet build StroopNext.sln
dotnet test  tests/StroopNext.Core.Tests
dotnet run --project src/StroopNext.Desktop           # optional: a .st path as argument

# A self-contained build for another OS (win-x64, linux-x64, linux-arm64, osx-x64, osx-arm64):
dotnet publish src/StroopNext.Desktop -c Release -r linux-x64 --self-contained
```

* **Platforms:**
  - Windows: the 3D map uses desktop OpenGL through WGL.
  - Linux (X11 or XWayland): OpenGL through GLX, falling back to EGL.
  - macOS: OpenGL 4.1 core.

  Without OpenGL 3.3, the app still starts and only the 3D view reports that it is unavailable.
  The 2D map is drawn by Skia on Avalonia's GPU context, so the textured level is fast
  everywhere.
* **Attaching to an emulator** reads another process's memory, which each OS guards
  differently:
  - **Windows:** run STROOP Next with the same privileges as the emulator.
  - **Linux:** reading `/proc/<pid>/mem` needs ptrace rights. Most distributions only allow
    them for a debugger's own children. Use one of:
    - `sudo sysctl kernel.yama.ptrace_scope=0` (until reboot);
    - run STROOP Next with sudo;
    - `sudo setcap cap_sys_ptrace=eip` on the executable.
  - **macOS:** only root may read another process (`task_for_pid`), so run STROOP Next with
    `sudo`.

  Emulators are found by name: Mupen and Mupen64Plus front ends, Project64, RetroArch,
  simple64, RMG and ares (Windows builds running under Wine count too). On Linux and macOS
  the RAM is always found by scanning memory, which works with any emulator.
* **Building while the app is running:** `dotnet build -p:StroopOut=<dir>` writes the
  binaries to another folder. Use it when the running app holds the files in `bin\` open.

* **Connect ▾** lists running emulators: anything named `mupen*`/`project64*`, plus the known
  builds from legacy STROOP's `Config.xml`. The RAM base is found in this order:
  1. the `CORE_RDRAM` symbol (recent Mupen);
  2. the per-build address from the profile;
  3. an emulator-agnostic scan of memory for the RDRAM exception vectors.

  Every candidate is verified, so there is no `ramStart` to edit by hand.
* **Open savestate…** inspects a `.st` file without an emulator. Both formats are supported:
  gzip (older Mupen) and LZ4 (mupen64-rr-lua 1.5+). Legacy STROOP cannot read the LZ4 ones.
  Edits only change the in-memory copy.
* Settings are stored in `StroopNext/settings.json` in the user's application-data folder:
  `%APPDATA%` on Windows, `~/.config` on Linux and macOS. The `STROOPNEXT_SETTINGS`
  environment variable points to another file, e.g. for a portable setup.
* `StroopNext.Desktop --ui-snapshot file.st out.png [mario|map]` renders the whole window
  headless, with no window and no GPU, on any OS: a picture of the real UI for bug reports.
* `StroopNext.Desktop --gl-selftest file.st out.png` checks the real OpenGL path of the 3D
  map. It opens the app in a window placed off screen, never activated and absent from the
  taskbar, reads back one rendered 3D frame and exits (exit code 0 on success).
* The commands below work with both `StroopNext.Desktop` and `StroopNext` (Windows Forms).
* `StroopNext --attach mupen64` attaches to the first emulator whose process name (or id)
  matches, as soon as the window opens.
* `StroopNext --render-map file.st out.png [scale]` renders the map of a savestate to a PNG,
  with default settings. Use it for bug reports, or to check rendering without the UI.
* `StroopNext --bench file.st [seconds] [--without layer1,layer2]` runs the whole pipeline
  without UI while a simulated game advances at 30 fps. It reports:
  - time and allocations per stage;
  - garbage collections;
  - process CPU;
  - background-cache hits;
  - buffer-pool health.

  `--without` measures what each map layer costs.

## Performance and footprint

Measured with `--bench` (Release, castle hall savestate, map at 1200×900 following Mario, all
Mario variables formatted every frame):

| | first version | now |
|---|---|---|
| process CPU (incl. simulated game) | 38.6 % of a core | **14.5 %** |
| map render per frame | 6.7 ms | **2.1 ms** |
| RDRAM copies (8 MB) | ~62/s, every poll | **one per game frame** (30/s), ~10/s while paused |
| allocations per frame | 195 KB | 107 KB |
| GCs in 15 s | 14 gen0 + 1 gen2 | 4 gen0, 0 gen2 |
| pooled 8 MB buffers after detach | 5 (40 MB kept) | **0** |

How:
- **Timer-driven polling.** A 4-byte probe of the global timer runs at 120 Hz. The full 8 MB
  copy happens only when:
  - the game advanced;
  - STROOP wrote;
  - an idle refresh is due (10/s, so Lua or other tools are still seen while paused).

  An idle refresh that finds identical RAM publishes nothing, so a paused game costs almost
  nothing to draw.
- **Static geometry is shared.** Level triangles are parsed once per level, not once per frame.
  This covers their address index, their XZ points, their hover payloads and the map's draw
  order.
- **Background raster cache.** Level floors, walls and grid are drawn once into an opaque
  image 25 % larger than the view, then copied with a pixel offset. They are re-rendered only
  when their exact inputs change: geometry, settings, Mario's height if a filter or shading
  uses it, zoom, rotation or size. Object surfaces, highlights, objects, path and Mario stay
  live on top.
- **Cheaper drawing.** No antialiasing on large fills, culling for objects and path dots, and
  no double copy of the canvas.

Live with mupen64-rr-lua and the UI attached:
- the Map tab uses about 5–10 % of a core;
- over 2–3 minute soaks on each tab, private memory, handles, GDI and USER objects stay flat;
- `SessionDiagnostics` reports 0 outstanding buffers once detached.

## Leak checks

- **Frames.** They are reference-counted on pooled buffers. A test runs 2000 frames with
  random UI-like holding, pauses and re-attaches, and requires 0 outstanding buffers at the
  end.
- **WinForms and Skia.** Everything that is created is disposed: fonts, typefaces, timers,
  context menus, rebuilt menu items and the raster cache. Event subscriptions and sample
  tracks are removed when their view goes away.
- **Process handles.** They are closed on detach and on attach failure. A dead emulator is
  detected and detached automatically.

Mupen crashes observed during testing were all inside the emulator itself:
- `0xC0000409` in `ucrtbase.dll`, a fail-fast in Mupen;
- the same build also crashed before STROOP Next existed: 11 crashes in 30 days, including
  heap corruption.

STROOP Next only reads that process unless you edit a value, and it handled every one of
these crashes by detaching cleanly.

## What the tabs do

### Ported from legacy STROOP (Development branch), improved

**Mario tab** covers legacy STROOP's whole MarioData.xml (about 95 values, 10 groups;
Trajectory and Advanced start folded).
- Computed values are editable where legacy allowed it, and write the field behind them:
  - de facto speed, fall height, sliding yaw;
  - Mario's distance and angle to the HOLP;
  - the trajectory values (remaining or peak height, double-jump speed and height).
- **Objects** (held, ridden, stood on, interaction, used, visual/effective Mario):
  - shown as "slot 12 · Goomba";
  - typed as `slot 12`, `none` or an address;
  - right-click selects the object on the map.
- **Cap flags** are separate checkboxes. Also shown: cap, hitstun and squish timers, swim
  strength, blinking, spawn point, room, torso and first-person angles, and angle velocities.
- **Commands:**
  - 8-direction move pad, with separate XZ and Y steps;
  - sliding X/Z/speed/direction, which keep the moving yaw in sync as the game does;
  - Go to / Retrieve HOLP, with all / lateral / X / Y / Z variants, and a HOLP move pad;
  - hands-free (from MarioActions.xml's `handsfreeValue`);
  - toggle visibility. Unlike legacy, it remembers the real graph node, so it also works
    outside US.
  - **Set action…**: a searchable list of all 233 actions (also on the action rows).

**Map tab:**
- **Walls:**
  - drawn as the game sees them: only walls its checks can reach (30/60 above the feet on
    the ground, 30/150 in the air, ±5);
  - optionally cut at Mario's height or at a fixed height;
  - with push-direction arrows.
- **Triangle units:** from 1.5 px per unit, floors and ceilings show exactly the units the
  game's integer test (`find_floor`, s16 coordinates) puts in them. Unit gridlines fade in
  from 4 px per unit.
- **Mario arrows** can show the distance covered per frame. There is also a twirl yaw arrow.
- **Objects:**
  - effective hitboxes (+ Mario's radius);
  - home position and line;
  - moving yaw;
  - angle to Mario;
  - "only selected objects".
- **HOLP** layer, in 2D and 3D.
- **Collision is integer-exact** (`GameCollision`). Next positions follow the real velocity
  and snap to the floor the game finds; "Teleport onto the floor" uses the same floor.
- **View:**
  - custom orientation, with rotate ±45° and North;
  - zoom presets: whole level, 1 / 4 / 12 / 40 px per unit;
  - a live **Where** panel: PU, QPU, level / area, unit, cell, floor triangle and normal.

**Trackers** (legacy STROOP's tracker system), in the TRACKERS section of the map panel.
- Add any number of trackers.
- **Targets:** Mario, the HOLP, the camera, the selected objects, every object of a kind
  ("all Goombas"), one object, or a fixed point.
- **Kinds:**
  - marker, circle, cylinder, path;
  - facing / moving / intended / sliding / twirl / angle-to-Mario arrows (length 0 = the
    distance per frame);
  - line to Mario, home line;
  - facing divider, sector, angle range;
  - current unit, nearby floor units, nearby ceiling units (coloured by height, game-exact);
  - hitbox, effective hitbox, hurtbox, effective hurtbox.
- Each tracker has its own colour, opacity, sizes, visibility and order (bottom / normal /
  top). Trackers can be reordered and removed.
- Trackers are drawn in 2D and in 3D, saved with the settings, and can be exported and
  imported as JSON.
- Right-click an object on the map (2D or 3D) to track it, or a point to add a tracker there.
- `STROOPNEXT_TRACKERS=file.json` loads a tracker file into `--ui-snapshot`.

The Mario tab now covers every value of legacy STROOP's MarioData.xml, including the ones built
on the game's own trigonometry — **Speed Multiplier**, **IG DYaw** (facing yaw minus the yaw the
game's atan2 table rounds it to) and **Rotation Displacement** (how a spinning platform carries
Mario). These use `GameTrig` (angles truncated to a multiple of 16, atan2 read from the ROM's
`gArctanTable` in RAM) and the slipperiness of `TriangleInfo.xml`, so they match the game frame
by frame.

**Map tools:**
- **Tape measure** (right-click › Tape measure, in 2D and 3D): measure from a point to another,
  or **from Mario to a point, following Mario as he moves**. Shows the XZ and 3D distances,
  ΔX/ΔY/ΔZ (with the X and Z legs drawn) and the SM64 yaw from A to B. Points picked on the 2D
  map take the height of the floor the game finds there; in 3D, the exact point under the cursor.
- **Freehand drawing**: Shift + drag on the 2D map; right-click › Clear drawings.
- **Custom reference points**: right-click › "Add a tracker at this point" (any tracker kind,
  e.g. a marker or a circle, at a fixed point).
- **Side view** (3D view › Camera › Side view): orthographic, level with the horizon, following
  Mario. Look from Mario's side (his facing runs left → right: jump profiles), from behind him,
  along X or along Z. **Cut ±N** keeps only what lies within N units of the plane through Mario
  (legacy STROOP's near/far planes): a cross-section of the level along his path. Drag pans,
  the wheel zooms; hovering and right-click ignore what the cut hides.

**Movement analysis:**
- **Previous positions** (layer, off by default): with a ROM patched to record Mario's sub-frame
  positions at `0x807F4800`, every step of the last 1–128 frames, coloured by update step
  (initial, warp, platform, wall checks, objects, then each quarter step). Hover a point to see
  which step wrote it; frames whose movement stopped early are marked "n/4". Without the patch
  the layer stays empty.
- **Iwerlipses** (layer, off by default): the ellipses of positions Mario can reach in the air
  after 1…N quarter steps, whatever the stick does (`AirMovement`, legacy STROOP's air step:
  drag 0.35, ±1.5 forward, ±10 sideways, gravity after the move). "Lock" freezes them in place.

**Ghosts** (GHOSTS section of the map panel):
- **Load** legacy STROOP `.ghost` files (several at once), **Save** them back in the same format.
  Unlike legacy STROOP's writer, saved files keep the trailer its reader expects.
- **Record**: samples Mario every game frame, even with the map hidden, starting with the frame on
  screen (paused at frame N, Record then frame advance includes N). Loading a savestate and
  replaying a part overwrites those frames, so a segment can be re-recorded.
- **Stop saves at once**, no dialog: `ghost YYYY-MM-DD HH-MM-SS.ghost` in `Documents/StroopNext/Ghosts`
  (Change… picks another folder, Open folder shows it; files are never overwritten).
  **Play right after recording** (remembered): checked, the new ghost replays **from the next
  frame** (its timers are shifted to now), on the map and in the game; unchecked, it is only saved
  and added hidden to the list. The file always keeps the recorded timers.
- A ghost replays at its own global timers. **▶ Now** replays it from its start beginning at the
  next frame; **↺** puts it back at the timers it was recorded at (to race it from a savestate made
  before the recording). "Starts at timer" sets any start.
- Each ghost is drawn where it was at the current global timer (2D marker and hitbox, 3D
  cylinder and facing arrow), with its whole path; colour, visibility, path and **start timer**
  can be changed. Indices wrap like legacy (a frame at index 0xFFFFFFFF plays at the base timer).
- On the 3D map a ghost is drawn as **Mario's model in its own recorded pose** (animation and
  frame, green cap), independent of what Mario does now. Only Mario's current animation is ever
  in RAM, so STROOP keeps a copy of every animation it sees him play (`MarioAnimations`, also saved
  in `%APPDATA%/StroopNext/animations`): a ghost recorded by STROOP always has them. For a ghost file
  whose animations STROOP has not seen yet, it falls back to Mario's current pose until he plays them.
  The ghost is drawn by the scene code as a virtual object node placed in a memory overlay at
  0x80800000 (after the 8 MB of RDRAM).
- **In the game (Mupen)** — legacy STROOP's Ghost tab: "Show ghosts in the game" writes its ghost
  hack (`GhostHackUS/JP.hck`) into the running game, which then draws the **ticked** ghosts itself,
  animated, transparent if wanted, with their cap colour. STROOP keeps a 128-frame buffer per ghost
  filled (every 100 ms, even with the map hidden).
  - Needs the **Expansion Pak** (8 MB RDRAM in the emulator settings); US and JP.
  - **Appears right away** with mupen64-rr-lua (and other Mupen 0.5-based cores): Mupen runs
    compiled copies of the game's code and ignores code written from outside until a savestate load.
    STROOP finds Mupen's `invalid_code` table (one byte per 4 KB page: "recompile this page"), by
    symbol or by its signature in the emulator's memory, and marks the pages it patches: the hack
    runs on the next frame, without a save/load. Checked with the real "Show ghosts in the game"
    button: written, running and the recorded ghost drawn after plain frame advances. With other
    emulators the status says what to do: save state, load state, frame advance.
  - The status lists which ticked ghosts play now and when the others do: a ghost only appears at
    the timers it was recorded at (▶ Now replays it from the current frame).
  - The install has two steps: the code and its hook first; Mario's cap nodes switch to the hack's
    coloured-cap generator only once the game has run the new code (it flags Mario's object). Doing
    both at once, while the emulator runs, could make Mario's head disappear until a reload.
  - While the game draws its ghosts, the 3D map shows **those ghost nodes**, with their recorded
    animation, cap colour and transparency, and draws Mario's head through the same generator.
  - "Remove from the game" asks the game to remove its ghosts (let a frame run), then puts back
    what the game runs — hook, patched functions, Mario's cap nodes, model LOD and body switchers.
    Legacy STROOP left the cap nodes pointing at removed code, which could crash the game.
  - **Show and remove at any time, without a crash** (fixes over legacy STROOP's hack):
    - A **level change** empties the game's object list, but legacy's hack kept pointing at its old
      ghost nodes; removing a ghost then unlinked it from a list it was no longer in, writing into
      other objects. A guard (STROOP-generated MIPS, at the address the hook calls) now runs before
      the ghost loop each frame and forgets the ghosts no longer in the list; the loop creates them
      again in the new level. Checked in Mupen: entering the castle, the ghost node is orphaned, then
      forgotten, then recreated and linked one frame later.
    - The hack's own code and data **stay** (inert) in the Expansion Pak RAM after removal, which the
      game never uses: an emulator still running older compiled code lands on valid code, never on
      erased memory. Showing the ghosts again later reuses them.
    - What to put back is kept **in the game's RAM**: a STROOP started after the install (or another
      map window) can remove the hack. Data the game may rebuild (Mario's model) is only put back
      where it is still as the hack left it, and step two is done again on a rebuilt model.
    - Checked in Mupen: show, record, play, level change (castle entry), remove from a newly started
      STROOP, show again, remove again, leave the castle: no crash, the game memory is as before.
  - **Own body states** (improvement over legacy STROOP's hack): every Mario model, real or ghost,
    reads the real Mario's body state (torso tilt, head rotation, hands, eyes, cap), so legacy
    ghosts copied those from the real Mario. STROOP Next records the body state with every ghost
    frame (a "BODY" section in the .ghost file, ignored by legacy readers) and adds two geo functions
    around the drawing of Mario's model (first and last children of its root): for a ghost, "In"
    saves the real state and loads the ghost's, "Out" puts the real one back. The real state is only
    replaced while a ghost is drawn, never during game logic. Checked in Mupen: the same 61 frames
    played with and without a ghost give the same Mario positions to the thousandth.
  - Patches are written with the emulator **stopped** (NtSuspendProcess on Windows, SIGSTOP/SIGCONT
    on Linux and macOS), like legacy STROOP: the game never runs a frame on half a patch.
  - Do not start recording a movie from a savestate made with the hack enabled.
  - Checked end to end in Mupen (`--ghost-serve`, a test driver writing to a running emulator):
    install, save state + load state, record a run, play it now — the game draws the ghost running
    along the recorded path, with its green cap, while Mario stands still.

**Windows and presets:**
- **⧉ Map window** (toolbar) opens the map in a window of its own that updates every frame,
  whatever the main window shows: watch the Mario tab and the map side by side. It keeps its
  own view and layer settings.
- **Trackers › Presets ▾**: save the current trackers under a name and switch between setups in
  one click (kept in the settings file; Export/Import share them as files).

With these, every feature of legacy STROOP's Mario and Map tabs has a counterpart in STROOP Next.

**Mario**
- Variables come in groups (Position, Speed, Angles, Action, Collision, Status) that you can
  collapse; the collapsed state is remembered.
- Each variable has a **Δ frame** column: the change since the previous *game* frame. It stays
  meaningful when the emulator is paused or when STROOP polls faster than the game runs.
- Values are edited in place: type, or press F2.
  - Angles accept units, `0x` hex or degrees (`90°`).
  - Actions accept a name (`walking/running`) or a hex value.
  - Computed values are writable when that makes sense: height above floor, sliding speed,
    health wedges, hat on head.
  - Invalid input is rejected with a message; nothing is ever written silently.
  - Leaving a cell without changing it never writes the stale value back.
- Commands:
  - move Mario relative to his facing yaw (or in absolute axes);
  - change H/Y speed and facing yaw by a configurable step.
- Angles can be shown as units, degrees, or both (toolbar).

**Map** (top-down)
- Layers, bottom to top: grid with level bounds, floors, ceilings (off by default), walls,
  Mario path, objects, camera, Mario.
  - **Floors** are shaded by height relative to Mario, and Mario's current floor is highlighted.
  - **Walls** show their real collision band: Mario's 50-unit radius, stretched along the
    wall's projection axis. Only walls at Mario's height are shown by default.
  - **Objects** show hitbox and hurtbox cylinders, their facing direction, and names.
  - **Mario** shows his hitbox and his facing, intended (scaled by stick magnitude), sliding
    and moving yaws.
  - **Mario path** has one dot per game frame. It is recorded even while the tab is hidden,
    and resets when a savestate is loaded.
- Hovering anything shows its details: triangle vertices, normal and push axis; object slot,
  behavior, hitbox and action; how far Mario moved on that frame.
- Mouse:
  - wheel to zoom at the cursor;
  - drag to pan; double-click to follow Mario again;
  - **drag Mario to move him**;
  - click an object to select it (Ctrl-click to add to the selection);
  - right-click to teleport Mario (keeping his Y, or onto the floor under the cursor), center
    the map, or copy coordinates.
- The map can be oriented north-up, camera-up or Mario-facing-up.
- **Go to level** (right-click, 2D and 3D; needs a running emulator, US or JP game): the game
  loads another level by itself, the way a painting does: STROOP writes the destination in
  `sWarpDest`, then `sCurrPlayMode = PLAY_MODE_CHANGE_LEVEL` (`level_update.c`), so the level
  script leaves the level and `init_level` spawns Mario at the area's usual warp node
  (`LevelWarp`, `GameLevels`). All the castle, course, Bowser and secret levels are listed, with
  their areas when there are several (areas with no spawn node, and the castle courtyard, whose
  only nodes are the star and death exits, are not). **Go to level, then place Mario…** also
  arms the next click in the map: once the new level is loaded it puts Mario there (the floor
  under the cursor in 2D, the exact 3D point in 3D; Esc or the menu cancels). The act is chosen
  in the same menu (1 by default; the act select screen is skipped on US). A course's first
  visit shows its intro text (A to dismiss), and Bowser's arenas their dialog. It refuses while
  the game is paused. Unknown layouts (rebuilt hacks, SH, EU) get the item greyed out with the
  reason; a hack patched over the US ROM works if it keeps the original levels' warp nodes.
- Every layer has its own options. The options panel is generated from the settings the
  layer declares.

**Map 3D** (the **3D** switch at the top of the map panel)

The same layers, settings and selection, rendered with OpenGL 3.3. The 3D view is only
created when first opened, so no OpenGL context exists unless you use it.

- **Level geometry:** real lit triangles with their edges (floors blue, walls green,
  ceilings red). Mario's floor and wall are highlighted.
- **Wall collision volumes:** translucent prisms, i.e. the triangle shifted by ±50/|normal|
  along its projection axis, for walls at Mario's height.
- **Hitboxes:** real cylinders at their real height, with the hitbox down-offset applied,
  for objects and Mario. Facing, intended, sliding and moving arrows lie at Mario's feet.
  The path is drawn in 3D, and the game camera with its line to the focus.
- **X-ray** (on by default): Mario, objects and hitboxes hidden behind walls show through as
  faint ghosts.
- **Distance fog** keeps the area around the target crisp in large levels.
- **Camera modes:**
  - *Follow Mario*: orbit around him.
  - *Free*: the target stays where you put it.
  - *Game camera*: look through Lakitu, using the position and focus from RAM.

  Leaving the game camera continues from exactly the same view.
- **Mouse:**
  - left-drag orbits; right-drag pans; the wheel zooms;
  - double-click looks at the point under the cursor;
  - hovering describes anything (ray picking against triangles and cylinders);
  - clicking selects objects;
  - right-click teleports Mario to the **exact 3D point** under the cursor, looks at it, or
    copies its coordinates.
- The camera angles, the mode and X-ray are remembered.

**The game's real scene, rebuilt from RAM** (layer *Game visuals*, ported from the
`feature/map-3d-moteur` branch). No ROM and no asset files are needed.
- **Static level:** the current area's graph-node tree is walked, and its display lists run
  through a CPU F3D / F3DEX / F3DEX2 interpreter. Textures are decoded from every N64 format
  (RGBA, CI, IA, I).
- **Objects and Mario:** each object's graph is walked with its live state:
  - animation frame and `oAnimState` switches;
  - Mario's cap, wings, cap effect and transparency;
  - held objects, billboards and shadows;
  - doors hidden by room culling are shown anyway.
- **Generated geometry:**
  - movtex water with scrolling and rotating UVs;
  - paintings, including the **ripple** when Mario jumps in;
  - snow and bubbles;
  - the flying carpet;
  - the castle light.

  Game functions are found through the decomp symbol maps (US/JP) and run with a small MIPS
  leaf interpreter on the snapshot, without ever writing to RAM.
- **Rendering:** the game's sky window, RSP fog (relative to the camera target, so zooming
  out keeps the level visible), RSP lighting, decal, texgen and alpha cut-out.
- **In 2D too:** the 2D map shows the same textured level seen from above.
  - The level is rendered by the GPU in orthographic projection, on an invisible offscreen
    context, then copied into the 2D background cache: about 12 ms per redraw, versus
    ~250 ms for Skia on the CPU, which stays as a fallback.
  - Ceilings are culled as the game does.
  - Objects, Mario and shadows are drawn live by Skia; off-screen objects are skipped.
- **Options:** hide everything above Mario, cull back faces, keep Mario visible from afar.
  The collision layers become translucent overlays on top of the textured level.
- **Trackers** (2D and 3D, off by default except *Next positions*):
  - collision cells, with the current cell;
  - parallel universes (the level is drawn in the 3×3 neighbouring PUs);
  - the current unit;
  - the facing divider and the facing sector;
  - Mario's velocity vector;
  - the game camera's frustum;
  - **next positions**, drawn in 3D as copies of Mario's model with a green cap.

How it is kept fast:
- The level is rebuilt only when its key changes: area, root, level, collision count,
  segment 7, water heights and castle light.
- Object meshes are cached per (display list, layer, incoming RDP state). That state is
  compared field by field, so a wrong mesh can never be reused, and old entries are aged
  out.
- Per-frame meshes (rippling painting, carpet) are streamed and never cached. The original
  branch cached the carpet once per timer value, so its GPU buffer grew forever.
- On the GPU:
  - the level is one buffer, uploaded once;
  - object meshes live in an arena;
  - every copy of a mesh (coins, trees, Mario's predicted copies, PU copies) is **one
    instanced draw per material**;
  - draws are sorted by phase then texture, and GL state changes only when it differs.

Offscreen tools (invisible window, never focused):
- `--ui-snapshot file.st out.png popout` captures the map's own window (the main window shows the
  Mario tab). `STROOPNEXT_GHOST=file.ghost` shows a ghost on the map in `--ui-snapshot`.
- `--render-3d file.st out.png side [axis 0-3] [width] [cut]` renders the side view (axis: 0 side of
  Mario, 1 behind, 2 along X, 3 along Z).
- `--render-3d file.st out.png [follow|game] [yaw°] [pitch°] [distance]` renders a PNG of the
  3D map.
- `STROOPNEXT_LAYERS="game,next,pu"` (environment variable) picks exactly which layers the
  offscreen tools draw; `*` means all of them.
- `--bench-3d file.st [frames]` moves Mario every frame and reports:
  - scene build time;
  - render CPU time;
  - GPU wait;
  - background uploads;
  - memory.

## The engine in the map (the SM64 TAS engine, "le moteur")

Legacy STROOP's map "Engine" panel (module-moteur branch), made simple: the "ENGINE" section of the
map's side panel and the map's right-click menu. STROOP drives the engine (a separate Python program:
headless SM64, CPU and GPU searches) through its HTTP server (`tools/builder/builder_server.py`) and
its command line (`tools/builder/moteur.py battre`). Code: `StroopNext.Core/Engine` (setup, HTTP
client, search log, movie inputs, frames → ghosts; unit-tested) and `StroopNext.Desktop/Engine`.

- **One Connect**: connecting STROOP to Mupen (Connect ▾) starts the engine and `bridge.lua` in that
  Mupen. Mupen has no remote control, so STROOP uses its Win32 menus: the script's "Lua Script →
  Recent Scripts" entry, else "Show Instances → Add instance" with the file picker filled in
  (`Platform/MupenLua.cs`). The bridge's heartbeat says it already runs: never started twice. The
  game keeps running and **the player keeps the controller**: the engine only takes Mupen over at its
  first action (`bridge.lua`, attached to a Mupen opened by the user, now only drives the controller
  while the engine plays frames; it used to set a neutral input every frame). Detach: the engine
  lets go.
- **Each click is a step of the engine's "Faire le chemin"** (`/api/go`, as its own page): the engine
  decides what the point is (floor, door, coin, star…; a click near a door means "through that
  door"), cuts a long stretch into parts along its route, opens and passes doors (a star door
  without the stars: a BLJ on a step), with the whole engine on each part, 120 s per part by
  default. Back removes a whole click, however many parts it made. A search is only stopped when
  its progress has not changed for 2 minutes (it used to be cut at 2× its budget: the 15/10 basement
  → hall search was stopped at 140 s while still working). When no way is found the message says
  what the engine took the point for and its last try, e.g. "going through the door at (-2303,
  -1074); last try: BLJ on the step…" (a 3-star door with 2 stars). The 15/10 "vitesse: access
  violation reading 0x0" (inside the engine, with the GPU on) did not come back in 3 replays of the
  same start and target: an intermittent engine crash, caught (the other experts go on).
- **The path**: right-click in the map → **Bring Mario here (engine)**. The engine finds the fastest
  way from the end of the path to the point (the top-down map: any height; 3D: the clicked point),
  adds it to the path (as close as it could when not reached) and Mupen plays it. The path is drawn
  bold on the map (2D and 3D, Mario's model replaying it). **↶ Back** (panel or right-click) removes
  the last part and Mupen goes back exactly to the end of the one before (its start state, then the
  rest replayed at full speed); then continue with more points. **Save movie…** writes the path as a
  `.m64` with its starting `.st` next to it (Mupen plays them as they are). **New path** starts the
  next one from where Mupen is then. Built on the engine's own session (goto, accept, undo, export).
- **The whole engine** for each point: Expert + GPU, as the engine's own page (Red's selector, then
  the GPU explorer from its line and Scattershot on the CPU at once, Scattershot on the GPU, every
  move: wall kicks, BLJ…, polish, wall-kick climbs, speed search; the CPU explorer when the card is
  busy). 60 s per point by default (Setup), the search ending earlier when it stops improving (10 s).
  It used to ask for plain "expert" (Red + the CPU explorer only). Checked from STROOP: GPU loaded,
  Scattershot CPU alongside, seed received, Scattershot GPU, GPU search, polish; experts shown live
  (speed 28 f, Red 33 f, Scattershot 27 f kept). Requests use a fresh connection each and reads are
  tried again (the Python server, busy, dropped some: "the engine does not answer"); a search still
  running is stopped before the next; a point Mario is already at adds nothing.
- **Live search**: while the engine looks for the way, the panel shows its phase, the time against
  the budget and each expert's line so far (e.g. "explore · 8 s / 15 s · red 82 f", with Stop), and
  the map marks the target point. A search far past its budget (2× + 20 s) is stopped by STROOP; an
  engine that does not even stop is restarted (when STROOP started it) and the path reopened from
  its movie. "The engine is stuck" (15/10): Red's selector had no time limit on a goto (`red_s` 0;
  the server never passed it) and ran for minutes with Mario in a dialog; the engine's server now
  passes `red_s` (the budget by default) and STROOP sends it. A search that finds no way at all no
  longer adds an empty part. Checked in a running game: live text every second, target on the map,
  A (11 f) and B (65 f) reached, Back to A's exact end.
- **Improve a movie** (`moteur.py battre`): the movie (the path saved last by default), minutes,
  ▶ Improve / ■ Stop; folded options (keep going, CPU only, no Mupen check, no competitors). Live:
  start → best, the attempt it works on and the reference as ghosts on the map; every verified
  record with Save as… (movie + .st).
- Folded away: **Setup** (engine folder and Python, found by themselves; port; search time per
  point; ROM hack; start with Mupen) and the **Log**.

Checked in a Mupen opened by hand: Connect → ready; Bring Mario here to A (1 part, 10 frames), to B
(2 parts, 33 frames, Mario at B in Mupen); Back → 1 part and Mario exactly back at A's end in Mupen
(same position, same timer); continue to C (2 parts); Save movie → `.m64` of 10 frames + its `.st`;
the bold path on the map. Not checked by hand: the player's own controller after the bridge change
(the code no longer sets the controller outside the engine's frames).

Teleport from the map's right-click: the 3D menu used to be greyed out when the click fell on the
textured level (not pickable), and to put Mario on whatever was hit (a wall, an object). It now aims at
the game's own collision under the cursor, whatever layers are shown, and puts Mario on its floor (a
wall or ceiling clicked: the floor in front of / below it). Checked in a running game: 2D onto the
floor and 3D, Mario stays there while the game runs, also with the engine holding Mupen.

## Verified so far

- **The game scene from RAM**, rendered offscreen from real savestates:
  - castle grounds: camera view, orbit, water, 3×3 PU copies;
  - castle hall and the stairs to Bob-omb;
  - Bowser in the Dark World;
  - Mario's predicted copies.

  Per frame (RTX 5070, 1200×900):
  - scene build (level cached, objects walked) 0.15–0.4 ms;
  - render CPU ~0.4 ms;
  - GPU ~0.4 ms;
  - 86–110 draw calls for the whole game scene.

  Managed memory stays flat over 6000 frames (13.25 → 13.25 MB after full GCs).

- **3D map**, checked offscreen on an RTX 5070 with real savestates:
  - per frame: scene build ~0.1–0.3 ms, render CPU ~0.35 ms;
  - the level is uploaded to the GPU **once** for 3000 frames;
  - private memory is flat over 3000 frames (147 → 142 MB).

  The GPU wait could not be measured cleanly because another application was using the GPU.
  The UI never waits for the GPU anyway.
- **220 unit and integration tests** cover:
  - endianness, patches and version detection;
  - Mario, objects and surfaces;
  - variables: formatting, parsing, writing and deltas;
  - the session: buffer pooling, the previous game frame, held frames, tracks;
  - map math: transforms, zoom anchor, orientation, wall bands and hit testing;
  - the 3D map:
    - camera orientation, consistent with the 2D map;
    - projection / ray round trip and pan;
    - leaving the game camera without a jump;
    - ray–triangle and ray–cylinder picking;
    - tessellation passes, wall prisms, 3D layers;
    - background sections being reused;
  - settings persistence;
  - real savestates from `D:\mupen` when they are present. Set `STROOP_TEST_SAVESTATES` to
    use another folder.
- **Live attach** to mupen64-rr-lua 1.5.0 running SM64 (US):
  - found through `CORE_RDRAM` in under 100 ms;
  - polls at about 60 Hz;
  - write → read-back round trip succeeds.
- **The UI was launched on real savestates and both tabs were captured.**

Not verified yet:
- the memory-scan fallback against an emulator that has no symbol;
- the JP, SH and EU address tables beyond what legacy STROOP provides. Missing entries show
  `n/a` instead of reading address 0;
- mouse interactions, which were only exercised through their underlying, unit-tested logic;
- the 3D view inside the app (GLControl). It uses the same renderer as the offscreen tools
  and starts without error, but it was not inspected on screen.

## Open questions for review

1. **Variables are declared in C# rather than XML.** The reason is typing and refactoring:
   computed variables are just readers. Should a data format also exist for user watch
   lists, referencing variable ids?
2. **One layer class, two projections.** Each layer implements `Build` (2D) and `Build3D`
   (3D) and shares its settings between them. Should a layer be able to exist in one view
   only?
3. **UI toolkit.** The app now runs on Avalonia, so it works on Windows, Linux and macOS.
   Core, the renderers and the platform layer did not need to change for it. Should the
   Windows Forms host be removed now, or kept until the Avalonia app has been used for a
   while?
