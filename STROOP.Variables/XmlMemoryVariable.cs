using System.Reflection;
using System.Xml.Linq;

namespace STROOP.Variables;

public interface IXmlMemoryVariable
{
    XElement GetXml();
}

public class XmlMemoryVariable<T>(MemoryDescriptor memoryDescriptor, XElement xElement)
    : MemoryVariable<T>(SubclassFromXml(xElement), memoryDescriptor)
        , IXmlMemoryVariable
    where T : struct, IConvertible
{
    static string SubclassFromXml(XElement xElement)
    {
        var subclassName = xElement.Attribute("subclass")?.Value;
        return subclassName != null
            ? (string?)typeof(VariableSubclass).GetFields( BindingFlags.Static | BindingFlags.Public)
                .SingleOrDefault(x => x.Name == subclassName)?.GetValue(null) ?? VariableSubclass.Number
            : VariableSubclass.Number;
    }

    public override string GetValueByKey(string key)
        => xElement.Attribute(key)?.Value;

    public override bool SetValueByKey(string key, string value)
    {
        xElement.SetAttributeValue(XName.Get(key), value);
        return true;
    }

    public XElement GetXml() => xElement;
}
