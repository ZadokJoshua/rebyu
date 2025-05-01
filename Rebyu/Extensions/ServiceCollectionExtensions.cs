using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rebyu.Interfaces;
using Rebyu.Services;
using Rebyu.ViewModels;
using Serilog;
using Serilog.Sinks.InMemory;
using System;

namespace Rebyu.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddServices(this IServiceCollection collection)
    {
        collection.AddSingleton<MainViewModel>();
        collection.AddSingleton<SqliteDataService>();

        var openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        if (string.IsNullOrEmpty(openAiApiKey))
            throw new InvalidOperationException("OPENAI_API_KEY environment variable is not set.");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.InMemory()
            .CreateLogger();

        collection.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddSerilog(Log.Logger);
        });


        collection.AddSingleton<ISemanticKernelService>(provider =>
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            return new SemanticKernelService(loggerFactory, openAiApiKey);
        });

        collection.AddSingleton<ILiteDbDataService, LiteDbDataService>(provider =>
        {
            var connStr = "Filename=rebyu.db;Mode=Shared"; // Hardcoded for now
            return new LiteDbDataService(connStr);
        });
    }
}
