using System;

namespace ConfigMapping
{
    public class MapFromAttribute : Attribute
    {
        public MapFromAttribute(string mapFrom)
        {
            MapFrom = mapFrom;
        }

        public string MapFrom { get; private set; }
    }
}