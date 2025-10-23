addiu	$sp, $sp, -0x40
sw	$ra, 0x14($sp)
sw	$a0, 0x40($sp)
lw	$t6, 0x40($sp)
lw	$t7, 0x88($t6)
sw	$t7, 0x1c($sp)
lw	$a1, 0x40($sp)
addiu	$a0, $sp, 0x20
jal	vec3f_copy
addiu	$a1, $a1, 0x48
lw	$t8, 0x40($sp)
lw	$t9, 0xc($t8)
andi	$t0, $t9, 0x2000
beqz	$t0, 0x48
nop
lw	$a0, 0x40($sp)
jal	0x270500
addiu	$a1, $sp, 0x20
lw	$t2, 0x40($sp)
addiu	$t1, $sp, 0x20
lwc1	$f4, ($t1)
lwc1	$f6, 0x3c($t2)
addiu	$t3, $sp, 0x2c
add.s	$f8, $f4, $f6
swc1	$f8, ($t3)
lw	$t5, 0x40($sp)
addiu	$t4, $sp, 0x20
lwc1	$f10, 4($t4)
lwc1	$f16, 0x40($t5)
addiu	$t6, $sp, 0x2c
add.s	$f18, $f10, $f16
swc1	$f18, 4($t6)
lw	$t8, 0x40($sp)
addiu	$t7, $sp, 0x20
lwc1	$f4, 8($t7)
lwc1	$f6, 0x44($t8)
addiu	$t9, $sp, 0x2c
add.s	$f8, $f4, $f6
swc1	$f8, 8($t9)
jal	set_next_argument_position_and_mario_forward_angle
addiu	$a0, $sp, 0x2c
lw	$t1, 0x40($sp)
lh	$t2, 0x76($t1)
addiu	$t3, $t2, -0x50
mtc1	$t3, $f16
lwc1	$f10, 0x30($sp)
cvt.s.w	$f18, $f16
c.lt.s	$f18, $f10
nop
bc1f	0xf8
nop
lw	$t4, 0x40($sp)
addiu	$t7, $sp, 0x2c
lh	$t5, 0x76($t4)
addiu	$t6, $t5, -0x50
mtc1	$t6, $f4
nop
cvt.s.w	$f6, $f4
swc1	$f6, 4($t7)
mtc1	$zero, $f8
lw	$t8, 0x40($sp)
swc1	$f8, 0x4c($t8)
lw	$a0, 0x40($sp)
jal	0x270304
addiu	$a1, $sp, 0x2c
sw	$v0, 0x38($sp)
lw	$a0, 0x1c($sp)
lw	$a1, 0x40($sp)
addiu	$a0, $a0, 0x20
jal	vec3f_copy
addiu	$a1, $a1, 0x3c
lw	$t9, 0x40($sp)
lw	$a0, 0x1c($sp)
lh	$a1, 0x2c($t9)
lh	$a2, 0x2e($t9)
lh	$a3, 0x30($t9)
addiu	$a0, $a0, 0x1a
jal	0x37897c
negu	$a1, $a1
nop
lw	$v0, 0x38($sp)
lui	$a0, MarioPosHi
jal	set_next_argument_position_and_mario_forward_angle
addiu	$a0, $a0, MarioPosLoX
lw	$ra, 0x14($sp)
jr	$ra
addiu	$sp, $sp, 0x40