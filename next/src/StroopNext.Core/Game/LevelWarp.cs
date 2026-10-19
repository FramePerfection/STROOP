using StroopNext.Memory;
using StroopNext.Session;

namespace StroopNext.Game;

public readonly record struct WarpProgress(short Level, int? Area, short PlayMode, byte WarpType, bool HasMario)
{
    public bool InFlight => PlayMode == LevelWarp.PlayModeChangeLevel || WarpType != LevelWarp.NotWarping;

    public bool Arrived(int level, int area) => Level == level && Area == area && !InFlight && HasMario;
}

// Makes the game leave the level by itself, the way a painting or a pipe does (level_update.c): sWarpDest holds where
// to go and sCurrPlayMode = PLAY_MODE_CHANGE_LEVEL makes play_mode_change_level() return that level number, which
// ends the level script; the main script then runs the destination level, whose init_level() sees sWarpDest and
// calls warp_level() -> init_mario_after_warp(), spawning Mario at the warp node (a spawn object of the area).
//
// The writes go in three steps, in this order: while the play mode is still normal a destination written first would
// make warp_area() spawn Mario at that node of the level he is in (a crash if it has none); in CHANGE_LEVEL mode
// without a destination play_mode_change_level() just waits.
public static class LevelWarp
{
    public const byte NotWarping = 0;
    public const byte WarpTypeChangeLevel = 1;
    public const short PlayModePaused = 2;
    public const short PlayModeChangeLevel = 4;
    public const int FirstAct = 1;
    public const int LastAct = 6;

    public static string? Unavailable(GameFrame? frame)
    {
        if (frame is null)
            return "Connect to an emulator first.";
        if (frame.Version is not { } version)
            return "This game's memory layout is not recognised (a rebuilt ROM hack?), so STROOP does not know where its level-change variables are.";
        if (Sm64Addresses.WarpDestination.For(version) is null)
            return $"The level-change variables are only known for the US and JP versions, not for {version}.";
        if (frame.Mario is not { MarioObject: not 0 })
            return "No level is running (title screen or menu).";
        return null;
    }

    public static WarpProgress? Read(GameFrame frame)
    {
        if (frame.Version is not { } version || Sm64Addresses.PlayMode.For(version) is not uint playMode
            || Sm64Addresses.WarpDestination.For(version) is not uint destination || frame.LevelIndex is not short level)
            return null;
        return new WarpProgress(level, frame.AreaIndex, frame.Ram.S16(playMode), frame.Ram.U8(destination), frame.Mario is { MarioObject: not 0 });
    }

    public static bool IsPaused(GameFrame frame) => Read(frame) is { PlayMode: PlayModePaused };

    // Empty when the game is not supported. Apply the patches one after the other.
    public static IReadOnlyList<RamPatch> Plan(GameFrame frame, GameLevel level, LevelArea area, int act)
        => Plan(frame, level.Id, area.Index, area.ArrivalNode, act);

    // A raw destination: a ROM hack's own level, area and warp node (HackLevels gives the valid ones).
    public static IReadOnlyList<RamPatch> Plan(GameFrame frame, int level, int area, int node, int act)
    {
        if (frame.Version is not { } version
            || Sm64Addresses.PlayMode.For(version) is not uint playMode
            || Sm64Addresses.TransitionTimer.For(version) is not uint timer
            || Sm64Addresses.TransitionUpdate.For(version) is not uint update
            || Sm64Addresses.WarpDestination.For(version) is not uint destination
            || Sm64Addresses.CurrentAct.For(version) is not uint currentAct)
            return [];

        // The act, and no act select screen (sWarpCheckpointActive makes lvl_set_current_level() skip it; where that flag
        // is unknown the game shows its own act select for the courses).
        var prepare = new RamPatch()
            .WriteS16(timer, 0)
            .WriteU32(update, 0)
            .WriteS16(currentAct, (short)Math.Clamp(act, FirstAct, LastAct));
        if (Sm64Addresses.WarpCheckpointActive.For(version) is uint checkpoint)
            prepare.WriteU8(checkpoint, 1);

        var begin = new RamPatch().WriteS16(playMode, PlayModeChangeLevel);

        var go = new RamPatch()
            .WriteU32(destination, (uint)(WarpTypeChangeLevel << 24 | (level & 0xFF) << 16 | (area & 0xFF) << 8 | (node & 0xFF)))
            .WriteU32(destination + 4, 0);

        return [prepare, begin, go];
    }
}
