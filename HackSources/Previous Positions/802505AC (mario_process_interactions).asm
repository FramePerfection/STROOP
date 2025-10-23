lui	$a0, MarioPosHi
jal	set_next_argument_position_and_mario_forward_angle
addiu	$a0, $a0, MarioPosLoX
lw	$s0, 0x18($sp)
lw	$ra, 0x1c($sp)
jr	$ra
addiu	$sp, $sp, 0x30