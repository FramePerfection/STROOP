using System;
using System.Collections.Generic;
using System.Linq;
using STROOP.Utilities;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using System.Xml.Linq;

namespace STROOP.Core.Variables
{
    public class MemoryDescriptor
    {
        public static (MemoryDescriptor descriptor, NamedVariableCollection.XmlMemoryView view) FromXml(XElement element)
        {
            string typeName = (element.Attribute(XName.Get("type"))?.Value);
            string baseAddressType = element.Attribute(XName.Get("base")).Value;
            uint? offsetUS = ParsingUtilities.ParseHexNullable(element.Attribute(XName.Get("offsetUS"))?.Value);
            uint? offsetJP = ParsingUtilities.ParseHexNullable(element.Attribute(XName.Get("offsetJP"))?.Value);
            uint? offsetSH = ParsingUtilities.ParseHexNullable(element.Attribute(XName.Get("offsetSH"))?.Value);
            uint? offsetEU = ParsingUtilities.ParseHexNullable(element.Attribute(XName.Get("offsetEU"))?.Value);
            uint? offsetDefault = ParsingUtilities.ParseHexNullable(element.Attribute(XName.Get("offset"))?.Value);
            uint? mask = element.Attribute(XName.Get("mask")) != null ? ParsingUtilities.ParseHexNullable(element.Attribute(XName.Get("mask")).Value) : null;
            int? shift = element.Attribute(XName.Get("shift")) != null ? int.Parse(element.Attribute(XName.Get("shift")).Value) : (int?)null;
            bool handleMapping = (element.Attribute(XName.Get("handleMapping")) != null) ? bool.Parse(element.Attribute(XName.Get("handleMapping")).Value) : true;

            var memoryDescriptor = new MemoryDescriptor(TypeUtilities.StringToType[typeName], baseAddressType, offsetUS, offsetJP, offsetSH, offsetEU, offsetDefault, mask, shift, handleMapping);
            var view = (NamedVariableCollection.XmlMemoryView)
                typeof(NamedVariableCollection.XmlMemoryView<>)
                    .MakeGenericType(memoryDescriptor.MemoryType)
                    .GetConstructor(new Type[] { typeof(MemoryDescriptor), typeof(XElement) })
                    .Invoke(new object[] { memoryDescriptor, element });
            return (memoryDescriptor, view);
        }

        public readonly Type MemoryType;
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

        public bool UseAbsoluteAddressing
        {
            get => BaseAddressType == Structs.BaseAddressType.Absolute;
        }

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

        public List<uint> GetBaseAddressList() => WatchVariableUtilities.GetBaseAddresses(BaseAddressType).ToList();

        public List<uint> GetAddressList(List<uint> addresses = null)
        {
            var baseAddresses = addresses ?? GetBaseAddressList();
            uint offset = Offset;
            return baseAddresses.ConvertAll(baseAddress => baseAddress + offset).ToList();
        }

        public MemoryDescriptor(Type memoryTypeName, string baseAddress, uint offset, uint? mask = null, int? shift = null)
            : this(memoryTypeName, baseAddress, null, null, null, null, offset, mask, shift, false)
        {
        }

        public NamedVariableCollection.MemoryDescriptorView CreateView(string wrapper = "Number")
            => (NamedVariableCollection.MemoryDescriptorView)
                typeof(NamedVariableCollection.MemoryDescriptorView<>)
                    .MakeGenericType(MemoryType)
                    .GetConstructor(new Type[] { typeof(MemoryDescriptor), typeof(string) })
                    .Invoke(new object[] { this, wrapper });

        private MemoryDescriptor(Type memoryType, string baseAddressType,
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

            MemoryType = memoryType;
            ByteCount = TypeUtilities.TypeSize[MemoryType];
            SignedType = TypeUtilities.TypeSign[MemoryType];

            Mask = mask;
            Shift = shift;
            HandleMapping = handleMapping;
        }

        public string GetTypeDescription()
        {
            string maskString = "";
            if (Mask != null)
            {
                maskString = " with mask " + HexUtilities.FormatValue(Mask.Value, NibbleCount.Value);
            }

            string shiftString = "";
            if (Shift != null)
            {
                shiftString = " right shifted by " + Shift.Value;
            }

            string byteCountString = "";
            if (ByteCount.HasValue)
            {
                string pluralSuffix = ByteCount.Value == 1 ? "" : "s";
                byteCountString = string.Format(" ({0} byte{1})", ByteCount.Value, pluralSuffix);
            }

            return TypeUtilities.TypeToString[MemoryType] + maskString + shiftString + byteCountString;
        }

        public string GetBaseTypeOffsetDescription() => $"{BaseAddressType} + {HexUtilities.FormatValue(Offset)}";

        public string GetProcessAddressListString(List<uint> addresses = null)
        {
            List<uint> addressList = GetAddressList(addresses);
            if (addressList.Count == 0) return "(none)";
            List<ulong> processAddressList = GetProcessAddressList(addresses).ConvertAll(address => address.ToUInt64());
            List<string> stringList = processAddressList.ConvertAll(address => HexUtilities.FormatValue(address, address > 0xFFFFFFFFU ? 16 : 8));
            return string.Join(", ", stringList);
        }

        private List<UIntPtr> GetProcessAddressList(List<uint> addresses = null)
        {
            List<uint> ramAddressList = GetRamAddressList(false, addresses);
            return ramAddressList.ConvertAll(address => Config.Stream.GetAbsoluteAddress(address, ByteCount.Value));
        }

        public string GetRamAddressListString(bool addressArea = true, List<uint> addresses = null)
        {
            List<uint> addressList = GetAddressList(addresses);
            if (addressList.Count == 0) return "(none)";
            List<uint> ramAddressList = GetRamAddressList(addressArea, addresses);
            List<string> stringList = ramAddressList.ConvertAll(address => HexUtilities.FormatValue(address, 8));
            return string.Join(", ", stringList);
        }

        private List<uint> GetRamAddressList(bool addressArea = true, List<uint> addresses = null)
        {
            List<uint> addressList = GetAddressList(addresses);
            return addressList.ConvertAll(address => GetRamAddress(address, addressArea));
        }

        private uint GetRamAddress(uint addr, bool addressArea = true)
        {
            UIntPtr addressPtr = new UIntPtr(addr);
            uint address;

            if (UseAbsoluteAddressing)
                address = EndiannessUtilities.SwapAddressEndianness(
                    Config.Stream.GetRelativeAddress(addressPtr, ByteCount.Value), ByteCount.Value);
            else
                address = addressPtr.ToUInt32();

            return addressArea ? address | 0x80000000 : address & 0x0FFFFFFF;
        }

        public string GetBaseAddressListString(List<uint> addresses = null)
        {
            var baseAddresses = addresses ?? GetBaseAddressList();
            if (baseAddresses.Count == 0) return "(none)";
            List<string> baseAddressesString = baseAddresses.ConvertAll(address => HexUtilities.FormatValue(address, 8));
            return string.Join(",", baseAddressesString);
        }
    }
}
