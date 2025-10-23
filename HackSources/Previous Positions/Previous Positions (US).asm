
.n64

; game functions and variables

vec3f_copy equ 0x378800
vec3f_set equ 0x378840

resolve_and_return_wall_collisions equ 0x251a48

MarioPosHi equ 0x8034
MarioPosLoX equ 0xb1ac
MarioPosLoY equ 0xb1b0
MarioPosLoZ equ 0xb1b4
MarioAngleLo equ 0xb19e

; hack functions and variables

overwrite_mario_position_and_forward_angle          equ 0x372e00
set_next_mario_position_and_forward_angle           equ 0x372e40
set_next_argument_position_and_mario_forward_angle  equ 0x372e80
set_first_mario_position_and_angle                  equ 0x372ec0

do_wall_collisions_for_ground_quarter_step equ 0x373380
do_wall_collisions_for_geometry_inputs            equ 0x3733c0
do_wall_collisions_for_air_quarter_step    equ 0x373400
do_wall_collisions_for_water_full_step     equ 0x373420

BufferOffsetHi equ 0x8037
BufferOffsetLo equ 0x2e3c

; build hack functions

.create "./build/US/80372E00.bin", 0x00000000
.include "./80372E00 (save_positions) (custom).asm"
.close

.create "./build/US/80373380.bin", 0x00000000
.include "./80373380 (do_wall_collisions) (custom).asm"
.close

; build game functions

.create "./build/US/802C8F10.bin", 0x00000000
.include "./802C8F10 (apply_mario_platform_displacement).asm"
.close

.create "./build/US/8024A56C.bin", 0x00000000
.include "./8024A56C (check_instant_warp).asm"
.close

.create "./build/US/8024A174.bin", 0x00000000
.include "./8024A174 (warp_area).asm"
.close

.create "./build/US/8024BCD8.bin", 0x00000000
.include "./8024BCD8 (lvl_init_or_update) (exact).asm"
.close

.create "./build/US/80255B04.bin", 0x00000000
.include "./80255B04 (perform_ground_quarter_step) (exact).asm"
.close

.create "./build/US/80255D88.bin", 0x00000000
.include "./80255D88 (perform_ground_step) (exact).asm"
.close

.create "./build/US/80256b24.bin", 0x00000000
.include "./80256B24 (perform_air_step) (exact).asm"
.close

.create "./build/US/802505AC.bin", 0x00000000
.include "./802505AC (mario_process_interactions).asm"
.close

.create "./build/US/802560AC.bin", 0x00000000
.include "./802560AC (perform_air_quarter_step) (exact).asm"
.close

.create "./build/US/80270304.bin", 0x00000000
.include "./80270304 (perform_water_full_step) (exact).asm"
.close

.create "./build/US/80270918.bin", 0x00000000
.include "./80270918 (perform_water_step).asm"
.close

; additional hacks

; update_mario_geometry_inputs + 0x28
.create "./build/US/80253A88.bin", 0x00000000
jal do_wall_collisions_for_geometry_inputs
.close

; update_mario_geometry_inputs + 0x4C
.create "./build/US/80253AAC.bin", 0x00000000
jal do_wall_collisions_for_geometry_inputs
.close

; check_instant_warp + 0x40
.create "./build/US/8024A3B4.bin", 0x00000000
b 0x1b8
.close