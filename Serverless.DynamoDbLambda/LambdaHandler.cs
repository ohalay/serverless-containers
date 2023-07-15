using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static Amazon.Lambda.DynamoDBEvents.StreamsEventResponse;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
namespace Serverless.DynamoDbLambda;

public class LambdaHandler
{
    private readonly IServiceProvider _serviceProvider;

    public LambdaHandler()
        => _serviceProvider = BuildServiceProvider();

    internal LambdaHandler(Action<IServiceCollection> configure)
        => _serviceProvider = BuildServiceProvider(configure);

    public async Task<StreamsEventResponse> Handle(DynamoDBEvent dynamoEvent)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var executor = scope.ServiceProvider.GetRequiredService<IExecutor>();

        var tasks = dynamoEvent.Records
            .Select(executor.Execute)
            .ToList();

        var failedItems = new List<BatchItemFailure>();

        try
        {
            await Task.WhenAll(tasks);
        }
        catch
        {
            failedItems = tasks
                .Select((task, index) => new { task, index })
                .Where(x => !x.task.IsCompletedSuccessfully)
                .Select(x => new BatchItemFailure { ItemIdentifier = dynamoEvent.Records[x.index].EventID })
                .ToList();
        }

        return new StreamsEventResponse { BatchItemFailures = failedItems };
    }

    public static IServiceProvider BuildServiceProvider(Action<IServiceCollection>? configure = null)
    {
        var configuration = GetConfiguration();

        var seriveCollection = new ServiceCollection()
            .Configure<Config>(options => configuration.Bind(options))
            .AddLogging(opt => opt.AddJsonConsole(c => c.IncludeScopes = true))
            .AddTransient<IExecutor, Executor>();

        configure?.Invoke(seriveCollection);

        return seriveCollection.BuildServiceProvider();
    }

    private static IConfiguration GetConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }
}
