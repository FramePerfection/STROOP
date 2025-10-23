addiu	$sp, $sp, -0x30
sw	$ra, 0x14($sp)
sw	$a0, 0x30($sp)
sw	$zero, 0x2c($sp)
lw	$t6, 0x30($sp)
lui	$at, 0x4080
mtc1	$at, $f6
lwc1	$f4, 0x48($t6)
lw	$t7, 0x68($t6)
lwc1	$f18, 0x3c($t6)
div.s	$f8, $f4, $f6
lwc1	$f10, 0x20($t7)
addiu	$t8, $sp, 0x1c
mul.s	$f16, $f8, $f10
add.s	$f4, $f16, $f18
swc1	$f4, ($t8)
nop
lui	$at, 0x4080
mtc1	$at, $f8
lwc1	$f6, 0x50($t6)
lw	$t0, 0x68($t6)
lwc1	$f4, 0x44($t6)
div.s	$f10, $f6, $f8
lwc1	$f16, 0x20($t0)
addiu	$t1, $sp, 0x1c
mul.s	$f18, $f10, $f16
add.s	$f6, $f18, $f4
swc1	$f6, 8($t1)
addiu	$a0, $sp, 0x1c
lwc1	$f8, 0x40($t6)
jal	set_next_argument_position_and_mario_forward_angle
swc1	$f8, 0x20($sp)
lw	$a0, 0x30($sp)
jal	0x255b04
addiu	$a1, $sp, 0x1c
sw	$v0, 0x28($sp)
nop
nop
nop
lw	$t4, 0x28($sp)
beqz	$t4, 0xc4
addiu	$at, $zero, 2
beq	$t4, $at, 0xc4
nop
lw	$t5, 0x2c($sp)
addiu	$t7, $t5, 1
slti	$at, $t7, 4
bnez	$at, 0x10
sw	$t7, 0x2c($sp)
jal	0x2518a8
lw	$a0, 0x30($sp)
lw	$t6, 0x30($sp)
sw	$v0, 0x14($t6)
lw	$t8, 0x30($sp)
lw	$a0, 0x88($t8)
addiu	$a1, $t8, 0x3c
jal	vec3f_copy
addiu	$a0, $a0, 0x20
lw	$t0, 0x30($sp)
move	$a1, $zero
move	$a3, $zero
lw	$a0, 0x88($t0)
lh	$a2, 0x2e($t0)
jal	0x37897c
addiu	$a0, $a0, 0x1a
lw	$t9, 0x28($sp)
addiu	$at, $zero, 3
bne	$t9, $at, 0x11c
nop
addiu	$t1, $zero, 2
sw	$t1, 0x28($sp)
b	0x12c
lw	$v0, 0x28($sp)
b	0x12c
nop
lw	$ra, 0x14($sp)
addiu	$sp, $sp, 0x30
jr	$ra
nop