.org 0x00 ; overwrite_mario_position_and_forward_angle
addiu	$sp, $sp, -0x18
sw	$ra, 0x14($sp)
jal	vec3f_set
nop
lui	$t0, BufferOffsetHi
lw	$t1, BufferOffsetLo($t0)
addiu	$t1, $t1, -0x10
sw	$t1, BufferOffsetLo($t0)
lui	$a0, MarioPosHi
jal	set_next_argument_position_and_mario_forward_angle
addiu	$a0, $a0, MarioPosLo
nop
lw	$ra, 0x14($sp)
jr	$ra
addiu	$sp, $sp, 0x18
nop

.org 0x40 ; set_next_mario_position_and_forward_angle
addiu	$sp, $sp, -0x18
sw	$ra, 0x14($sp)
lui	$a0, MarioPosHi
jal	set_next_argument_position_and_mario_forward_angle
addiu $a0, $a0, MarioPosLo
lw	$ra, 0x14($sp)
jr	$ra
addiu	$sp, $sp, 0x18

.org 0x80 ; set_next_argument_position_and_mario_forward_angle
lui	$t0, BufferOffsetHi
lw	$t1, BufferOffsetLo($t0)
addiu	$t2, $t1, 0x10
andi	$t2, $t2, 0x1f0
sw	$t2, BufferOffsetLo($t0)
add	$t0, $t0, $t1
lw	$t3, ($a0)
sw	$t3, 0x2f00($t0)
lw	$t3, 4($a0)
sw	$t3, 0x2f04($t0)
lw	$t3, 8($a0)
lui	$t2, MarioPosHi
lhu	$t2, MarioAngleLo($t2)
sw	$t2, 0x2f0c($t0)
jr	$ra
sw	$t3, 0x2f08($t0)

.org 0xc0 ; set_first_mario_position_and_angle
addiu	$sp, $sp, -0x18
sw	$ra, 0x14($sp)
lui	$t0, BufferOffsetHi
jal	set_next_mario_position_and_forward_angle
sw	$zero, BufferOffsetLo($t0)
lw	$ra, 0x14($sp)
jr	$ra
addiu	$sp, $sp, 0x18