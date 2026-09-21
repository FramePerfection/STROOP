
set_mario_animation equ 0x802507E8
make_gfx_mario_alpha equ 0x802769E0

MarioObjectAddrHi equ 0x8036
MarioObjectAddrLo equ 0xFDE8

GlobalTimerAddrHi equ 0x8033
GlobalTimerAddrLo equ 0xC694

gBodyStatesAddrHi equ 0x8034
gBodyStatesAddrLo equ 0xA040

gCurGraphNodeObjectHi equ 0x8033
gCurGraphNodeObjectLo equ 0xCFA0

gMarioStatesHi equ 0x8033
gMarioStatesLo equ 0x9e00

alloc_display_list equ 0x8027897c


.create "./build/JP_80408000_ghost_loop.bin", 0x00000000
.include "./ghost_loop.asm"
.Close

.create "./build/JP_80276AF4_geo_mirror_mario_set_alpha.bin", 0x00000000
.include "./geo_mirror_mario_set_alpha.asm"
.close

.create "./build/JP_80277128_geo_switch_mario_cap_effect.bin", 0x00000000
.include "./geo_switch_mario_cap_effect.asm"
.close

; this is in the middle of geo_switch_mario_hand_grab_pos and prevents ghosts from updating held objects
.create "./build/JP_802773D8_avoid_holp_desyncs.bin", 0x00000000
.include "./avoid_holp_desyncs.asm"
.close

.create "./build/JP_80408200_gfx_generate_colored_hats.bin", 0x00000000
.include "./gfx_generate_colored_hats.asm"
.close

.include "./export_moving_code_references.asm"
