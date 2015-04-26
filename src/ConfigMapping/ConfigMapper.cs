using System;
using System.Collections.Concurrent;
using System.Configuration;
using ConfigMapping.Configuration;

namespace ConfigMapping
{
    public static class ConfigMapper
    {
        private static readonly ConcurrentDictionary<Tuple<Type,Type>, object> ExistingConfig;

        static ConfigMapper()
        {
            ExistingConfig = new ConcurrentDictionary<Tuple<Type, Type>, object>();
        }

        /// <summary>
        /// Returns an implementation of the given interface with properties populated from the specified source or config section.
        /// </summary>
        /// <param name="mapfrom">The config mapping source</param>
        public static T Map<T>(MapFrom mapfrom = MapFrom.AppSettings)
        {
            switch (mapfrom)
            {
                case MapFrom.AppSettings:
                    return Lookup<T, AppSettings>();

                case MapFrom.ConnectionStrings:
                    return Lookup<T, ConnectionStrings>();

                case MapFrom.EnvironmentVariables:
                    return Lookup<T, EnvironmentVariables>();

                default:
                    throw new ArgumentOutOfRangeException("mapfrom");
            }
        }

        [Obsolete("Map<>(MapFrom.ConnectionStrings) can be used instead", false)]
        public static T MapConnectionStrings<T>()
        {
            return Map<T>(MapFrom.ConnectionStrings);
        }
        
        private static TI Lookup<TI, TP>() where TP : ConfigBase
        {
            return (TI)ExistingConfig.GetOrAdd(new Tuple<Type,Type>(typeof(TI),typeof(TP)), t => new TypeGenerator<TI, TP>().Generate());
        }

        /// <summary>
        /// Repopulates all configuration objects with fresh values from the configuration file. Does not refresh environment variables. 
        /// </summary>
        public static void RefreshConfiguration()
        {
            ConfigurationManager.RefreshSection("appSettings");
            ConfigurationManager.RefreshSection("connectionStrings");

            foreach (var configuration in ExistingConfig)
                ((ConfigBase)configuration.Value).___InitialiseFieldValues();
        }
    }
}
