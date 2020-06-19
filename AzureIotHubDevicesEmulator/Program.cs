using System.Collections.Generic;
using System.Threading.Tasks;
using AzureIotHubDevicesEmulator.Constants;
using AzureIotHubDevicesEmulator.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AzureIotHubDevicesEmulator
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        private static Task MainAsync(string[] args)
        {
            var configBuilder = new ConfigurationBuilder();
            var configuration = configBuilder
                .AddJsonFile("appsettings.json")
                .Build();

            var services = new ServiceCollection();
            ConfigureServices(services, configuration);

            var serviceProvider = services.BuildServiceProvider();

            var simulation = serviceProvider.GetService<ISimulation>();
            return simulation.Run();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging(builder => builder.ClearProviders().AddConfiguration(configuration).AddConsole())
                .AddSingleton(configuration)
                .AddSingleton<ISimulation, Simulation>()
                .AddSingleton<IIotHubRegistryManager, IotHubRegistryManager>()
                .AddSingleton<IIotHubDevicesSimulator, IotHubDevicesSimulator>()
                .AddSingleton<IMessageFactory>(sp => new MessageFactory(
                    new Dictionary<string, IMessageBuilder>()
                    {
                        { MessageTypes.Alarm, new AlarmMessageBuilder() },
                        { MessageTypes.Event, new EventMessageBuilder() },
                        { MessageTypes.TimeSeries, new TimeSeriesMessageBuilder() }
                    }));
        }
    }
}
