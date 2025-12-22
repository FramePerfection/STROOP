# Previous Positions Hack
This hack is designed to hook into the game's update logic to store mid-frame information about Mario's movement that can then be displayed using the "Previous Positions" tracker in STROOP's Map Tab.

The hack is built to work with the NTSC and JP version of the game. Other versions are currently unsupported.

This version of the hack tracks the positions of up to 128 subsequent frames described in the [MARIO_IMAGES array defined in MapPreviousPositionsObject.cs](../../STROOP/Tabs/MapTab/MapObjects/MapPreviousPositionsObject.cs#L26).

Activating the hack restores the vanilla behavior for several collision related functions and may thus desync on ROM hacks that modify this behavior.

# How to build
The process of "building" the hack shall result in the `Previous Positions (US).hck` and `Previous Positions (J).hck` files respectively.
`.hck` files are really just text files with a specific format, in which each line represents a hexadecimal RAM location,
followed by a colon and then the payload to inject at that location as a hexadecimal representation, e.g.
```
8027ABD8: 0C 10 20 00
80276BEC: 34 0C 00 01
```
Download [armips](https://github.com/Kingcom/armips) and add it to your PATH or place the executable next to `create-hack-file.py`.
Then run `create-hack-file.py` (must be in the current working directory) with [any modern python](https://python.org/downloads) version (I didn't test what the minimum requirement is, but anything beyond 3.0 should work).

# How it works
This hack replaces several movement related functions with functionally identical versions that call custom injected functions near the beginning of extended RAM that store Mario's position (or where he would be if the movement would be successful).
The information is stored in a cyclic buffer near the end of extended RAM starting at 0x807F4800, with the information for each frame being indexed as `gGlobalTimer % 128` in that buffer.
STROOP will read out the current value of `gGlobalTimer` and show up to 128 frames "into the past", assuming that the information for those frames were written during the same continuous section of gameplay. Loading savestates or modifying the global timer otherwise may yield inconsistent data.