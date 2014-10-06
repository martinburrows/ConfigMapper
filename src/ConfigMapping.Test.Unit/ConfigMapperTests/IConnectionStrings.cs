using System.Configuration;

namespace ConfigMapping.Test.Unit.ConfigMapperTests
{
    public interface IConnectionStrings
    {
        string TestName { get; set; }
        ConnectionStringSettings Hello { get; set; }
    }

    public interface IInvalidTypeConnectionStrings
    {
        int Foo { get; set; }
    }

    public interface IInvalidNameConnectionStrings
    {
        string Invalid { get; set; }
    }
}
