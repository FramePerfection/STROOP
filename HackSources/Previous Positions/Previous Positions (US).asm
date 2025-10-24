
.n64

ROM_VERSION equ "US"

; game functions and variables

vec3f_copy equ 0x378800
vec3f_set equ 0x378840
vec3s_set equ 0x37897c
atan2s equ 0x37a9a8

resolve_and_return_wall_collisions equ 0x251a48
find_floor equ 0x381900
vec3f_find_ceil equ 0x251afc
find_water_level equ 0x381ba0
perform_ground_quarter_step equ 0x255b04
mario_get_terrain_sound_addend equ 0x2518a8
perform_air_quarter_step equ 0x2560ac
apply_gravity equ 0x25661c
apply_vertical_wind equ 0x2569f8
check_ledge_grab equ 0x255ec4
apply_water_current equ 0x270500
perform_water_full_step equ 0x270304
f32_find_wall_collision equ 0x380de8

MarioPosHi equ 0x8034
MarioPosLo equ 0xb1ac
MarioAngleLo equ 0xb19e

gWaterSurfacePseudoFloorHi equ 0x8033
gWaterSurfacePseudoFloorLo equ 0xdaf8

.include "Previous Positions (Common).asm"

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