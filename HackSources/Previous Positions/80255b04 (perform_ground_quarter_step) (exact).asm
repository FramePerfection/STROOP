addiu	$sp, $sp, -0x40
sw	$ra, 0x1c($sp)
sw	$a0, 0x40($sp)
sw	$a1, 0x44($sp)
sw	$s0, 0x18($sp)
lw	$a0, 0x44($sp)
lui	$a1, 0x41f0
jal	do_wall_collisions_for_ground_quarter_step
lui	$a2, 0x41c0
sw	$v0, 0x3c($sp)
lw	$a0, 0x44($sp)
lui	$a1, 0x4270
jal	do_wall_collisions_for_ground_quarter_step
lui	$a2, 0x4248
sw	$v0, 0x38($sp)
lw	$t6, 0x44($sp)
addiu	$a3, $sp, 0x30
lwc1	$f12, ($t6)
lwc1	$f14, 4($t6)
jal	find_floor
lw	$a2, 8($t6)
swc1	$f0, 0x28($sp)
lw	$a0, 0x44($sp)
lw	$a1, 0x28($sp)
jal	vec3f_find_ceil
addiu	$a2, $sp, 0x34
swc1	$f0, 0x2c($sp)
lw	$t7, 0x44($sp)
lwc1	$f12, ($t7)
jal	find_water_level
lwc1	$f14, 8($t7)
swc1	$f0, 0x24($sp)
lw	$t8, 0x38($sp)
lw	$t9, 0x40($sp)
sw	$t8, 0x60($t9)
jal	set_next_argument_position_and_mario_forward_angle
lw	$a0, 0x44($sp)
lw	$t0, 0x30($sp)
beq	$zero, $t0, 0x270
addiu	$v0, $zero, 2
lw	$t1, 0x40($sp)
lui	$at, 1
lw	$t2, 0xc($t1)
and	$t3, $t2, $at
beqz	$t3, 0xf0
nop
lwc1	$f4, 0x28($sp)
lwc1	$f6, 0x24($sp)
c.lt.s	$f4, $f6
nop
bc1f	0xf0
nop
lwc1	$f8, 0x24($sp)
swc1	$f8, 0x28($sp)
lui	$t4, 0x8033
addiu	$t4, $t4, -0x2508
sw	$t4, 0x30($sp)
lwc1	$f10, 0x28($sp)
lw	$t5, 0x30($sp)
swc1	$f10, 0x28($t5)
lui	$at, 0x42c8
mtc1	$at, $f4
lwc1	$f18, 0x28($sp)
lw	$t6, 0x44($sp)
add.s	$f6, $f18, $f4
lwc1	$f16, 4($t6)
c.lt.s	$f6, $f16
nop
bc1f	0x178
nop
lw	$t7, 0x44($sp)
lui	$at, 0x4320
mtc1	$at, $f10
lwc1	$f8, 4($t7)
lwc1	$f4, 0x2c($sp)
add.s	$f18, $f8, $f10
c.le.s	$f4, $f18
nop
bc1f	0x148
nop
b	0x270
addiu	$v0, $zero, 2
lw	$a0, 0x40($sp)
lw	$a1, 0x44($sp)
jal	vec3f_copy
addiu	$a0, $a0, 0x3c
lw	$t8, 0x30($sp)
lw	$t9, 0x40($sp)
sw	$t8, 0x68($t9)
lwc1	$f16, 0x28($sp)
lw	$t0, 0x40($sp)
swc1	$f16, 0x70($t0)
b	0x270
move	$v0, $zero
lui	$at, 0x4320
mtc1	$at, $f8
lwc1	$f6, 0x28($sp)
lwc1	$f18, 0x2c($sp)
add.s	$f10, $f6, $f8
c.le.s	$f18, $f10
nop
bc1f	0x1a4
nop
b	0x270
addiu	$v0, $zero, 2
lw	$t1, 0x44($sp)
lw	$a0, 0x40($sp)
lw	$a2, 0x28($sp)
lw	$a1, ($t1)
lw	$a3, 8($t1)
jal	overwrite_mario_position_and_forward_angle
addiu	$a0, $a0, 0x3c
lw	$t2, 0x30($sp)
lw	$t3, 0x40($sp)
sw	$t2, 0x68($t3)
lwc1	$f4, 0x28($sp)
lw	$t4, 0x40($sp)
swc1	$f4, 0x70($t4)
lw	$t5, 0x38($sp)
beqz	$t5, 0x260
nop
lw	$t6, 0x38($sp)
lwc1	$f12, 0x24($t6)
jal	atan2s
lwc1	$f14, 0x1c($t6)
lw	$t8, 0x40($sp)
sll	$s0, $v0, 0x10
sra	$t7, $s0, 0x10
lh	$t9, 0x2e($t8)
move	$s0, $t7
subu	$t0, $s0, $t9
sh	$t0, 0x22($sp)
lh	$t1, 0x22($sp)
slti	$at, $t1, 0x2aaa
bnez	$at, 0x234
nop
slti	$at, $t1, 0x5556
beqz	$at, 0x234
nop
b	0x270
addiu	$v0, $zero, 1
lh	$t2, 0x22($sp)
slti	$at, $t2, -0x2aa9
beqz	$at, 0x258
nop
slti	$at, $t2, -0x5555
bnez	$at, 0x258
nop
b	0x270
addiu	$v0, $zero, 1
b	0x270
addiu	$v0, $zero, 3
b	0x270
addiu	$v0, $zero, 1
b	0x270
nop
lw	$ra, 0x1c($sp)
lw	$s0, 0x18($sp)
addiu	$sp, $sp, 0x40
jr	$ra
nop