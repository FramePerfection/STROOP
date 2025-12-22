using System;

namespace STROOP.Tabs.MapTab
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ObjectDescriptionAttribute : Attribute
    {
        public readonly string DisplayName;
        public readonly string Initializer;
        public readonly string Category;

        public ObjectDescriptionAttribute(string DisplayName, string Category, string InitializerName = null)
        {
            this.DisplayName = DisplayName;
            this.Category = Category;
            this.Initializer = InitializerName;
        }
    }
}
