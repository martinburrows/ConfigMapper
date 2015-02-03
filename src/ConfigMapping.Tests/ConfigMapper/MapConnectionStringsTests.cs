using System.Reflection;
using NUnit.Framework;

namespace ConfigMapping.Tests.ConfigMapper
{
    [TestFixture]
    public class MapConnectionStringsTests
    {
        private IConnectionStrings _config;

        [SetUp]
        public void Setup()
        {
            _config = ConfigMapping.ConfigMapper.MapConnectionStrings<IConnectionStrings>();
        }

        [Test]
        public void When_mapping_connectionstrings_then_connectionstrings_are_mapped_correctly()
        {
            Assert.AreEqual("TestConnectionString", _config.TestName);
            Assert.AreEqual("World", _config.Hello.ConnectionString);
        }

        [Test]
        public void When_mapping_connectionstrings_with_the_same_interface_multiple_times_then_only_one_concrete_implementation_of_the_interface_is_ever_created()
        {
            var localConfig = ConfigMapping.ConfigMapper.MapConnectionStrings<IConnectionStrings>();

            _config.TestName = "test";

            Assert.AreEqual(_config.TestName, localConfig.TestName);
        }

        [Test]
        public void When_invalid_connection_string_key_mappings_are_attempted_then_exceptions_are_thrown()
        {
            Assert.Throws<TargetInvocationException>(
                () => ConfigMapping.ConfigMapper.MapConnectionStrings<IInvalidNameConnectionStrings>());

            Assert.Throws<TargetInvocationException>(
                () => ConfigMapping.ConfigMapper.MapConnectionStrings<IInvalidTypeConnectionStrings>());
        }
    }
}
