using System.Configuration;

namespace ConfigMapping.Tests.ConfigMapper.Map.Interfaces
{
    public interface IConnectionStrings
    {
        string TestName { get; set; }
        ConnectionStringSettings Hello { get; set; }
    }
}
