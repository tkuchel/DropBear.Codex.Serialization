using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZLogger;
using ZLogger.Providers;

namespace DropBear.Codex.Serialization.ConsoleApp;

internal abstract class Program
{
    private static async Task Main()
    {
        var serviceProvider = ConfigureServices();
        await RunSerializationDemo(serviceProvider).ConfigureAwait(false);
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddDataSerializationServices();
        services.AddTransient<SerializationDemo>();
        ConfigureZLogger(services);
        return services.BuildServiceProvider();
    }

    private static async Task RunSerializationDemo(IServiceProvider serviceProvider)
    {
        var serializerDemo = serviceProvider.GetRequiredService<SerializationDemo>();
        await serializerDemo.ExecuteDemo().ConfigureAwait(false);
    }

    /// <summary>
    ///     Configures ZLogger logging services.
    /// </summary>
    /// <param name="services">The IServiceCollection to add logging services to.</param>
    private static void ConfigureZLogger(IServiceCollection services) =>
        services.AddLogging(builder =>
        {
            builder.ClearProviders()
                .SetMinimumLevel(LogLevel.Debug)
                .AddZLoggerConsole(options =>
                {
                    options.UseJsonFormatter(formatter =>
                    {
                        formatter.IncludeProperties = IncludeProperties.Timestamp | IncludeProperties.LogLevel |
                                                      IncludeProperties.FilePath | IncludeProperties.CategoryName |
                                                      IncludeProperties.Message;
                    });
                })
                .AddZLoggerRollingFile(options =>
                {
                    options.FilePathSelector = (timestamp, sequenceNumber) =>
                        $"logs/{timestamp.ToLocalTime():yyyy-MM-dd}_{sequenceNumber:000}.log";
                    options.RollingInterval = RollingInterval.Day;
                    options.RollingSizeKB = 1024; // 1MB
                });
        });
}