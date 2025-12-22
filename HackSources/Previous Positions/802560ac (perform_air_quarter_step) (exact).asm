addiu	$sp, $sp, -0x50
sw	$ra, 0x1c($sp)
sw	$a0, 0x50($sp)
sw	$a1, 0x54($sp)
sw	$a2, 0x58($sp)
sw	$s0, 0x18($sp)
addiu	$a0, $sp, 0x40
jal	vec3f_copy
lw	$a1, 0x54($sp)
addiu	$a0, $sp, 0x40
lui	$a1, 0x4316
jal	do_wall_collisions_for_air_quarter_step
lui	$a2, 0x4248
sw	$v0, 0x3c($sp)
addiu	$a0, $sp, 0x40
lui	$a1, 0x41f0
jal	do_wall_collisions_for_air_quarter_step
lui	$a2, 0x4248
sw	$v0, 0x38($sp)
addiu	$t6, $sp, 0x40
lwc1	$f12, ($t6)
lwc1	$f14, 4($t6)
lw	$a2, 8($t6)
jal	find_floor
addiu	$a3, $sp, 0x30
swc1	$f0, 0x28($sp)
addiu	$a0, $sp, 0x40
lw	$a1, 0x28($sp)
jal	vec3f_find_ceil
addiu	$a2, $sp, 0x34
swc1	$f0, 0x2c($sp)
addiu	$t7, $sp, 0x40
lwc1	$f12, ($t7)
jal	find_water_level
lwc1	$f14, 8($t7)
swc1	$f0, 0x24($sp)
lw	$t8, 0x50($sp)
sw	$zero, 0x60($t8)
nop
addiu	$a0, $sp, 0x40
lw	$t9, 0x30($sp)
bnez	$t9, 0xf0
lw	$t1, 0x50($sp)
lwc1	$f4, 0x44($sp)
lwc1	$f6, 0x70($t1)
c.le.s	$f4, $f6
nop
bc1f	0xd8
nop
lw	$t2, 0x50($sp)
lwc1	$f8, 0x70($t2)
swc1	$f8, 0x40($t2)
b	0x420
addiu	$v0, $zero, 1
addiu	$t3, $sp, 0x40
lwc1	$f10, 4($t3)
lw	$t4, 0x50($sp)
swc1	$f10, 0x40($t4)
b	0x420
addiu	$v0, $zero, 2
lw	$t5, 0x50($sp)
lui	$at, 1
lw	$t6, 0xc($t5)
and	$t7, $t6, $at
beqz	$t7, 0x140
nop
lwc1	$f16, 0x28($sp)
lwc1	$f18, 0x24($sp)
c.lt.s	$f16, $f18
nop
bc1f	0x140
nop
lwc1	$f4, 0x24($sp)
swc1	$f4, 0x28($sp)
lui	$t8, gWaterSurfacePseudoFloorHi
addiu	$t8, $t8, gWaterSurfacePseudoFloorLo
sw	$t8, 0x30($sp)
lwc1	$f6, 0x28($sp)
lw	$t9, 0x30($sp)
swc1	$f6, 0x28($t9)
addiu	$t0, $sp, 0x40
lwc1	$f8, 4($t0)
lwc1	$f10, 0x28($sp)
c.le.s	$f8, $f10
nop
bc1f	0x1cc
nop
lwc1	$f16, 0x2c($sp)
lwc1	$f18, 0x28($sp)
lui	$at, 0x4320
mtc1	$at, $f6
sub.s	$f4, $f16, $f18
c.lt.s	$f6, $f4
nop
bc1f	0x1b8
nop
addiu	$t1, $sp, 0x40
lwc1	$f8, ($t1)
lw	$t2, 0x50($sp)
swc1	$f8, 0x3c($t2)
addiu	$t3, $sp, 0x40
lwc1	$f10, 8($t3)
lw	$t4, 0x50($sp)
swc1	$f10, 0x44($t4)
lw	$t5, 0x30($sp)
lw	$t6, 0x50($sp)
sw	$t5, 0x68($t6)
lwc1	$f16, 0x28($sp)
lw	$t7, 0x50($sp)
swc1	$f16, 0x70($t7)
lwc1	$f18, 0x28($sp)
lw	$t8, 0x50($sp)
swc1	$f18, 0x40($t8)
b	0x420
addiu	$v0, $zero, 1
addiu	$t9, $sp, 0x40
lui	$at, 0x4320
mtc1	$at, $f6
lwc1	$f4, 4($t9)
lwc1	$f10, 0x2c($sp)
add.s	$f8, $f4, $f6
c.lt.s	$f10, $f8
nop
bc1f	0x2a8
nop
lw	$t0, 0x50($sp)
mtc1	$zero, $f18
lwc1	$f16, 0x4c($t0)
c.le.s	$f18, $f16
nop
bc1f	0x25c
nop
mtc1	$zero, $f4
lw	$t1, 0x50($sp)
swc1	$f4, 0x4c($t1)
lw	$t2, 0x58($sp)
andi	$t3, $t2, 2
beqz	$t3, 0x254
nop
lw	$t4, 0x50($sp)
lw	$t5, 0x64($t4)
beqz	$t5, 0x254
nop
lh	$t6, ($t5)
addiu	$at, $zero, 5
bne	$t6, $at, 0x254
nop
b	0x420
addiu	$v0, $zero, 4
b	0x420
move	$v0, $zero
lw	$t8, 0x50($sp)
addiu	$t7, $sp, 0x40
lwc1	$f6, 4($t7)
lwc1	$f8, 0x70($t8)
c.le.s	$f6, $f8
nop
bc1f	0x290
nop
lw	$t9, 0x50($sp)
lwc1	$f10, 0x70($t9)
swc1	$f10, 0x40($t9)
b	0x420
addiu	$v0, $zero, 1
addiu	$t0, $sp, 0x40
lwc1	$f16, 4($t0)
lw	$t1, 0x50($sp)
swc1	$f16, 0x40($t1)
b	0x420
addiu	$v0, $zero, 2
lw	$t2, 0x58($sp)
andi	$t3, $t2, 1
beqz	$t3, 0x324
nop
lw	$t4, 0x3c($sp)
bnez	$t4, 0x324
nop
lw	$t5, 0x38($sp)
beqz	$t5, 0x324
nop
lw	$a0, 0x50($sp)
lw	$a1, 0x38($sp)
lw	$a2, 0x54($sp)
jal	check_ledge_grab
addiu	$a3, $sp, 0x40
beqz	$v0, 0x2f4
nop
b	0x420
addiu	$v0, $zero, 3
lw	$a0, 0x50($sp)
addiu	$a1, $sp, 0x40
jal	vec3f_copy
addiu	$a0, $a0, 0x3c
lw	$t6, 0x30($sp)
lw	$t7, 0x50($sp)
sw	$t6, 0x68($t7)
lwc1	$f18, 0x28($sp)
lw	$t8, 0x50($sp)
swc1	$f18, 0x70($t8)
b	0x420
move	$v0, $zero
lw	$a0, 0x50($sp)
addiu	$a1, $sp, 0x40
jal	vec3f_copy
addiu	$a0, $a0, 0x3c
lw	$t9, 0x30($sp)
lw	$t0, 0x50($sp)
sw	$t9, 0x68($t0)
lwc1	$f4, 0x28($sp)
lw	$t1, 0x50($sp)
swc1	$f4, 0x70($t1)
lw	$t2, 0x3c($sp)
bnez	$t2, 0x364
nop
lw	$t3, 0x38($sp)
beqz	$t3, 0x410
nop
lw	$t4, 0x3c($sp)
beqz	$t4, 0x37c
nop
lw	$t5, 0x50($sp)
b	0x388
sw	$t4, 0x60($t5)
lw	$t6, 0x38($sp)
lw	$t7, 0x50($sp)
sw	$t6, 0x60($t7)
lw	$t8, 0x50($sp)
lw	$t9, 0x60($t8)
lwc1	$f12, 0x24($t9)
jal	atan2s
lwc1	$f14, 0x1c($t9)
lw	$t1, 0x50($sp)
sll	$s0, $v0, 0x10
sra	$t0, $s0, 0x10
lh	$t2, 0x2e($t1)
move	$s0, $t0
subu	$t3, $s0, $t2
sh	$t3, 0x4e($sp)
lw	$t4, 0x50($sp)
addiu	$at, $zero, 1
lw	$t5, 0x60($t4)
lh	$t6, ($t5)
bne	$t6, $at, 0x3d8
nop
b	0x420
addiu	$v0, $zero, 6
lh	$t7, 0x4e($sp)
slti	$at, $t7, -0x6000
bnez	$at, 0x3f4
nop
slti	$at, $t7, 0x6001
bnez	$at, 0x410
nop
lw	$t8, 0x50($sp)
lui	$at, 0x4000
lw	$t9, 4($t8)
or	$t0, $t9, $at
sw	$t0, 4($t8)
b	0x420
addiu	$v0, $zero, 2
b	0x420
move	$v0, $zero
b	0x420
nop
lw	$ra, 0x1c($sp)
lw	$s0, 0x18($sp)
addiu	$sp, $sp, 0x50
jr	$ra
nop