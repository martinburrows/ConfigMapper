namespace ConfigMapping.Tests.ConfigMapper
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

        [MapFrom("TestString")]
        string TestStringUsingMapFromAttribute { get; }

        [Optional]
        string NonExistentOptionalWithNoDefaultValue { get; }

        [Optional("default")]
        string NonExistentOptionalWithDefaultValue { get; }

        [MapFrom("TestString")]
        [Optional("default")]
        string TestStringWithMapFromAttributeAndOptionalValueWithDefault { get; }

        [MapFrom("NonExistent")]
        [Optional]
        string NonExistentOptionalValueWithMapFromAttributeAndNoDefault { get; }
    }
}