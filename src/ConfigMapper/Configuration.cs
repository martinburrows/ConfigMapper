using System;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace ConfigMapper
{
    public abstract class Configuration
    {
        protected internal void InitialiseFieldValues()
        {
            var type = this.GetType();

            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var field = type.GetField("_" + property.Name, BindingFlags.Instance | BindingFlags.NonPublic);

                if (ConfigurationManager.AppSettings.AllKeys.Contains(property.Name))
                    SetFieldValue(field, ConfigurationManager.AppSettings[property.Name]);
            }
        }

        private void SetFieldValue(FieldInfo field, string value)
        {
            field.SetValue(this,
                field.FieldType.IsEnum ? Enum.Parse(field.FieldType, value) : Convert.ChangeType(value, field.FieldType));
        }
    }
}