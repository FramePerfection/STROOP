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
}
