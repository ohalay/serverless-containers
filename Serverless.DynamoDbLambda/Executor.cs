using System.IO;
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

    public async Task Execute(DynamodbStreamRecord request)
    {
        logger.LogInformation("Handler executed... {EventId}", request.EventID);

        if (request.Dynamodb.Keys["Sk"].S == "F")
        {
            throw new InvalidDataException();
        }

        await Task.CompletedTask;
    }
}
