using STROOP.Core;
using STROOP.Core.Utilities;
using STROOP.Variables.Views;
using System.Reflection;
using System.Xml.Linq;

namespace STROOP.Variables;

public class NamedVariableCollection
{
    // HACK: delegate the variable rounding to the view for now with this
    public delegate bool SetVariableValueFunc(ProcessStream processStream, Type type, object value, uint address, uint? mask = null, int? shift = null);

    public static SetVariableValueFunc SetVariableValue = null!;

    private static IEnumerable<T> GetValues<T>(DescribedMemoryState memoryState) where T : struct, IConvertible
        => memoryState.GetAddressList().ConvertAll(address => (T)ProcessStream.Instance.GetValue(
            typeof(T),
            address,
            false,
            memoryState.descriptor.Mask,
            memoryState.descriptor.Shift
        ));

    private static IEnumerable<bool> SetAll<T>(DescribedMemoryState memoryState, T value) where T : struct, IConvertible
        => memoryState.GetAddressList().Select(address => SetVariableValue(
            ProcessStream.Instance,
            typeof(T),
            value,
            address,
            memoryState.descriptor.Mask,
            memoryState.descriptor.Shift
        )).ToArray();

    public interface IMemoryDescriptorView : IVariableView
    {
        MemoryDescriptor memoryDescriptor { get; }
        DescribedMemoryState describedMemoryState { get; }
    }

    public class CustomView : IVariableView
    {
        public Action ValueSet { get; set; }
        public Action OnDelete { get; set; }
        public string Name { get; set; }
        public string Subclass { get; }
        public Type ClrType { get; }

        public string Color
        {
            set => SetValueByKey(CommonViewProperties.color, value);
        }

        public string Display
        {
            set => SetValueByKey(CommonViewProperties.display, value);
        }

        public int DislpayPriority { get; }

        private Dictionary<string, string> keyedValues = new Dictionary<string, string>();

        public CustomView(string subclass, Type clrType)
        {
            Subclass = subclass;
            ClrType = clrType;
        }

        public virtual string GetValueByKey(string key)
        {
            if (keyedValues.TryGetValue(key, out string? result))
                return result;
            return null;
        }

        public virtual bool SetValueByKey(string key, object value)
        {
            keyedValues[key] = value.ToString();
            return true;
        }
    }

    public class CustomView<T> : CustomView, IVariableView<T>
    {
        public GetterFunction<T> _getterFunction { get; set; }
        public SetterFunction<T> _setterFunction { get; set; }

        public CustomView(string subclass) : base(subclass, typeof(T))
        {
            _getterFunction = SpecialVariableDefaults<T>.DEFAULT_GETTER;
            _setterFunction = SpecialVariableDefaults<T>.DEFAULT_SETTER;
        }
    }

    public class MemoryDescriptorView : CustomView, IMemoryDescriptorView
    {
        public MemoryDescriptor memoryDescriptor { get; }
        public DescribedMemoryState describedMemoryState { get; }

        public MemoryDescriptorView(string subclass, MemoryDescriptor memoryDescriptor)
            : base(subclass, memoryDescriptor.ClrType)
        {
            this.memoryDescriptor = memoryDescriptor;
            describedMemoryState = new DescribedMemoryState(memoryDescriptor);
        }
    }

    public class MemoryDescriptorView<T> : MemoryDescriptorView, IVariableView<T> where T : struct, IConvertible
    {
        public MemoryDescriptorView(string subclass, MemoryDescriptor memoryDescriptor)
            : base(subclass, memoryDescriptor)
        {
            _getterFunction = () => GetValues<T>(describedMemoryState);
            _setterFunction = (T value) => SetAll(describedMemoryState, value);
        }

        public GetterFunction<T> _getterFunction { get; private set; }
        public SetterFunction<T> _setterFunction { get; private set; }
    }

    public class XmlMemoryView : IMemoryDescriptorView
    {
        public Action ValueSet { get; set; }
        public Action OnDelete { get; set; }
        public string Name { get; private set; }
        public string Subclass { get; }
        int IVariableView.DislpayPriority => 0;
        public Type ClrType => describedMemoryState.descriptor.ClrType;

        private readonly XElement xElement;
        public MemoryDescriptor memoryDescriptor { get; }
        public DescribedMemoryState describedMemoryState { get; }

        public XmlMemoryView(MemoryDescriptor memoryDescriptor, XElement xElement)
        {
            this.xElement = xElement;
            this.memoryDescriptor = memoryDescriptor;
            describedMemoryState = new DescribedMemoryState(memoryDescriptor);
            Name = xElement.Value;
            string? subclassName = xElement.Attribute("subclass")?.Value;
            Subclass = subclassName != null
                ? (string?)typeof(WatchVariableSubclass).GetFields( BindingFlags.Static | BindingFlags.Public)
                    .SingleOrDefault(x => x.Name == subclassName)?.GetValue(null) ?? WatchVariableSubclass.Number
                : WatchVariableSubclass.Number;
        }

        public string GetValueByKey(string key) => xElement.Attribute(key)?.Value ?? null;

        public bool SetValueByKey(string key, object value)
        {
            xElement.SetAttributeValue(XName.Get(key), value.ToString());
            return true;
        }

        public XElement GetXml() => xElement;
    }

    public class XmlMemoryView<T> : XmlMemoryView, IVariableView<T> where T : struct, IConvertible
    {
        public XmlMemoryView(MemoryDescriptor memoryDescriptor, XElement xElement)
            : base(memoryDescriptor, xElement)
        {
            _getterFunction = () => GetValues<T>(describedMemoryState);
            _setterFunction = (T value) => SetAll(describedMemoryState, value);
        }

        public GetterFunction<T> _getterFunction { get; private set; }
        public SetterFunction<T> _setterFunction { get; private set; }
    }
}
