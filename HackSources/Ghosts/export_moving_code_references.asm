.create "./build/DynamicOffsets.bin", 0x00000000
.word orga(FirstAnimationBufferAddrHi_LUI_1)
.word orga(GhostBaseHi_LUI_PLUS_1)
.word orga(GhostBaseHi_LUI_1)
.word orga(GhostBaseHi_LUI_2)
.word orga(GhostBaseHi_LUI_3)
.word orga(GhostBaseHi_LUI_4)
.close
