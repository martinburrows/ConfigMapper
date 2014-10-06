using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using ConfigMapping.Exceptions;

namespace ConfigMapping
{
    public abstract class Configuration : ConfigBase
    {
        protected internal override void MapProperty(PropertyInfo property, Type type)
        {
            var field = type.GetField("_" + property.Name, BindingFlags.Instance | BindingFlags.NonPublic);

            var configKey = ConfigurationManager.AppSettings.AllKeys.SingleOrDefault(c => c.Equals(property.Name, StringComparison.OrdinalIgnoreCase));

            if (configKey == null)
            {
                throw new ConfigMappingException("No matching key in appSettings could be found", property.Name);
            }
                
            ___SetFieldValue(field, configKey);
        }

        private void ___SetFieldValue(FieldInfo field, string configKey)
        {
            try
            {
                var value = ConfigurationManager.AppSettings[configKey];

                field.SetValue(this,
                    field.FieldType.IsEnum
                        ? Enum.Parse(field.FieldType, value, true)
                        : Convert.ChangeType(value, field.FieldType));
            }
            catch (Exception e)
            {
                throw new ConfigMappingException(e.Message, configKey);
            }
        }
    }
}