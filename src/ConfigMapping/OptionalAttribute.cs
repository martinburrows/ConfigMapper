using System;

namespace ConfigMapping
{
    public class OptionalAttribute : Attribute
    {
        /// <summary>
        /// Mark this property as optional; ConfigMapper will not throw an exception if this key does not exist in config.
        /// </summary>
        /// <param name="defaultValue">Default value to use when this key does not exist in config (optional)</param>
        public OptionalAttribute(string defaultValue = null)
        {
            Default = defaultValue;
        }

        public string Default { get; private set; }
    }
}