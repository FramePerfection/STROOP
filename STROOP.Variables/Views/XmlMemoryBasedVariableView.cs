using System.Reflection;
using System.Xml.Linq;

namespace STROOP.Variables.Views;

public interface IXmlMemoryBasedVariableView
{
    XElement GetXml();
}

public class XmlMemoryBasedVariableView<T>
    : MemoryBasedVariableView<T>, IXmlMemoryBasedVariableView
    where T : struct, IConvertible
{
    private readonly XElement xElement;

    static string SubclassFromXml(XElement xElement)
    {
        var subclassName = xElement.Attribute("subclass")?.Value;
        return subclassName != null
            ? (string?)typeof(WatchVariableSubclass).GetFields( BindingFlags.Static | BindingFlags.Public)
                .SingleOrDefault(x => x.Name == subclassName)?.GetValue(null) ?? WatchVariableSubclass.Number
            : WatchVariableSubclass.Number;
    }

    public XmlMemoryBasedVariableView(MemoryDescriptor memoryDescriptor, XElement xElement)
        : base(SubclassFromXml(xElement), memoryDescriptor)
    {
        this.xElement = xElement;
        Name = xElement.Value;
    }

    public override string GetValueByKey(string key) => xElement.Attribute(key)?.Value ?? null;

    public override bool SetValueByKey(string key, object value)
    {
        xElement.SetAttributeValue(XName.Get(key), value.ToString());
        return true;
    }

    public XElement GetXml() => xElement;
}
