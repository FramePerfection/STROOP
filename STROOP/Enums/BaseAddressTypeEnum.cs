namespace STROOP.Structs
{
    public static class BaseAddressType
    {
        static BaseAddressType() => Utilities.StringUtilities.InitializeDeclaredStrings(typeof(BaseAddressType));

        [Utilities.DeclaredString]
        public static string
            None,
            Absolute,
            Relative,
            Mario,
            MarioObj,
            Camera,
            CameraStruct,
            LakituStruct,
            CameraModeInfo,
            CameraModeTransition,
            CameraSettings,
            File,
            MainSave,
            Object,
            ProcessGroup,
            Coin,
            Triangle,
            Area,
            GfxNode;
    };
}
