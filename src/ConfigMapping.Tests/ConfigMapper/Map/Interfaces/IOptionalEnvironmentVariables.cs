namespace ConfigMapping.Tests.ConfigMapper.Map.Interfaces
{
    public interface IOptionalEnvironmentVariables
    {
        [Optional]
        string TestOptional { get; }

        [Optional("default")]
        string TestOptionalWithDefault { get; }
    }
}