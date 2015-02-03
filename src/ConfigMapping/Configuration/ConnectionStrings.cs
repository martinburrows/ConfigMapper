using System;
using System.Configuration;
using System.Reflection;
using ConfigMapping.Exceptions;

namespace ConfigMapping.Configuration
{
    public abstract class ConnectionStrings : ConfigBase
    {
        protected internal override void ___MapProperty(PropertyInfo property, FieldInfo field, string mapFromKey, string defaultValue, bool isOptional)
        {
            var connectionStringSettings = ConfigurationManager.ConnectionStrings[mapFromKey];

            if (connectionStringSettings == null && !isOptional)
                throw new ConfigMappingException("No matching key in connectionStrings could be found", mapFromKey);

            connectionStringSettings = connectionStringSettings ?? new ConnectionStringSettings(mapFromKey, defaultValue);

            ___SetFieldValue(field, connectionStringSettings, mapFromKey);
        }

        private void ___SetFieldValue(FieldInfo field, ConnectionStringSettings connectionStringSettings, string mapFromKey)
        {
            if (field.FieldType == typeof (ConnectionStringSettings))
            {
                field.SetValue(this,connectionStringSettings);
            }
            else if (field.FieldType == typeof (string))
            {
                field.SetValue(this,connectionStringSettings.ConnectionString);
            }
            else 
            {
                throw new ConfigMappingException(string.Format("Connection string mappings must be either of type System.String or System.Configuration.ConnectionStringSettings. The type provided was {0}", field.FieldType.Name), mapFromKey);
            }
        }
    }
}