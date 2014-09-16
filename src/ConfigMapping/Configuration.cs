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
        protected internal void ___InitialiseFieldValues()
        {
            var type = this.GetType();

            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var field = type.GetField("_" + property.Name, BindingFlags.Instance | BindingFlags.NonPublic);

                if (ConfigurationManager.AppSettings.AllKeys.Contains(property.Name))
                    ___SetFieldValue(field, property);
                else 
                    throw new ConfigMappingException("No matching key in appSettings could be found", property.Name);
            }
        }

        private void ___SetFieldValue(FieldInfo field, PropertyInfo property)
        {
            try
            {
                var value = ConfigurationManager.AppSettings[property.Name];

                field.SetValue(this,
                    field.FieldType.IsEnum
                        ? Enum.Parse(field.FieldType, value)
                        : Convert.ChangeType(value, field.FieldType));
            }
            catch (Exception e)
            {
                throw new ConfigMappingException(e.Message, property.Name);
            }
        }
    }
}