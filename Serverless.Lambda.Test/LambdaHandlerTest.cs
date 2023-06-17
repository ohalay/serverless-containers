using Amazon.Lambda.TestUtilities;
using Amazon.S3;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Serverless.Lambda.Test;

public class LambdaHandlerTest : IClassFixture<LocalStackFixture>
{
    private readonly LocalStackFixture fixture;
    private readonly ITestOutputHelper outputHelper;

    public LambdaHandlerTest(LocalStackFixture localStackFixture, ITestOutputHelper outputHelper)
    {
        fixture = localStackFixture;
        this.outputHelper = outputHelper;
    }

    [Fact]
    public async Task HandleShoudPutDocToS3Test()
    {
        var client = new AmazonS3Client(new AmazonS3Config { ServiceURL = fixture.Container.GetConnectionString() });

        var sut = new LambdaHandler(collection => collection
          .AddLogging(b => b.AddXUnit(outputHelper))
          .AddSingleton<IAmazonS3>(client)
          .PostConfigure<Config>(c => c.BucketName = LocalStackFixture.BUCKETNAME));

        var docId = Guid.NewGuid().ToString();
        await sut.Handle(new TestLambdaContext { AwsRequestId = docId });

        var res = await client.GetObjectAsync(LocalStackFixture.BUCKETNAME, $"{docId}.txt");
        _ = res.HttpStatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
}
