using STROOP.Core;
using STROOP.Utilities;
using STROOP.Variables.Utilities;

namespace STROOP.Variables.Formatting;

public static class MemoryDescriptorExtensions
{
    public static string GetTypeDescription(this MemoryDescriptor memoryDescriptor)
    {
        string maskString = "";
        if (memoryDescriptor.Mask != null)
            maskString = " with mask " + HexUtilities.FormatValue(memoryDescriptor.Mask.Value, memoryDescriptor.NibbleCount.Value);

        string shiftString = "";
        if (memoryDescriptor.Shift != null)
            shiftString = " right shifted by " + memoryDescriptor.Shift.Value;

        string byteCountString = "";
        if (memoryDescriptor.ByteCount.HasValue)
        {
            string pluralSuffix = memoryDescriptor.ByteCount.Value == 1 ? "" : "s";
            byteCountString = $" ({memoryDescriptor.ByteCount.Value} byte{pluralSuffix})";
        }

        return TypeUtilities.TypeToString[memoryDescriptor.ClrType] + maskString + shiftString + byteCountString;
    }

    public static string GetBaseTypeOffsetDescription(this MemoryDescriptor memoryDescriptor)
        => $"{memoryDescriptor.BaseAddressType} + {HexUtilities.FormatValue(memoryDescriptor.Offset)}";

    public static string GetProcessAddressListString(this MemoryDescriptor memoryDescriptor)
    {
        List<uint> addressList = memoryDescriptor.GetAddressList();
        if (addressList.Count == 0)
            return "(none)";

        List<ulong> processAddressList = memoryDescriptor.GetProcessAddressList().ConvertAll(address => address.ToUInt64());
        List<string> stringList = processAddressList.ConvertAll(address => HexUtilities.FormatValue(address, address > 0xFFFFFFFFU ? 16 : 8));
        return string.Join(", ", stringList);
    }

    public static string GetRamAddressListString(this MemoryDescriptor memoryDescriptor, bool addressArea = true)
    {
        List<uint> addressList = memoryDescriptor.GetAddressList();
        if (addressList.Count == 0) return "(none)";
        List<uint> ramAddressList = memoryDescriptor.GetRamAddressList(addressArea);
        List<string> stringList = ramAddressList.ConvertAll(address => HexUtilities.FormatValue(address, 8));
        return string.Join(", ", stringList);
    }

    public static string GetBaseAddressListString(this MemoryDescriptor memoryDescriptor)
    {
        List<uint> baseAddresses = WatchVariableUtilities.GetBaseAddresses(memoryDescriptor.BaseAddressType).ToList();
        if (baseAddresses.Count == 0)
            return "(none)";
        List<string> baseAddressesString = baseAddresses.ConvertAll(address => HexUtilities.FormatValue(address, 8));
        return string.Join(",", baseAddressesString);
    }

    private static List<UIntPtr> GetProcessAddressList(this MemoryDescriptor memoryDescriptor)
    {
        List<uint> ramAddressList = memoryDescriptor.GetRamAddressList(false);
        return ramAddressList.ConvertAll(address => ProcessStream.Instance.GetAbsoluteAddress(address, memoryDescriptor.ByteCount.Value));
    }

    private static List<uint> GetRamAddressList(this MemoryDescriptor memoryDescriptor, bool addressArea = true)
        => memoryDescriptor.GetAddressList().ConvertAll(addr => addressArea ? addr | 0x80000000 : addr & 0x0FFFFFFF);
}
