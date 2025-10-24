; hack functions and variables

do_wall_collisions_for_ground_quarter_step equ 0x372f00
do_wall_collisions_for_geometry_inputs     equ 0x372f40
do_wall_collisions_for_air_quarter_step    equ 0x372f80
do_wall_collisions_for_water_full_step     equ 0x372fa0

save_positions_start equ 0x372e00

BufferOffsetHi equ 0x8037
BufferOffsetLo equ 0x2ffc

; each frame consists of 7 + 4 * 4 = 23 lines of 16 bytes each,
; we want 0x80 (128) previous positions, so this allocates 0xb800 bytes to the very end of extended RDRAM
BufferStartHi equ 0x807F
BufferStartLo equ 0x4800

; build hack functions

.create "./build/" + ROM_VERSION + "/80372e00.bin", 0x00000000
.include "./80372E00 (save_positions) (custom).asm"
.close

.create "./build/" + ROM_VERSION + "/80372f00.bin", 0x00000000
.include "./80372F00 (do_wall_collisions) (custom).asm"
.close
