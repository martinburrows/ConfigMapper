using NUnit.Framework;

namespace ConfigMapping.Test.Unit.ConfigMapperTests
{
    public class ToStringTests
    {
        protected ISingleAppSetting SingleAppSetting;
        protected IAppSettingsConfiguration AppSettingsConfiguration;  

        [SetUp]
        protected void SetUp()
        {
            AppSettingsConfiguration = ConfigMapping.ConfigMapper.RefreshConfiguration<IAppSettingsConfiguration>();
            SingleAppSetting = ConfigMapping.ConfigMapper.RefreshConfiguration<ISingleAppSetting>();
        }

        [TestFixture]
        public class SingleAppSettingsToStringTest : ToStringTests
        {
            [Test]
            public void TestSinglePropertyInterfacesCanBeAccessedViaToStringCalledOnTheClass()
            {
                var toStringValue = SingleAppSetting.ToString();
                Assert.AreEqual(AppSettingsConfiguration.TestString, toStringValue);
                Assert.AreEqual(AppSettingsConfiguration.TestString, SingleAppSetting.TestString);
            }

            [Test]
            public void TestMultiplePropertyInterfacesCallTheBaseToStringMethod()
            {
                var toStringValue = AppSettingsConfiguration.ToString();
                Assert.IsNotEmpty(toStringValue);
            }
        }
        
    }
}