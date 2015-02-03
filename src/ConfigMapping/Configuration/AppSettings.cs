using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using ConfigMapping.Exceptions;

namespace ConfigMapping.Configuration
{
    public abstract class AppSettings : ConfigBase
    {
        protected internal override void ___MapProperty(PropertyInfo property, FieldInfo field, string mapFromKey, string defaultValue, bool isOptional)
        {
            var configKey = ConfigurationManager.AppSettings.AllKeys.SingleOrDefault(c => c.Equals(mapFromKey, StringComparison.OrdinalIgnoreCase));

            if (configKey == null && !isOptional)
                throw new ConfigMappingException("No matching key in appSettings could be found", mapFromKey);

            var value = ConfigurationManager.AppSettings[configKey] ?? defaultValue;

            ___SetFieldValue(field, configKey, value);
        }

        private void ___SetFieldValue(FieldInfo field, string mapFromKey, string value)
        {
            try
            {
                field.SetValue(this,
                    field.FieldType.IsEnum
                        ? Enum.Parse(field.FieldType, value, true)
                        : Convert.ChangeType(value, field.FieldType));
            }
            catch (Exception e)
            {
                throw new ConfigMappingException(e.Message, mapFromKey);
            }
        }
    }
}