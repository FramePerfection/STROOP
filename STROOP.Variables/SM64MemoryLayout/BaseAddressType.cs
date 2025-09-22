using STROOP.Core.Utilities;

namespace STROOP.Structs
{
    public static class BaseAddressType
    {
        static BaseAddressType() => StringSymbolAttribute.InitializeDeclaredStrings(typeof(BaseAddressType));

        [StringSymbol]
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
