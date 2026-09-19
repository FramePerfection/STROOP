.n64
lui t0, gCurGraphNodeObjectHi
lw t0, gCurGraphNodeObjectLo (t0)
lui at, MarioObjectAddrHi
lw at, MarioObjectAddrLo (at)
bne t0, at, @@IsGhost

; render Mario as usual
lui t9, gBodyStatesAddrHi
lh t2, gBodyStatesAddrLo + 0x8 (t9)
srl t2, t2, 0x8
beq r0, r0, @@RETURN
sh t2, 0x1e (a1)

@@IsGhost:
; apply the chosen effect from the ghost node
lb t2, 0x61 (t0)
sh t2, 0x1e (a1)

@@RETURN:
jr ra
or v0, r0, r0