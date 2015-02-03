using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml;

namespace ConfigMapping
{
    public class Section : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, XmlNode section)
        {
            return new ConfigMapper((XmlElement)section);
        }
    }
}
}
