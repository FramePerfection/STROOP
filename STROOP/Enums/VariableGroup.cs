namespace STROOP.Structs
{
    public static class VariableGroup
    {
        static VariableGroup() => Utilities.StringUtilities.InitializeDeclaredStrings(typeof(VariableGroup));

        [Utilities.DeclaredString]
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
