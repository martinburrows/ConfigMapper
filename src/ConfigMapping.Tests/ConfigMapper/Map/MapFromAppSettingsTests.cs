using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using ConfigMapping.Exceptions;
using ConfigMapping.Tests.ConfigMapper.Map.Interfaces;
using NUnit.Framework;

namespace ConfigMapping.Tests.ConfigMapper.Map
{
    public class MapFromAppSettingsTests
    {
        protected IAppSettingsConfiguration AppSettingsConfiguration;

        [SetUp]
        protected void SetUp()
        {
            ConfigMapping.ConfigMapper.RefreshConfiguration();
            AppSettingsConfiguration = ConfigMapping.ConfigMapper.Map<IAppSettingsConfiguration>();
        }
        
        [TestFixture]
        public class MapTypeConversionTests : MapFromAppSettingsTests
        {
            [TestCase(typeof (string), "TestString")]
            [TestCase(typeof (int), "TestInt")]
            [TestCase(typeof (bool), "TestBoolT")]
            [TestCase(typeof (bool), "TestBoolF")]
            [TestCase(typeof (decimal), "TestDecimal")]
            [TestCase(typeof (double), "TestDouble")]
            [TestCase(typeof (TestEnum), "TestEnum")]
            public void When_mapping_then_properties_are_mapped_correctly_from_appsettings(Type type, string key)
            {
                var propertyInfo = AppSettingsConfiguration.GetType().GetProperty(key, BindingFlags.Public| BindingFlags.Instance);
                var propertyValue = propertyInfo.GetValue(AppSettingsConfiguration, null);

                Assert.AreEqual(type, propertyInfo.PropertyType);
                Assert.AreEqual(ConfigurationManager.AppSettings[key], propertyValue.ToString());
            }

            [TestCase("tEsTsTrInG")]
            [TestCase("testint")]
            public void When_mapping_then_properties_and_config_key_mappings_are_case_insensitive(string key)
            {
                var propertyInfo = AppSettingsConfiguration.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Single(k => k.Name.Equals(key,StringComparison.OrdinalIgnoreCase));
                var propertyValue = propertyInfo.GetValue(AppSettingsConfiguration, null);

                Assert.AreEqual(ConfigurationManager.AppSettings[key], propertyValue.ToString());
            }

            [TestCase("TestEnumWrongCase")]
            public void When_mapping_enums_then_case_is_not_sensitive(string key)
            {
                var config = ConfigMapping.ConfigMapper.Map<IAppSettingsEnumConfiguration>();

                var propertyInfo = config.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Single(k => k.Name.Equals(key,StringComparison.OrdinalIgnoreCase));
                var propertyValue = propertyInfo.GetValue(config, null);
                
                Assert.AreEqual(TestEnum.Bar, propertyValue);
            }
        }

        [TestFixture]
        public class MapDuplicateObjectGenerationTests : MapFromAppSettingsTests
        {
            [Test]
            public void When_mapping_multiple_times_then_only_one_concrete_implementaion_of_an_interface_is_ever_created()
            {
                const string modifiedString = "modified";

                var firstCall = AppSettingsConfiguration;
                var secondCall = ConfigMapping.ConfigMapper.Map<IAppSettingsConfiguration>();
                var thirdCall = ConfigMapping.ConfigMapper.Map<IAppSettingsConfiguration>();

                firstCall.TestString = modifiedString;

                Assert.AreEqual(modifiedString, firstCall.TestString);
                Assert.AreEqual(modifiedString, secondCall.TestString);
                Assert.AreEqual(firstCall.TestString, thirdCall.TestString);
            }

            [Test]
            public void When_refreshing_config_then_config_is_refreshed()
            {
                const string modifiedString = "modified";

                var firstCall = AppSettingsConfiguration;
                var secondCall = ConfigMapping.ConfigMapper.Map<IAppSettingsConfiguration>();
                var thirdCall = ConfigMapping.ConfigMapper.Map<IAppSettingsConfiguration>();

                var originalString = firstCall.TestString;
                firstCall.TestString = modifiedString;

                Assert.AreNotEqual(originalString,modifiedString);

                ConfigMapping.ConfigMapper.RefreshConfiguration();

                Assert.AreEqual(originalString, firstCall.TestString);
                Assert.AreEqual(originalString, secondCall.TestString);
                Assert.AreEqual(firstCall.TestString, thirdCall.TestString);
            }
        }

        [TestFixture]
        public class MapInvalidInterfacesTests : MapFromAppSettingsTests
        {
            [Test]
            public void When_exceptions_are_thrown_they_contain_the_property_name_of_the_attempted_mapping()
            {
                try
                {
                    ConfigMapping.ConfigMapper.Map<INonExistentAppSettingsConfiguration>();
                }
                catch (Exception e)
                {
                    Assert.IsTrue(e.InnerException.Message.Contains("NonExistent"));
                }
            }
            
            [Test]
            public void When_mapping_missing_keys_exceptions_should_be_thrown()
            {
                Assert.Throws<TargetInvocationException>(() => ConfigMapping.ConfigMapper.Map<INonExistentAppSettingsConfiguration>());
            }

            [Test]
            public void When_mapping_invalid_types_exceptions_should_be_thrown()
            {
                Assert.Throws<TargetInvocationException>(() => ConfigMapping.ConfigMapper.Map<IInvalidTypeAppSettingsConfiguration>());
            }
        }
    }
}
