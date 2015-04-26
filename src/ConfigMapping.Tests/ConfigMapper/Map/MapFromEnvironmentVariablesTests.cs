using System;
using System.Reflection;
using ConfigMapping.Tests.ConfigMapper.Map.Interfaces;
using NUnit.Framework;

namespace ConfigMapping.Tests.ConfigMapper.Map
{
    [TestFixture]
    public class MapFromEnvironmentVariablesTests
    {
        const string TestString = "test";
        const int TestInt = 123;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Environment.SetEnvironmentVariable("teststring", TestString, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("testint", TestInt.ToString(), EnvironmentVariableTarget.Process);
        }

        [Test]
        public void When_mapping_from_environment_variables_then_maps_work_correctly()
        {
            var environmentVariables =
                ConfigMapping.ConfigMapper.Map<IEnvironmentVariables>(MapFrom.EnvironmentVariables);

            Assert.AreEqual(TestString, environmentVariables.TestString);
            Assert.AreEqual(TestInt, environmentVariables.TestInt);
        }

        [Test]
        public void When_mapping_optional_environment_variables_then_maps_work_correctly()
        {
            const string @default = "default";

            var environmentVariables =
                ConfigMapping.ConfigMapper.Map<IOptionalEnvironmentVariables>(MapFrom.EnvironmentVariables);

            Assert.IsNull(environmentVariables.TestOptional);
            Assert.AreEqual(@default, environmentVariables.TestOptionalWithDefault);
        }

        [Test]
        public void When_mapping_invalid_environment_variables_then_exception_is_thrown()
        {
            Assert.Throws<TargetInvocationException>(() => ConfigMapping.ConfigMapper.Map<IInvalidEnvironmentVariables>(MapFrom.EnvironmentVariables));
        }
    }
}