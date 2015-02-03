using System;
using System.Collections.Concurrent;
using System.Configuration;
using ConfigMapping.Configuration;

namespace ConfigMapping
{
    public static class ConfigMapper
    {
        private static readonly ConcurrentDictionary<Type, object> ExistingConfiguration;
        private static readonly ConcurrentDictionary<Type, object> ExistingConnectionStrings;

        static ConfigMapper()
        {
            ExistingConfiguration = new ConcurrentDictionary<Type, object>();
            ExistingConnectionStrings = new ConcurrentDictionary<Type, object>();
        }
 
        /// <summary>
        /// Returns an implementation of the given interface with properties populated from the appSettings config section.
        /// </summary>
        public static T Map<T>()
        {
            return Lookup<T, AppSettings>(ExistingConfiguration);
        }

        /// <summary>
        /// Returns an implementation of the given interface with properties populated from the connectionStrings config section.
        /// </summary>
        public static T MapConnectionStrings<T>()
        {
            return Lookup<T, ConnectionStrings>(ExistingConnectionStrings);
        }

        private static TI Lookup<TI, TP>(ConcurrentDictionary<Type, object> collection) where TP : ConfigBase
        {
            return (TI)collection.GetOrAdd(typeof(TI), t => new TypeGenerator<TI,TP>().Generate());
        }

        /// <summary>
        /// Repopulates all configuration objects with fresh values from the appSettings config section.
        /// </summary>
        public static void RefreshConfiguration()
        {
            RefreshConfigurationManager();

            foreach (var configuration in ExistingConfiguration)
                ((AppSettings)configuration.Value).___InitialiseFieldValues();
        }

        /// <summary>
        /// Repopulates configuration objects of only the type provided with fresh values from the appSettings config section.
        /// <returns>
        /// The refreshed configuration of the given type
        /// </returns>
        /// </summary>
        public static T RefreshConfiguration<T>()
        {
            RefreshConfigurationManager();

            var type = typeof (T);

            if (!ExistingConfiguration.ContainsKey(type)) return Map<T>();

            var configuration = ExistingConfiguration[type];
            ((AppSettings)configuration).___InitialiseFieldValues();
            return (T)configuration;
        }

        private static void RefreshConfigurationManager()
        {
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
