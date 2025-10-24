.n64

ROM_VERSION equ "J"

; game functions and variables

vec3f_copy equ 0x378800
vec3f_set equ 0x378840
vec3s_set equ 0x37897c
atan2s equ 0x37a9a8

resolve_and_return_wall_collisions equ 0x25181c
find_floor equ 0x381900
vec3f_find_ceil equ 0x2518d0
find_water_level equ 0x381ba0
perform_ground_quarter_step equ 0x2558dc
mario_get_terrain_sound_addend equ 0x25167c
perform_air_quarter_step equ 0x255e84
apply_gravity equ 0x2563f4
apply_vertical_wind equ 0x2567d0
check_ledge_grab equ 0x255c9c
apply_water_current equ 0x26ff90
perform_water_full_step equ 0x26fd94
f32_find_wall_collision equ 0x380de8

MarioPosHi equ 0x8034
MarioPosLoX equ 0x9e3c
MarioPosLoY equ 0x9e40
MarioPosLoZ equ 0x9e44
MarioAngleLo equ 0x9e2e

gWaterSurfacePseudoFloorHi equ 0x8033
gWaterSurfacePseudoFloorLo equ 0xcb98

.include "Previous Positions (Common).asm"

; build game functions

.create "./build/J/802C8440.bin", 0x00000000 ; +0x50
.include "./802C8F10 (apply_mario_platform_displacement).asm"
.close

.create "./build/J/8024A474.bin", 0x00000000; +0x1f8
.include "./8024A56C (check_instant_warp).asm"
.close

.create "./build/J/8024a07c.bin", 0x00000000; +0x50
.include "./8024A174 (warp_area).asm"
.close

.create "./build/J/8024bb38.bin", 0x00000000
.include "./8024BCD8 (lvl_init_or_update) (exact).asm"
.close

.create "./build/J/802558dc.bin", 0x00000000
.include "./80255B04 (perform_ground_quarter_step) (exact).asm"
.close

.create "./build/J/80255b60.bin", 0x00000000
.include "./80255D88 (perform_ground_step) (exact).asm"
.close

.create "./build/J/80256940.bin", 0x00000000
.include "./80256B24 (perform_air_step) (exact).asm"
.close

.create "./build/J/802503D4.bin", 0x00000000 ; +0x1bc
.include "./802505AC (mario_process_interactions).asm"
.close

.create "./build/J/80255e84.bin", 0x00000000
.include "./802560AC (perform_air_quarter_step) (exact).asm"
.close

.create "./build/J/8026FD94.bin", 0x00000000
.include "./80270304 (perform_water_full_step) (exact).asm"
.close

.create "./build/J/802703A8.bin", 0x00000000
.include "./80270918 (perform_water_step).asm"
.close

; additional hacks

; update_mario_geometry_inputs + 0x28
.create "./build/J/8025385C.bin", 0x00000000
jal do_wall_collisions_for_geometry_inputs
.close

; update_mario_geometry_inputs + 0x4C
.create "./build/J/80253880.bin", 0x00000000
jal do_wall_collisions_for_geometry_inputs
.close

; check_instant_warp + 0x40
.create "./build/J/8024A2BC.bin", 0x00000000
b 0x1b8
.close