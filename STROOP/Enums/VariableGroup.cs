using STROOP.Core.Utilities;

namespace STROOP.Structs
{
    public static class VariableGroup
    {
        static VariableGroup() => StringSymbolAttribute.InitializeDeclaredStrings(typeof(VariableGroup));

        [StringSymbol]
        public static string
            Basic,
            Intermediate,
            Advanced,
            ObjectSpecific,
            Scheduler,
            Snow,
            WarpNode,
            NoGroup,
            Custom,
            ProcessGroup,
            Collision,
            Movement,
            Transformation,
            Coordinate,
            FloorCoordinate,
            ExtendedLevelBoundaries,
            HolpMario,
            HolpPoint,
            Trajectory,
            Point,
            Coin,
            Hacks,
            Rng,
            Self,
            QuarterFrameHack,
            GhostHack;
    };
}
