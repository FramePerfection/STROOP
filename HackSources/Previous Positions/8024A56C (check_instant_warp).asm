lui	$a0, MarioPosHi
jal	set_next_argument_position_and_mario_forward_angle
addiu	$a0, $a0, MarioPosLoX
lw	$ra, 0x14($sp)
jr	$ra
addiu	$sp, $sp, 0x28