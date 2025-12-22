addiu	$sp, $sp, -0x28
sw	$ra, 0x1c($sp)
sw	$a0, 0x28($sp)
sw	$a1, 0x2c($sp)
sw	$s0, 0x18($sp)
jal	set_first_mario_position_and_angle
sw	$zero, 0x24($sp)
lh	$s0, 0x2a($sp)
beq	$zero, $s0, 0x38
addiu	$at, $zero, 1