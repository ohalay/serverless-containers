using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using static Amazon.Lambda.DynamoDBEvents.DynamoDBEvent;

public interface IExecutor
{
    Task Execute(DynamodbStreamRecord request);
}

internal class Executor : IExecutor
{
    private readonly ILogger<Executor> logger;

    public Executor(ILogger<Executor> logger)
        => this.logger = logger;

    public Task Execute(DynamodbStreamRecord request)
    {
        logger.LogInformation("Handler executed... {EventId}", request.EventID);

        return Task.CompletedTask;
    }
}
