using STROOP.Core;
using STROOP.Core.Utilities;
using System.Reflection;
using System.Xml.Linq;

namespace STROOP.Variables
{
    public class NamedVariableCollection
    {
        // HACK: delegate the variable rounding to the view for now with this
        public delegate bool SetVariableValueFunc(ProcessStream processStream, Type type, object value, uint address, bool absoluteAddress = false, uint? mask = null, int? shift = null);

        public static SetVariableValueFunc SetVariableValue = null!;

        private static IEnumerable<T> GetValues<T>(DescribedMemoryState memoryState) where T : struct, IConvertible
            => memoryState.GetAddressList().ConvertAll(address => (T)ProcessStream.Instance.GetValue(
                typeof(T),
                address,
                memoryState.descriptor.UseAbsoluteAddressing,
                memoryState.descriptor.Mask,
                memoryState.descriptor.Shift
            ));

        private static IEnumerable<bool> SetAll<T>(DescribedMemoryState memoryState, T value) where T : struct, IConvertible
            => memoryState.GetAddressList().Select(address => SetVariableValue(
                ProcessStream.Instance,
                typeof(T),
                value,
                address,
                memoryState.descriptor.UseAbsoluteAddressing,
                memoryState.descriptor.Mask,
                memoryState.descriptor.Shift
            )).ToArray();

        public delegate IEnumerable<T> GetterFunction<out T>();

        public delegate IEnumerable<bool> SetterFunction<T>(T value);

        public static class ViewProperties
        {
            static ViewProperties() => StringSymbolAttribute.InitializeDeclaredStrings(typeof(ViewProperties));

            [StringSymbol]
            public static readonly string
                useHex,
                invertBool,
                specialType,
                roundingLimit,
                display,
                color;
        }

        public interface IView
        {
            Action ValueSet { get; set; }
            Action OnDelete { get; set; }
            string Name { get; }
            bool SetValueByKey(string key, object value);
            string GetValueByKey(string key);
            int DislpayPriority { get; }
            string Subclass { get; }
            public Type ClrType { get; }
        }

        public interface IView<T> : IView
        {
            GetterFunction<T> _getterFunction { get; }
            SetterFunction<T> _setterFunction { get; }
        }

        public interface IMemoryDescriptorView : IView
        {
            MemoryDescriptor memoryDescriptor { get; }
            DescribedMemoryState describedMemoryState { get; }
        }

        public class CustomView : IView
        {
            public Action ValueSet { get; set; }
            public Action OnDelete { get; set; }
            public string Name { get; set; }
            public string Subclass { get; }
            public Type ClrType { get; }

            public string Color
            {
                set { SetValueByKey(ViewProperties.color, value); }
            }

            public string Display
            {
                set { SetValueByKey(ViewProperties.display, value); }
            }

            public int DislpayPriority { get; }

            Dictionary<string, string> keyedValues = new Dictionary<string, string>();

            public CustomView(string subclass, Type clrType)
            {
                Subclass = subclass;
                ClrType = clrType;
            }

            public virtual string GetValueByKey(string key)
            {
                if (keyedValues.TryGetValue(key, out var result))
                    return result;
                return null;
            }

            public virtual bool SetValueByKey(string key, object value)
            {
                keyedValues[key] = value.ToString();
                return true;
            }
        }

        public class CustomView<T> : CustomView, IView<T>
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
                this.describedMemoryState = new DescribedMemoryState(memoryDescriptor);
            }
        }

        public class MemoryDescriptorView<T> : MemoryDescriptorView, IView<T> where T : struct, IConvertible
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
            int IView.DislpayPriority => 0;
            public Type ClrType => describedMemoryState.descriptor.ClrType;

            readonly XElement xElement;
            public MemoryDescriptor memoryDescriptor { get; }
            public DescribedMemoryState describedMemoryState { get; }

            public XmlMemoryView(MemoryDescriptor memoryDescriptor, XElement xElement)
            {
                this.xElement = xElement;
                this.memoryDescriptor = memoryDescriptor;
                this.describedMemoryState = new DescribedMemoryState(memoryDescriptor);
                Name = xElement.Value;
                var subclassName = xElement.Attribute("subclass")?.Value;
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

        public class XmlMemoryView<T> : XmlMemoryView, IView<T> where T : struct, IConvertible
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
}
