using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
namespace Serverless.Lambda;

public class LambdaHandler
{
    private readonly IServiceProvider _serviceProvider;

    public LambdaHandler()
        => _serviceProvider = BuildServiceProvider();

    internal LambdaHandler(Action<IServiceCollection> configure)
        => _serviceProvider = BuildServiceProvider(configure);

    public async Task Handle(ILambdaContext context)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<LambdaHandler>>();
        using var _ = logger.BeginScope(new Dictionary<string, object> { ["RequestId"] = context.AwsRequestId });

        var executor = scope.ServiceProvider.GetRequiredService<IExecutor>();
        await executor.Execute(context);
    }

    public static IServiceProvider BuildServiceProvider(Action<IServiceCollection>? configure = null)
    {
        var configuration = GetConfiguration();

        var seriveCollection = new ServiceCollection()
            .Configure<Config>(options => configuration.Bind(options))
            .AddLogging(opt => opt.AddJsonConsole(c => c.IncludeScopes = true))
            .AddTransient<IExecutor, Executor>()
            .AddSingleton<IAmazonS3>(_ => new AmazonS3Client());

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
