using System;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace AutoConfiguration
{
    public abstract class Configuration
    {
        protected internal void InitialiseFieldValues()
        {
            var type = this.GetType();

            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var field = type.GetField("_" + property.Name, BindingFlags.Instance | BindingFlags.NonPublic);
                SetFieldValue(field, ConfigurationManager.AppSettings[property.Name]);
            }
        }

        private void SetFieldValue(FieldInfo field, string value)
        {
            var fieldType = field.FieldType;
            
            var convertMethod = typeof(Convert).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m => m.Name.StartsWith("To") && m.ReturnType == fieldType && m.GetParameters().Last().ParameterType == typeof(string));

            field.SetValue(this, convertMethod.Invoke(null, new object[] { value }));
        }
    }
}