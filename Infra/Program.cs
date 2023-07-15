using Amazon.CDK;

namespace Infra;

public sealed class Program
{
    public static void Main()
    {
        var app = new App();
        _ = new DynamoDbLambda(app, "DynamoDbLambda", new StackProps());
        _ = app.Synth();
    }
}
