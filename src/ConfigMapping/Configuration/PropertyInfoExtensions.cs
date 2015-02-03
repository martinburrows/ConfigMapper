using System.Linq;
using System.Reflection;

namespace ConfigMapping.Configuration
{
    internal static class PropertyInfoExtensions
    {
        internal static string GetCustomMappingKeyFromAttribute(this PropertyInfo property)
        {
            var attribute = property.GetCustomAttributes(true).FirstOrDefault(a => a is MapFromAttribute) as MapFromAttribute;

            return attribute != null ? attribute.MapFrom : null;
        }

        internal static string GetDefaultValueFromAttribute(this PropertyInfo property, out bool isOptional)
        {
            var attribute = property.GetCustomAttributes(true).FirstOrDefault(a => a is OptionalAttribute) as OptionalAttribute;

            isOptional = attribute != null;

            return attribute != null ? attribute.Default : null;
        }
    }
}