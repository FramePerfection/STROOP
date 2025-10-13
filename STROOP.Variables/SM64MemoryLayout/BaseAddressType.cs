using STROOP.Core.Utilities;

namespace STROOP.Variables.SM64MemoryLayout;

public static class BaseAddressType
{
    static BaseAddressType() => StringSymbolAttribute.InitializeDeclaredStrings(typeof(BaseAddressType));

    [StringSymbol]
    public static string
        None,
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
