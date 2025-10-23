


.org 0x00 ; do_wall_collisions_for_ground_quarter_step
move	$gp, $ra
jal	resolve_and_return_wall_collisions
nop
jal	set_next_argument_position_and_mario_forward_angle
lw	$a0, 0x44($sp)
jr	$gp
nop

.org 0x40 ; do_wall_collisions_for_geometry_inputs
move	$gp, $ra
jal	0x380de8
nop
jal	set_next_mario_position_and_forward_angle
nop
jr	$gp
nop

.org 0x80 ; do_wall_collisions_for_air_quarter_step
move	$gp, $ra
jal	resolve_and_return_wall_collisions
nop
jal	set_next_argument_position_and_mario_forward_angle
addiu	$a0, $sp, 0x40
jr	$gp
nop

.org 0xA0 ; do_wall_collisions_for_water_full_step
move	$gp, $ra
jal	resolve_and_return_wall_collisions
nop
jal	set_next_argument_position_and_mario_forward_angle
lw	$a0, 0x34($sp)
jal	set_next_argument_position_and_mario_forward_angle
lw	$a0, 0x34($sp)
jr	$gp
nop