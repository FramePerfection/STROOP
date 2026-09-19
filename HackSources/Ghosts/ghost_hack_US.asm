
set_mario_animation equ 0x802509B8
make_gfx_mario_alpha equ 0x80276F90

MarioObjectAddrHi equ 0x8036
MarioObjectAddrLo equ 0x1158

GlobalTimerAddrHi equ 0x8033
GlobalTimerAddrLo equ 0xD5D4

gBodyStatesAddrHi equ 0x8034
gBodyStatesAddrLo equ 0xB3B0

gCurGraphNodeObjectHi equ 0x8033
gCurGraphNodeObjectLo equ 0xDF00

gMarioStatesHi equ 0x8033
gMarioStatesLo equ 0xB170

alloc_display_list equ 0x80278f2c


.create "./build/US_80408000_ghost_loop.bin", 0x00000000
.include "./ghost_loop.asm"
.close

.create "./build/US_802770A4_geo_mirror_mario_set_alpha.bin", 0x00000000
.include "./geo_mirror_mario_set_alpha.asm"
.close

.create "./build/US_802776D8_geo_switch_mario_cap_effect.bin", 0x00000000
.include "./geo_switch_mario_cap_effect.asm"
.close

; this is in the middle of geo_switch_mario_hand_grab_pos and prevents ghosts from updating held objects
.create "./build/US_80277988_avoid_holp_desyncs.bin", 0x00000000
.include "./avoid_holp_desyncs.asm"
.close

.create "./build/US_80408200_gfx_generate_colored_hats.bin", 0x00000000
.include "./gfx_generate_colored_hats.asm"
.close

.include "./export_moving_code_references.asm"
