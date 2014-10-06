using System;
using System.Linq;
using System.Reflection;

namespace ConfigMapping
{
    // using non-standard method names to avoid clashes with app settings of the same names
    public abstract class ConfigBase
    {
        protected internal void ___InitialiseFieldValues()
        {
            var type = this.GetType();

            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                MapProperty(property,type);
            }
        }

        protected internal abstract void MapProperty(PropertyInfo property, Type type);

        public override string ToString()
        {
            var type = this.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            return properties.Count() == 1 ? properties.Single().GetValue(this, null).ToString() : base.ToString();
        }
    }
}