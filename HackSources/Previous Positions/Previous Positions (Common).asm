; hack functions and variables

overwrite_mario_position_and_forward_angle          equ 0x372e00
set_next_mario_position_and_forward_angle           equ 0x372e40
set_next_argument_position_and_mario_forward_angle  equ 0x372e80
set_first_mario_position_and_angle                  equ 0x372ec0

do_wall_collisions_for_ground_quarter_step equ 0x373380
do_wall_collisions_for_geometry_inputs     equ 0x3733c0
do_wall_collisions_for_air_quarter_step    equ 0x373400
do_wall_collisions_for_water_full_step     equ 0x373420

BufferOffsetHi equ 0x8037
BufferOffsetLo equ 0x2e3c

; build hack functions

.create "./build/" + ROM_VERSION + "/80372e00.bin", 0x00000000
.include "./80372E00 (save_positions) (custom).asm"
.close

.create "./build/" + ROM_VERSION + "/80373380.bin", 0x00000000
.include "./80373380 (do_wall_collisions) (custom).asm"
.close