using System.Text;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Serverless.Lambda;

internal interface IExecutor
{
    Task Execute(ILambdaContext ctx);
}

public class Executor : IExecutor
{
    private readonly ILogger<Executor> logger;
    private readonly IAmazonS3 s3Client;
    private readonly Config config;

    public Executor(
        ILogger<Executor> logger,
        IAmazonS3 s3Client,
        IOptions<Config> options)
    {
        this.logger = logger;
        this.s3Client = s3Client;
        config = options.Value;
    }
    public async Task Execute(ILambdaContext ctx)
    {
        var content = "this is my test content";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        await Save(ctx.AwsRequestId, stream);

        logger.LogInformation("File '{FileName} 'saved", ctx.AwsRequestId);
    }

    private async Task Save(string key, Stream stream)
    {
        var request = new PutObjectRequest
        {
            BucketName = config.BucketName,
            InputStream = stream,
            Key = $"{key}.txt",
        };

        _ = await s3Client.PutObjectAsync(request);
    }
}
