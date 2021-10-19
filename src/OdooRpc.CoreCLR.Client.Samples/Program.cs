using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OdooRpc.CoreCLR.Client.Models;

namespace OdooRpc.CoreCLR.Client.Samples
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Starting...");
            var configuration = LoadConfiguration();
            var odooConnection = configuration.GetSection("OdooConnection").Get<OdooConnectionInfo>();

            var client = new SampleClient(odooConnection);
            await client.LoginToOdoo();
            await client.GetMetadata();

            // test your calls here
                        
            Console.WriteLine("Done!");
        }

        private static IConfiguration LoadConfiguration()
        {
            var environment = Environment.GetEnvironmentVariable("DOTNETCORE_ENVIRONMENT") ?? "Development";
            Console.WriteLine($"Loading config for environment: {environment}");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json", true)
                .AddEnvironmentVariables();
            IConfiguration configuration = builder.Build();

            return configuration;
        }
    }
}