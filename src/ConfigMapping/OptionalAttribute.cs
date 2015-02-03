using System;

namespace ConfigMapping
{
    public class OptionalAttribute : Attribute
    {
        public OptionalAttribute(string defaultValue = null)
        {
            Default = defaultValue;
        }

        public string Default { get; private set; }
    }
}