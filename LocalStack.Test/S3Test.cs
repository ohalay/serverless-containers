using Amazon.S3;
using FluentAssertions;
using System.Text;
using Testcontainers.LocalStack;

namespace LocalStack.Test;

public class LambdaTests : IClassFixture<LocalStackFixture>
{
    private const string fileName = "test.txtx";
    private readonly LocalStackContainer container;

    public LambdaTests(LocalStackFixture localStackFixture)
        => container = localStackFixture.Container;

    [Fact]
    public async Task PutS3Test()
    {
        var s3Client = new AmazonS3Client(new AmazonS3Config { ServiceURL = container.GetConnectionString() });


        await s3Client.PutObjectAsync(new Amazon.S3.Model.PutObjectRequest
        {
            BucketName = LocalStackFixture.BUCKETNAME,
            Key = fileName,
            InputStream = new MemoryStream(Encoding.UTF8.GetBytes("My test data"))
        });


        var result = await s3Client.GetObjectAsync(LocalStackFixture.BUCKETNAME, fileName);

        using var reader = new StreamReader(result.ResponseStream, Encoding.UTF8);
        var data = reader.ReadToEnd();

        result.HttpStatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
}
