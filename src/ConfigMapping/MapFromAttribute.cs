using System;

namespace ConfigMapping
{
    public class MapFromAttribute : Attribute
    {
        /// <summary>
        /// Specify the configuration key to map this property from.
        /// </summary>
        /// <param name="mapFrom"></param>
        public MapFromAttribute(string mapFrom)
        {
            MapFrom = mapFrom;
        }

        public string MapFrom { get; private set; }
    }
}