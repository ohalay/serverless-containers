using DotNet.Testcontainers.Builders;
using Testcontainers.LocalStack;

namespace LocalStack.Test;

public class LocalStackFixture : IAsyncLifetime
{
    public const string BUCKETNAME = "test-bucket";

    public readonly LocalStackContainer Container = new LocalStackBuilder()
        .WithImage("localstack/localstack:latest")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted(new[]
        {
            "awslocal", "s3api", "create-bucket", "--bucket", BUCKETNAME,
        }))
        .Build();

    static LocalStackFixture()
    {
        Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", "my-test-key");
        Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", "my-test-secret-key");
    }

    public Task DisposeAsync()
        => Container.StopAsync();

    public Task InitializeAsync()
        => Container.StartAsync();
}
