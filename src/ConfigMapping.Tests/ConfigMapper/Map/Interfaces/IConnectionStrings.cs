using System.Configuration;

namespace ConfigMapping.Tests.ConfigMapper.Map.Interfaces
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
