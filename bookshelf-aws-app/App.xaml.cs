using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using System.Configuration;
using System.Windows;

namespace bookshelf_aws_app
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public AmazonDynamoDBClient DynamoDbClient { get; set; }
        public DynamoDBContext DynamoDbContext { get; set; }
        public Amazon.Runtime.BasicAWSCredentials AwsCredentials { get; set; }
        public User CurrentUser { get; set; }
        public Bookshelf CurrentBookshelf { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initializes AWS credentials and DynamoDB client/context to make it accessible throughout the application
            AwsCredentials = new Amazon.Runtime.BasicAWSCredentials
                (
                ConfigurationManager.AppSettings["accessId"], 
                ConfigurationManager.AppSettings["secretKey"]
                );
            DynamoDbClient = new AmazonDynamoDBClient(AwsCredentials, Amazon.RegionEndpoint.USEast1);
            DynamoDbContext = new DynamoDBContext(DynamoDbClient);
        }

        public App()
        {
        }

    }
}
