using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using ConfigMapping.Test.Unit.ConfigMapper;
using NUnit.Framework;

namespace ConfigMapping.Test.Unit.ConfigMapperTests
{
    public class AppSettingsTests
    {
        protected IAppSettingsConfiguration AppSettingsConfiguration;  

        [SetUp]
        protected void SetUp()
        {
            AppSettingsConfiguration = ConfigMapping.ConfigMapper.RefreshConfiguration<IAppSettingsConfiguration>();
        }
        
        [TestFixture]
        public class AppSettingsTypeConversionTest : AppSettingsTests
        {
            [TestCase(typeof (string), "TestString")]
            [TestCase(typeof (int), "TestInt")]
            [TestCase(typeof (bool), "TestBoolT")]
            [TestCase(typeof (bool), "TestBoolF")]
            [TestCase(typeof (decimal), "TestDecimal")]
            [TestCase(typeof (double), "TestDouble")]
            [TestCase(typeof (TestEnum), "TestEnum")]
            public void TestMapping(Type type, string key)
            {
                var propertyInfo = AppSettingsConfiguration.GetType().GetProperty(key, BindingFlags.Public| BindingFlags.Instance);
                var propertyValue = propertyInfo.GetValue(AppSettingsConfiguration, null);

                Assert.AreEqual(type, propertyInfo.PropertyType);
                Assert.AreEqual(ConfigurationManager.AppSettings[key], propertyValue.ToString());
            }

            [TestCase("tEsTsTrInG")]
            [TestCase("testint")]
            public void TestMappingCaseInsensitivity(string key)
            {
                var propertyInfo = AppSettingsConfiguration.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Single(k => k.Name.Equals(key,StringComparison.OrdinalIgnoreCase));
                var propertyValue = propertyInfo.GetValue(AppSettingsConfiguration, null);

                Assert.AreEqual(ConfigurationManager.AppSettings[key], propertyValue.ToString());
            }

            [TestCase("tEsTsTrInG")]
            [TestCase("testint")]
            public void TestEnumParsingCaseInsensitivity(string key)
            {
                var propertyInfo = AppSettingsConfiguration.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Single(k => k.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
                var propertyValue = propertyInfo.GetValue(AppSettingsConfiguration, null);

                Assert.AreEqual(ConfigurationManager.AppSettings[key], propertyValue.ToString());
            }
        }

        [TestFixture]
        public class AppSettingsDuplicateObjectGenerationTest : AppSettingsTests
        {
            [Test]
            public void TestOnlyOneConcreteImplementaionOfAnInterfaceIsEverCreated()
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
            public void TestRefreshConfig()
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
    }
}
