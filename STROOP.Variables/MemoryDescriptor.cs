using STROOP.Variables.SM64MemoryLayout;
using STROOP.Variables.Utilities;

namespace STROOP.Variables;

public class MemoryDescriptor
{
    public readonly Type ClrType;
    public readonly int? ByteCount;
    public readonly bool? SignedType;

    public readonly string BaseAddressType;

    public readonly uint? OffsetUS;
    public readonly uint? OffsetJP;
    public readonly uint? OffsetSH;
    public readonly uint? OffsetEU;
    public readonly uint? OffsetDefault;

    public readonly uint? Mask;
    public readonly int? Shift;
    public readonly bool HandleMapping;

    public int? NibbleCount => ByteCount.HasValue ? (int?)(ByteCount.Value * 2) : null;

    public uint Offset
    {
        get
        {
            if (OffsetUS.HasValue || OffsetJP.HasValue || OffsetSH.HasValue || OffsetEU.HasValue)
            {
                if (HandleMapping)
                    return RomVersionConfig.SwitchMap(
                        OffsetUS ?? 0,
                        OffsetJP ?? 0,
                        OffsetSH ?? 0,
                        OffsetEU ?? 0);
                else
                    return RomVersionConfig.SwitchOnly(
                        OffsetUS ?? 0,
                        OffsetJP ?? 0,
                        OffsetSH ?? 0,
                        OffsetEU ?? 0);
            }

            if (OffsetDefault.HasValue) return OffsetDefault.Value;
            return 0;
        }
    }

    public MemoryDescriptor(Type clrTypeName, string baseAddress, uint offset, uint? mask = null, int? shift = null)
        : this(clrTypeName, baseAddress, null, null, null, null, offset, mask, shift, false)
    {
    }

    public MemoryDescriptor(Type clrType, string baseAddressType,
        uint? offsetUS, uint? offsetJP, uint? offsetSH, uint? offsetEU, uint? offsetDefault, uint? mask, int? shift, bool handleMapping)
    {
        if (offsetDefault.HasValue && (offsetUS.HasValue || offsetJP.HasValue || offsetSH.HasValue || offsetEU.HasValue))
        {
            throw new ArgumentOutOfRangeException("Can't have both a default offset value and a rom-specific offset value");
        }

        BaseAddressType = baseAddressType;

        OffsetUS = offsetUS;
        OffsetJP = offsetJP;
        OffsetSH = offsetSH;
        OffsetEU = offsetEU;
        OffsetDefault = offsetDefault;

        ClrType = clrType;
        ByteCount = TypeUtilities.TypeSize[ClrType];
        SignedType = TypeUtilities.TypeSign[ClrType];

        Mask = mask;
        Shift = shift;
        HandleMapping = handleMapping;
    }

    public IMemoryVariable CreateVariable(string wrapper = "Number")
        => (IMemoryVariable)
            typeof(MemoryVariable<>)
                .MakeGenericType(ClrType)
                .GetConstructor(new Type[] { typeof(MemoryDescriptor), typeof(string) })
                .Invoke(new object[] { this, wrapper });

    public List<uint> GetAddressList()
        => VariableUtilities.GetBaseAddresses(BaseAddressType).Select(baseAddress => baseAddress + Offset).ToList();
}
