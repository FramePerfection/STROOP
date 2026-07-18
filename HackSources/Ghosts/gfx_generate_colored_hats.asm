.n64

addiu sp, sp, 0xFFC0
sw ra, 0x14 (sp)

lui t1, 0x0401
ori t0, t1, 0x19A0
lui at, MarioObjectAddrHi
lw at, MarioObjectAddrLo (at)
lui t8, gCurGraphNodeObjectHi
lw t8, gCurGraphNodeObjectLo (t8)
beq t8, at, @@SkipGhostRead
or t2, r0, r0
lb t2, 0x60 (t8)

@@SkipGhostRead:
ori t1, t1, 0x1978

sw t0, 0x18 (sp)
sw t1, 0x1C (sp)
sw t2, 0x20 (sp)
addiu at, r0, 0x1
bne at, a0, @@FinishTheJob
ori a0, r0, 0x38
jal alloc_display_list
nop
lui t4, 0x0600
sw t4, 0x10 (v0)
lw at, 0x18 (sp)
sw at, 0x14 (v0)
lui at, 0x0388
ori at, at, 0x0010
sw at, 0x18 (v0)
sw at, 0x0 (v0)
lui t0, GhostBaseHi                     ; colored-hats "lights" buffer at base + 0x8300 (GhostBaseHi comes from ghost_loop.asm, included first; mirrors COLORED_HATS_LIGHTS_ADDR in C#)
ori t0, t0, 0x8300
lw t1, 0x20 (sp)
sll t1, t1, 0x5
addu t2, t1, t0
sw t2, 0x1C (v0)
sw t2, 0x4 (v0)
lui at, 0x0386
ori at, at, 0x0010
sw at, 0x20 (v0)
sw at, 0x8 (v0)
addiu t3, t2, 0x8
sw t3, 0x24 (v0)
sw t3, 0xC (v0)
sw t4, 0x28 (v0)
lw t0, 0x1C (sp)

@@FinishTheJob:
sw t0, 0x2C (v0)
lui at, 0xB800
sw at, 0x30 (v0)
lw ra, 0x14 (sp)
jr ra
addiu sp, sp, 0x40