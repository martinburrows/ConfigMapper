using System;
using System.Configuration;
using System.Reflection;
using ConfigMapping.Exceptions;

namespace ConfigMapping
{
    public abstract class ConnectionStrings : ConfigBase
    {
        protected internal override void MapProperty(PropertyInfo property, Type type)
        {
            var field = type.GetField("_" + property.Name, BindingFlags.Instance | BindingFlags.NonPublic);

            var connectionStringSettings = ConfigurationManager.ConnectionStrings[property.Name];

            if (connectionStringSettings == null)
            {
                throw new ConfigMappingException("No matching key in connectionStrings could be found",
                    property.Name);
            }

            ___SetFieldValue(field, connectionStringSettings, property.Name);
        }

        private void ___SetFieldValue(FieldInfo field, ConnectionStringSettings connectionStringSettings, string propertyName)
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
                throw new ConfigMappingException(string.Format("Connection string mappings must be either of type System.String or System.Configuration.ConnectionStringSettings. The type provided was {0}", field.FieldType.Name), propertyName);
            }
        }
    }
}