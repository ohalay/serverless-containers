using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.SQS;
using Constructs;
using Attribute = Amazon.CDK.AWS.DynamoDB.Attribute;

namespace Infra;

public class DynamoDbLambda : Stack
{
    internal DynamoDbLambda(Construct scope, string id, IStackProps props) : base(scope, id, props)
    {
        var dynamoDbTable = new Table(this, "testTable", new TableProps
        {
            BillingMode = BillingMode.PAY_PER_REQUEST,
            PartitionKey = new Attribute { Name = "Pk", Type = AttributeType.STRING },
            SortKey = new Attribute { Name = "Sk", Type = AttributeType.STRING },
            Stream = StreamViewType.NEW_AND_OLD_IMAGES
        });

        var buildOption = new BundlingOptions()
        {
            Image = Runtime.DOTNET_6.BundlingImage,
            User = "root",
            OutputType = BundlingOutput.ARCHIVED,
            Command = new string[]{
               "/bin/sh",
                "-c",
                " dotnet tool install -g Amazon.Lambda.Tools"+
                " && dotnet build"+
                " && dotnet lambda package --output-package /asset-output/function.zip"
                }
        };

        var dynamoDbLambda = new Function(this, "dynamoDbLambda", new FunctionProps
        {
            Runtime = Runtime.DOTNET_6,
            MemorySize = 256,
            Handler = "Serverless.DynamoDbLambda::Serverless.DynamoDbLambda.LambdaHandler::Handle",
            Code = Code.FromAsset("Serverless.DynamoDbLambda/", new Amazon.CDK.AWS.S3.Assets.AssetOptions
            {
                Bundling = buildOption
            }),
        });

        var deadLetterQueue = new Queue(this, "deadLetterQueue");

        dynamoDbLambda.AddEventSource(new DynamoEventSource(dynamoDbTable, new DynamoEventSourceProps
        {
            ReportBatchItemFailures = true,
            StartingPosition = StartingPosition.TRIM_HORIZON,
            OnFailure = new SqsDlq(deadLetterQueue),
            RetryAttempts = 5,
            Filters = new[]
            {
                FilterCriteria.Filter(new Dictionary<string, object> {
                    ["dynamodb"] = new Dictionary<string, object>
                    {
                        ["Keys"] = new Dictionary<string, object>
                        {
                            ["Pk"] = new Dictionary<string, object>
                            {
                                ["S"] = new[]{"prefix","MyTableName" }
                            }
                        }
                    }
                })
            },
        }));
    }
}
