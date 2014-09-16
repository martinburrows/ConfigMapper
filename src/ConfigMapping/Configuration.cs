using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using ConfigMapping.Exceptions;

namespace ConfigMapping
{
    // using non-standard method names to avoid clashes with app settings of the same names
    public abstract class Configuration
    {
        protected internal void ___InitialiseFieldValues(bool ignoreMissingEntries = false)
        {
            var type = this.GetType();

            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var field = type.GetField("_" + property.Name, BindingFlags.Instance | BindingFlags.NonPublic);

                var configKey = ConfigurationManager.AppSettings.AllKeys.SingleOrDefault(c => c.Equals(property.Name, StringComparison.OrdinalIgnoreCase));

                if (configKey == null)
                {
                    if (ignoreMissingEntries)
                        continue;
                    throw new ConfigMappingException("No matching key in appSettings could be found", property.Name);
                }

                ___SetFieldValue(field, configKey);
            }
        }

        private void ___SetFieldValue(FieldInfo field, string configKey)
        {
            try
            {
                var value = ConfigurationManager.AppSettings[configKey];

                field.SetValue(this,
                    field.FieldType.IsEnum
                        ? Enum.Parse(field.FieldType, value)
                        : Convert.ChangeType(value, field.FieldType));
            }
            catch (Exception e)
            {
                throw new ConfigMappingException(e.Message, configKey);
            }
        }
    }
}