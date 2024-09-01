; this describes the main update loop to create, delete and animate Gfx nodes for ghosts

; common registers
RegMarioObject equ s4           ; Permanent reference to gMarioObject
RegAnimationBuffer equ s3       ; Pointer to the "Mario animation buffer" for the currently processed ghost node
RegCurrentGhost equ s2          ; Memory offset of the currently processed ghost node
RegPointerToCurrentGhost equ s1 ; Pointer to the currently processed ghost node
RegProcessedGhostCount equ s0   ; Iteration counter for loops that process all ghosts

.n64

addiu SP, SP, 0xFFC0
; return early if there's no Mario object
lui t0, MarioObjectAddrHi
lw t0, MarioObjectAddrLo (t0)
beq r0, t0, @@EARLY_RETURN

; push static registers to stack
sw ra, 0x34 (SP)
sw RegMarioObject, 0x30 (SP)
sw RegAnimationBuffer, 0x2C (SP)
sw RegCurrentGhost, 0x28 (SP)
sw RegPointerToCurrentGhost, 0x24 (SP)
sw RegProcessedGhostCount, 0x20 (SP)

; skip initializing if already Mario object is flagged
or RegMarioObject, r0, t0
lh t0, 0x2 (RegMarioObject)
andi t1, t0, 0x40
bnez t1, @@SKIP_INIT

; set initialized flag on Mario object
ori t1, t0, 0x40
sh t1, 0x2 (RegMarioObject)

; clean up (this can cause failure?)
lui RegPointerToCurrentGhost, 0x8040
beq r0, r0, @@CLEAN_UP_EARLY
ori RegPointerToCurrentGhost, RegPointerToCurrentGhost, 0x7FF8
@@SKIP_INIT:

; set up dummy Mario struct
lui RegAnimationBuffer, 0x8050
lui t8, 0x8037
ori at, r0, 0xBD
sh at, 0x5A8 (t8)
ori at, t8, 0x5B8
sw at, 0x598 (t8)
li at, 0x80064040
sw at, 0x5B8 (t8)

; clean up if no ghosts are requested
or RegProcessedGhostCount, r0, r0
lui at, 0x8040
ori RegPointerToCurrentGhost, at, 0x7FF8
lb at, 0x7FFF (at)
beq r0, at, @@CLEAN_UP_EARLY
nop

@@ITERATE_GHOSTS:

; skip initialization if ghost already exists
lw t0, 0x0 (RegPointerToCurrentGhost)
bnez t0, @@GHOST_EXISTS

; create a new ghost object graph node
or a0, r0, r0
addiu a1, RegPointerToCurrentGhost, 0xFF9C
lw a2, 0x14 (RegMarioObject)
lui a3, 0x8038
ori at, a3, 0x5FDC
sw at, 0x10 (SP)
ori at, a3, 0x5FE4
sw at, 0x14 (SP)
jal 0x8037B9E0
ori a3, a3, 0x5FD0

; add the new graph node object as child to Mario's parent node
sw v0, 0x0 (RegPointerToCurrentGhost)
lw a0, 0xC (RegMarioObject)
jal 0x8037C044
or a1, v0, r0

@@GHOST_EXISTS:

; copy Mario's area and something else into the ghost node
lw RegCurrentGhost, 0x0 (RegPointerToCurrentGhost)
lb t1, 0x18 (RegMarioObject)
sb t1, 0x18 (RegCurrentGhost)
lw t1, 0x38 (RegMarioObject)
sw t1, 0x38 (RegCurrentGhost)

; find offset in ghost data buffer
lui at, GlobalTimerAddrHi
lw t0, GlobalTimerAddrLo (at)
andi t0, t0, 0x7F
sll t0, t0, 0x5
sll t1, RegProcessedGhostCount, 0xC
addu t0, t0, t1
lui at, 0x8041
addu t0, t0, at
addiu t0, t0, 0x8800

; copy data from buffer to ghost node

; position
lw t1, 0x00 (t0)
sw t1, 0x20 (RegCurrentGhost)
lw t1, 0x04 (t0)
sw t1, 0x24 (RegCurrentGhost)
lw t1, 0x08 (t0)
sw t1, 0x28 (RegCurrentGhost)

; angles (TODO: store angles as RegPointerToCurrentGhost6 in file?)
lw t1, 0x10 (t0)
sh t1, 0x1A (RegCurrentGhost)
lw t1, 0x14 (t0)
sh t1, 0x1C (RegCurrentGhost)
lw t1, 0x18 (t0)
sh t1, 0x1E (RegCurrentGhost)

; do the hacky thing with Mario animations
lui t8, 0x8037
sw RegCurrentGhost, 0x0580 (t8)
sw RegAnimationBuffer, 0x05C0 (t8)
lh t1, 0x1C (t0)
sh t1, 0x38 (SP)
ori a0, t8, 0x04F8
sh r0, 0x38 (RegCurrentGhost)
sw r0, 0x05BC (t8)
jal set_mario_animation
lw a1, 0xC (t0)
lh t1, 0x38 (SP)
sh t1, 0x40 (RegCurrentGhost)

; move on to next ghost
addiu RegAnimationBuffer, RegAnimationBuffer, 0x4000
addiu RegPointerToCurrentGhost, RegPointerToCurrentGhost, 0xFF98
addiu RegProcessedGhostCount, RegProcessedGhostCount, 0x1
lui at, 0x8040
lb at, 0x7FFF (at)
sltu t0, RegProcessedGhostCount, at
bnez t0, @@ITERATE_GHOSTS
sb RegProcessedGhostCount, 0x60 (RegCurrentGhost)

; delete leftover ghosts
lw RegCurrentGhost, 0x0 (RegPointerToCurrentGhost)
beq RegCurrentGhost, r0, @@RETURN
or a0, r0, RegCurrentGhost
@@CLEAN_UP_LOOP:
jal 0x8037C0BC
sw r0, 0x0 (RegPointerToCurrentGhost)
@@CLEAN_UP_EARLY:
lw a0, 0x0 (RegPointerToCurrentGhost)
bnez a0, @@CLEAN_UP_LOOP
addiu RegPointerToCurrentGhost, RegPointerToCurrentGhost, 0xFF98

@@RETURN:

; pop static registers from stack
lw ra, 0x34 (SP)
lw RegMarioObject, 0x30 (SP)
lw RegAnimationBuffer, 0x2C (SP)
lw RegCurrentGhost, 0x28 (SP)
lw RegPointerToCurrentGhost, 0x24 (SP)
lw RegProcessedGhostCount, 0x20 (SP)

@@EARLY_RETURN:
jr ra
addiu SP, SP, 0x40
