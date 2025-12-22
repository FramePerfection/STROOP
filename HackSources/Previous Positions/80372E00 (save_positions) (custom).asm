set_next_argument_position_and_mario_forward_angle_offset:
set_next_argument_position_and_mario_forward_angle equ (save_positions_start + set_next_argument_position_and_mario_forward_angle_offset)
lui	$t0, BufferOffsetHi
lw	$t1, BufferOffsetLo($t0)
addiu	$t2, $t1, 0x10
sw	$t2, BufferOffsetLo($t0)
lui $t0, BufferStartHi
add	$t0, $t0, $t1
lw	$t3, ($a0)
sw	$t3, BufferStartLo + 0x0($t0)
lw	$t3, 4($a0)
sw	$t3, BufferStartLo + 0x4($t0)
lw	$t3, 8($a0)
sw	$t3, BufferStartLo + 0x8($t0)
lui	$t2, MarioPosHi
lhu	$t2, MarioAngleLo($t2)
sh	$t2, BufferStartLo + 0xE($t0) ; keeping this at 0xE for "compatibility"

; store the lower half of the global timer value when this position was written
; to let STROOP know not to display this position if it doesn't align with the expected global timer value
lui $t4, gGlobalTimerHi
lw $t4, gGlobalTimerLo($t4)
jr	$ra
sh $t4, BufferStartLo + 0xC($t0)

overwrite_mario_position_and_forward_angle_offset:
overwrite_mario_position_and_forward_angle equ (save_positions_start + overwrite_mario_position_and_forward_angle_offset)
addiu	$sp, $sp, -0x18
jal	vec3f_set
sw	$ra, 0x14($sp)
lui	$t0, BufferOffsetHi
lw	$t1, BufferOffsetLo($t0)
addiu	$t1, $t1, -0x10
sw	$t1, BufferOffsetLo($t0)
lui	$a0, MarioPosHi
jal	set_next_argument_position_and_mario_forward_angle
addiu	$a0, $a0, MarioPosLo
lw	$ra, 0x14($sp)
jr	$ra
addiu	$sp, $sp, 0x18

set_next_mario_position_and_forward_angle_offset:
set_next_mario_position_and_forward_angle equ (save_positions_start + set_next_mario_position_and_forward_angle_offset)
addiu	$sp, $sp, -0x18
sw	$ra, 0x14($sp)
lui	$a0, MarioPosHi
jal	set_next_argument_position_and_mario_forward_angle
addiu $a0, $a0, MarioPosLo
lw	$ra, 0x14($sp)
jr	$ra
addiu	$sp, $sp, 0x18

set_first_mario_position_and_angle_offset:
set_first_mario_position_and_angle equ (save_positions_start + set_first_mario_position_and_angle_offset)
addiu	$sp, $sp, -0x18
sw	$ra, 0x14($sp)
lui $t1, gGlobalTimerHi
lw  $t1, gGlobalTimerLo($t1)
andi $t1, $t1, 0x7f

; multiply by 0x17
sll $t2, $t1, 0x3
sll $t3, $t1, 0x4
addu $t4, $t2, $t3
subu $t1, $t4, $t1

sll $t1, $t1, 0x4
lui	$t0, BufferOffsetHi
jal	set_next_mario_position_and_forward_angle
sw	$t1, BufferOffsetLo($t0)
lw	$ra, 0x14($sp)
jr	$ra
addiu	$sp, $sp, 0x18
