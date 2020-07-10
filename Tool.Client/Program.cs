using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Tool.Client.Tools;
using Tool.Manager;
using Tool.Manager.Interfaces;

namespace Tool.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
            .AddSingleton<ITool, ExampeTool>()
            .BuildServiceProvider();

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            ToolsManager.Configure(serviceProvider, config);
            ToolsManager.Run();
        }
    }
}
