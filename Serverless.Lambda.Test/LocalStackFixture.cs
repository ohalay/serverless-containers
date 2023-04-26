using Testcontainers.LocalStack;

namespace Serverless.Lambda.Test;

public class LocalStackFixture : IAsyncLifetime
{
    public const string BUCKETNAME = "test-bucket";

    public readonly LocalStackContainer Container = new LocalStackBuilder()
        .WithStartupCallback((container, ct) => container.ExecAsync(new[] { "awslocal", "s3api", "create-bucket", "--bucket", BUCKETNAME }, ct))
        .Build();

    static LocalStackFixture()
    {
        Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", "dummy");
        Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", "dummy");
    }

    public Task DisposeAsync()
        => Container.StopAsync();

    public Task InitializeAsync()
        => Container.StartAsync();
}
