using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using STROOP.Structs.Configurations;
using STROOP.Utilities;

namespace STROOP.Core.Variables
{
    public class NamedVariableCollection
    {
        private static IEnumerable<T> GetValues<T>(DescribedMemoryState memoryState) where T : struct, IConvertible
            => memoryState.GetAddressList().ConvertAll(address => (T)Config.Stream.GetValue(
                typeof(T),
                address,
                memoryState.descriptor.UseAbsoluteAddressing,
                memoryState.descriptor.Mask,
                memoryState.descriptor.Shift
            ));

        private static IEnumerable<bool> SetAll<T>(DescribedMemoryState memoryState, T value) where T : struct, IConvertible
            => memoryState.GetAddressList().Select(address => Config.Stream.SetValueRoundingWrapping(
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
            static ViewProperties() => StringUtilities.InitializeDeclaredStrings(typeof(ViewProperties));

            [DeclaredString]
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
            Type GetWrapperType();
            int DislpayPriority { get; }
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

            protected readonly Type wrapperType;
            public Type GetWrapperType() => wrapperType;

            public CustomView(Type wrapperType)
            {
                if (wrapperType == null)
                    System.Diagnostics.Debugger.Break();
                this.wrapperType = wrapperType;
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

            public CustomView(Type wrapperType) : base(wrapperType)
            {
                _getterFunction = WatchVariableSpecialUtilities.Defaults<T>.DEFAULT_GETTER;
                _setterFunction = WatchVariableSpecialUtilities.Defaults<T>.DEFAULT_SETTER;
            }
        }

        public class MemoryDescriptorView : CustomView, IMemoryDescriptorView
        {
            public MemoryDescriptor memoryDescriptor { get; }
            public DescribedMemoryState describedMemoryState { get; }

            public MemoryDescriptorView(MemoryDescriptor memoryDescriptor, string wrapper)
                : base(WatchVariableUtilities.GetWrapperType(memoryDescriptor.MemoryType, wrapper))
            {
                this.memoryDescriptor = memoryDescriptor;
                this.describedMemoryState = new DescribedMemoryState(memoryDescriptor);
            }
        }

        public class MemoryDescriptorView<T> : MemoryDescriptorView, IView<T> where T : struct, IConvertible
        {
            public MemoryDescriptorView(MemoryDescriptor memoryDescriptor, string wrapper = "Number")
                : base(memoryDescriptor, wrapper)
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
            int IView.DislpayPriority => 0;

            readonly string wrapper;
            readonly XElement xElement;
            public MemoryDescriptor memoryDescriptor { get; }
            public DescribedMemoryState describedMemoryState { get; }

            public XmlMemoryView(MemoryDescriptor memoryDescriptor, XElement xElement)
            {
                this.xElement = xElement;
                this.memoryDescriptor = memoryDescriptor;
                this.describedMemoryState = new DescribedMemoryState(memoryDescriptor);
                Name = xElement.Value;
                wrapper = xElement.Attribute(XName.Get("subclass"))?.Value ?? "Number";
            }

            public Type GetWrapperType() => WatchVariableUtilities.GetWrapperType(memoryDescriptor.MemoryType, wrapper);
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

        public static IView ParseXml(XElement element)
        {
            switch (element.Name.LocalName)
            {
                case "Data":
                    var specialType = element.Attribute(XName.Get("specialType"))?.Value;
                    if (specialType != null)
                    {
                        if (WatchVariableSpecialUtilities.dictionary.TryGetValue(specialType, out var value))
                            return value;
                        return null;
                    }

                    return MemoryDescriptor.FromXml(element).view;
            }

            return null;
        }
    }
}
