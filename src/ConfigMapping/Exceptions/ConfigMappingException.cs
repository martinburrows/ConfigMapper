using System;

namespace ConfigMapping.Exceptions
{
    public class ConfigMappingException : Exception
    {
        private readonly string _message;
        private readonly string _field;

        public ConfigMappingException(string message, string field)
        {
            _message = message;
            _field = field;
        }

        public override string Message
        {
            get { return string.Format("Failed to map field '{0}' - {1}", _field, _message ); }
        }
    }
}
