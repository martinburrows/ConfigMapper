using System.Reflection;
using NUnit.Framework;

namespace ConfigMapping.Test.Unit.ConfigMapperTests
{
    [TestFixture]
    public class ConnectionStringsTests
    {
        private IConnectionStrings _config;

        [SetUp]
        public void Setup()
        {
            _config = ConfigMapper.MapConnectionStrings<IConnectionStrings>();
        }

        [Test]
        public void TestConnectionStringsMapCorrectly()
        {
            Assert.AreEqual("TestConnectionString", _config.TestName);
            Assert.AreEqual("World", _config.Hello.ConnectionString);
        }

        [Test]
        public void TestOnlyOneConnectionStringInstanceOfEachTypeIsCreated()
        {
            var localConfig = ConfigMapper.MapConnectionStrings<IConnectionStrings>();

            _config.TestName = "test";

            Assert.AreEqual(_config.TestName, localConfig.TestName);
        }

        [Test]
        public void TestExceptionsAreThrownForInvalidInterfaces()
        {
            Assert.Throws<TargetInvocationException>(
                () => ConfigMapper.MapConnectionStrings<IInvalidNameConnectionStrings>());

            Assert.Throws<TargetInvocationException>(
                () => ConfigMapper.MapConnectionStrings<IInvalidTypeConnectionStrings>());
        }
    }
}
