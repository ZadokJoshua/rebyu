using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Rebyu.Plugins;
using Rebyu.Services;
using Rebyu.ViewModels;
using System;

namespace Rebyu.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddServices(this IServiceCollection collection)
    {
        collection.AddSingleton<MainViewModel>();
        collection.AddSingleton<SqliteDataService>();

        var openaiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY", EnvironmentVariableTarget.User);

        if (string.IsNullOrEmpty(openaiApiKey))
        {
            throw new InvalidOperationException("OPENAI_API_KEY environment variable is not set.");
        }

        var kernelBuilder = collection.AddKernel();
        kernelBuilder.Services.AddOpenAIChatCompletion(
            "gpt-4.1", 
            openaiApiKey);


        kernelBuilder.Plugins.AddFromType<SchemaPlugin>();
        kernelBuilder.Plugins.AddFromType<TimeInformation>();

    }
}
