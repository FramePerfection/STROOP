addiu	$sp, $sp, -0x30
sw	$ra, 0x14($sp)
sw	$a0, 0x30($sp)
sw	$a1, 0x34($sp)
lw	$a0, 0x34($sp)
lui	$a1, 0x4120
jal	do_wall_collisions_for_water_full_step
lui	$a2, 0x42dc
sw	$v0, 0x2c($sp)
lw	$t6, 0x34($sp)
addiu	$a3, $sp, 0x24
lwc1	$f12, ($t6)
lwc1	$f14, 4($t6)
jal	find_floor
lw	$a2, 8($t6)
swc1	$f0, 0x1c($sp)
lw	$a0, 0x34($sp)
lw	$a1, 0x1c($sp)
jal	vec3f_find_ceil
addiu	$a2, $sp, 0x28
swc1	$f0, 0x20($sp)
lw	$t7, 0x24($sp)
bnez	$t7, 0x68
nop
b	0x1ec
addiu	$v0, $zero, 3
lw	$t8, 0x34($sp)
lwc1	$f6, 0x1c($sp)
lwc1	$f4, 4($t8)
c.le.s	$f6, $f4
nop
bc1f	0x17c
nop
lw	$t9, 0x34($sp)
lwc1	$f8, 0x20($sp)
lui	$at, 0x4320
lwc1	$f10, 4($t9)
mtc1	$at, $f18
sub.s	$f16, $f8, $f10
c.le.s	$f18, $f16
nop
bc1f	0xf8
nop
lw	$a0, 0x30($sp)
lw	$a1, 0x34($sp)
jal	vec3f_copy
addiu	$a0, $a0, 0x3c
lw	$t0, 0x24($sp)
lw	$t1, 0x30($sp)
sw	$t0, 0x68($t1)
lwc1	$f4, 0x1c($sp)
lw	$t2, 0x30($sp)
swc1	$f4, 0x70($t2)
lw	$t3, 0x2c($sp)
beqz	$t3, 0xf0
nop
b	0x1ec
addiu	$v0, $zero, 4
b	0xf8
nop
b	0x1ec
move	$v0, $zero
lwc1	$f6, 0x20($sp)
lwc1	$f8, 0x1c($sp)
lui	$at, 0x4320
mtc1	$at, $f16
sub.s	$f10, $f6, $f8
c.lt.s	$f10, $f16
nop
bc1f	0x124
nop
b	0x1ec
addiu	$v0, $zero, 3
lui	$at, 0x4320
mtc1	$at, $f4
lwc1	$f18, 0x20($sp)
lw	$t4, 0x34($sp)
lw	$a0, 0x30($sp)
sub.s	$f6, $f18, $f4
lw	$a1, ($t4)
lw	$a3, 8($t4)
addiu	$a0, $a0, 0x3c
mfc1	$a2, $f6
jal	vec3f_set
nop
lw	$t5, 0x24($sp)
lw	$t6, 0x30($sp)
sw	$t5, 0x68($t6)
lwc1	$f8, 0x1c($sp)
lw	$t7, 0x30($sp)
swc1	$f8, 0x70($t7)
b	0x1ec
addiu	$v0, $zero, 2
b	0x1e4
nop
lwc1	$f10, 0x20($sp)
lwc1	$f16, 0x1c($sp)
lui	$at, 0x4320
mtc1	$at, $f4
sub.s	$f18, $f10, $f16
c.lt.s	$f18, $f4
nop
bc1f	0x1a8
nop
b	0x1ec
addiu	$v0, $zero, 3
lw	$t8, 0x34($sp)
lw	$a0, 0x30($sp)
lw	$a2, 0x1c($sp)
lw	$a1, ($t8)
lw	$a3, 8($t8)
jal	vec3f_set
addiu	$a0, $a0, 0x3c
lw	$t9, 0x24($sp)
lw	$t0, 0x30($sp)
sw	$t9, 0x68($t0)
lwc1	$f6, 0x1c($sp)
lw	$t1, 0x30($sp)
swc1	$f6, 0x70($t1)
b	0x1ec
addiu	$v0, $zero, 1
b	0x1ec
nop
lw	$ra, 0x14($sp)
addiu	$sp, $sp, 0x30
jr	$ra
nop