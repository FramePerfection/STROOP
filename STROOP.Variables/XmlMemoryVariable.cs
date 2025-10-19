using System.Reflection;
using System.Xml.Linq;

namespace STROOP.Variables;

public interface IXmlMemoryVariable
{
    XElement GetXml();
}

public class XmlMemoryVariable<T>
    : MemoryVariable<T>, IXmlMemoryVariable
    where T : struct, IConvertible
{
    private readonly XElement xElement;

    static string SubclassFromXml(XElement xElement)
    {
        var subclassName = xElement.Attribute("subclass")?.Value;
        return subclassName != null
            ? (string?)typeof(VariableSubclass).GetFields( BindingFlags.Static | BindingFlags.Public)
                .SingleOrDefault(x => x.Name == subclassName)?.GetValue(null) ?? VariableSubclass.Number
            : VariableSubclass.Number;
    }

    public XmlMemoryVariable(MemoryDescriptor memoryDescriptor, XElement xElement)
        : base(SubclassFromXml(xElement), memoryDescriptor)
    {
        this.xElement = xElement;
        Name = xElement.Value;
    }

    public override string GetValueByKey(string key) => xElement.Attribute(key)?.Value;

    public override bool SetValueByKey(string key, string value)
    {
        xElement.SetAttributeValue(XName.Get(key), value);
        return true;
    }

    public XElement GetXml() => xElement;
}
