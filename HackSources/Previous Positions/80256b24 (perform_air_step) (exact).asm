addiu	$sp, $sp, -0x30
sw	$ra, 0x14($sp)
sw	$a0, 0x30($sp)
sw	$a1, 0x34($sp)
sw	$zero, 0x18($sp)
lw	$t6, 0x30($sp)
sw	$zero, 0x60($t6)
sw	$zero, 0x20($sp)
lw	$t7, 0x30($sp)
lui	$at, 0x4080
mtc1	$at, $f6
lwc1	$f4, 0x48($t7)
lwc1	$f10, 0x3c($t7)
addiu	$t8, $sp, 0x24
div.s	$f8, $f4, $f6
add.s	$f16, $f8, $f10
swc1	$f16, ($t8)
addiu	$a0, $sp, 0x24
lui	$at, 0x4080
mtc1	$at, $f4
lwc1	$f18, 0x4c($t7)
lwc1	$f8, 0x40($t7)
addiu	$t0, $sp, 0x24
div.s	$f6, $f18, $f4
add.s	$f10, $f6, $f8
swc1	$f10, 4($t0)
lui	$at, 0x4080
mtc1	$at, $f18
lwc1	$f16, 0x50($t7)
lwc1	$f6, 0x44($t7)
addiu	$t2, $sp, 0x24
div.s	$f4, $f16, $f18
add.s	$f8, $f4, $f6
jal	set_next_argument_position_and_mario_forward_angle
swc1	$f8, 8($t2)
lw	$a0, 0x30($sp)
addiu	$a1, $sp, 0x24
jal	perform_air_quarter_step
lw	$a2, 0x34($sp)
sw	$v0, 0x1c($sp)
lui	$a0, MarioPosHi
jal	set_next_argument_position_and_mario_forward_angle
addiu	$a0, $a0, MarioPosLoX
lw	$t5, 0x1c($sp)
beq	$zero, $t5, 0xbc
addiu	$at, $zero, 1
sw	$t5, 0x18($sp)
beq	$t5, $at, 0xe8
nop
addiu	$at, $zero, 3
beq	$t5, $at, 0xe8
nop
addiu	$at, $zero, 4
beq	$t5, $at, 0xe8
nop
addiu	$at, $zero, 6
bne	$t5, $at, 0xf0
nop
b	0x104
nop
lw	$t6, 0x20($sp)
addiu	$t7, $t6, 1
slti	$at, $t7, 4
bnez	$at, 0x20
sw	$t7, 0x20($sp)
lw	$t8, 0x30($sp)
mtc1	$zero, $f16
lwc1	$f10, 0x4c($t8)
c.le.s	$f16, $f10
nop
bc1f	0x12c
nop
lw	$t9, 0x30($sp)
lwc1	$f18, 0x40($t9)
swc1	$f18, 0xbc($t9)
jal	mario_get_terrain_sound_addend
lw	$a0, 0x30($sp)
lw	$t0, 0x30($sp)
sw	$v0, 0x14($t0)
lw	$t1, 0x30($sp)
lui	$at, 0x1088
ori	$at, $at, 0x899
lw	$t2, 0xc($t1)
beq	$t2, $at, 0x15c
nop
jal	apply_gravity
lw	$a0, 0x30($sp)
jal	apply_vertical_wind
lw	$a0, 0x30($sp)
lw	$t3, 0x30($sp)
lw	$a0, 0x88($t3)
addiu	$a1, $t3, 0x3c
jal	vec3f_copy
addiu	$a0, $a0, 0x20
lw	$t4, 0x30($sp)
move	$a1, $zero
move	$a3, $zero
lw	$a0, 0x88($t4)
lh	$a2, 0x2e($t4)
jal	vec3s_set
addiu	$a0, $a0, 0x1a
b	0x1a4
lw	$v0, 0x18($sp)
b	0x1a4
nop
lw	$ra, 0x14($sp)
addiu	$sp, $sp, 0x30
jr	$ra