using System;
using System.Configuration;

namespace AutoConfiguration.TestClient
{
    public interface IConfiguration
    {
        string Test { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = ConfigurationReader.Read<IConfiguration>();

            Console.WriteLine(configuration.Test);
            configuration.Test = "modified";

            Console.WriteLine(configuration.Test);

            Console.ReadKey();
        }
    }
}
