using System.Linq;
using System.Reflection;

namespace ConfigMapping.Configuration
{
    // using non-standard method names to avoid clashes with app settings of the same names
    public abstract class ConfigBase
    {
        protected internal void ___InitialiseFieldValues()
        {
            var type = this.GetType();

            var interfaces = type.GetInterfaces();
            var @interface = interfaces.Except(interfaces.SelectMany(t => t.GetInterfaces())).Single();

            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var interfaceProperty = @interface.GetProperty(property.Name);

                var mapFromKey = interfaceProperty.GetCustomMappingKeyFromAttribute() ?? property.Name;
                bool isOptional;

                var defaultValue = interfaceProperty.GetDefaultValueFromAttribute(out isOptional);

                var field = type.GetField("_" + property.Name, BindingFlags.Instance | BindingFlags.NonPublic);

                ___MapProperty(property,field, mapFromKey, defaultValue, isOptional);
            }
        }

        protected internal abstract void ___MapProperty(PropertyInfo property, FieldInfo field, string mapFromKey, string defaultValue, bool isOptional);

        public override string ToString()
        {
            var type = this.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            return properties.Count() == 1 ? properties.Single().GetValue(this, null).ToString() : base.ToString();
        }
    }
}