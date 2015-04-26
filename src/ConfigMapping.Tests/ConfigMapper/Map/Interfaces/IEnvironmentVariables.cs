namespace ConfigMapping.Tests.ConfigMapper.Map.Interfaces
{
    public interface IEnvironmentVariables
    {
        string TestString { get; }
        int TestInt { get; }
    }

    public interface IOptionalEnvironmentVariables
    {
        [Optional]
        string TestOptional { get; }

        [Optional("default")]
        string TestOptionalWithDefault { get; }
    }

    public interface IInvalidEnvironmentVariables
    {
        string NonExistent { get; }
    }
}