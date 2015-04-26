using System;
using System.Reflection;
using ConfigMapping.Exceptions;

namespace ConfigMapping.Configuration
{
    public abstract class EnvironmentVariables : ConfigBase
    {
        protected internal override void ___MapProperty(PropertyInfo property, FieldInfo field, string mapFromKey, string defaultValue, bool isOptional)
        {
            var environmentVariable = Environment.GetEnvironmentVariable(mapFromKey);

            if (!isOptional && environmentVariable == null)
                throw new ConfigMappingException("No matching key in environment variables", mapFromKey);

            var value = environmentVariable ?? defaultValue;

            ___SetFieldValue(field, mapFromKey, value);
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