using System.IO;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NLog;

namespace PIMTool
{
    public class Program
    {
        public static void Main(string[] args)
        {
            LogManager.Setup()
                .LoadConfigurationFromFile($"{Directory.GetCurrentDirectory()}/Configurations/nlog.config")
                .GetCurrentClassLogger();

            var host = Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(builder =>
                {
                    builder.UseStartup<Startup>();
                    // builder.ConfigureLogging(configure => { configure.ClearProviders(); });
                })
                .Build();

            host.Run();
            LogManager.Shutdown();
        }
    }
}