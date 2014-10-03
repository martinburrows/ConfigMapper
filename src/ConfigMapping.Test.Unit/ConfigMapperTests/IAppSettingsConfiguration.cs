namespace ConfigMapping.Test.Unit.ConfigMapperTests
{
    public interface IAppSettingsConfiguration
    {
        string TestString { get; set; }
        int TestInt { get; set; }
        bool TestBoolT { get; set; }
        bool TestBoolF { get; set; }
        decimal TestDecimal { get; set; }
        double TestDouble { get; set; }
        TestEnum TestEnum { get; set; }
    }
}