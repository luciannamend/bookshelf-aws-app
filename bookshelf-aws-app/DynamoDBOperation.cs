using Amazon.DynamoDBv2;
using System.Configuration;
using Amazon.DynamoDBv2.Model;
using System.Windows;
using System.IO;
using Amazon.S3;
using Amazon.S3.Model;
using System.Diagnostics;

namespace bookshelf_aws_app
{
    /// <summary>
    /// Provides methods for interacting with AWS DynamoDB and S3 services. Includes asynchronous methods
    /// to check the status of a DynamoDB table and verify its existence by listing available tables.
    /// Additionally, it has a method to retrieve a PDF file from an S3 bucket, converting the file's stream
    /// into a MemoryStream for easy access and manipulation. 
    /// </summary>
    class DynamoDBOperation
    {
        AmazonS3Client s3Client;
        private App app;

        public DynamoDBOperation()
        {
            app = (App)Application.Current;
            var credentials = app.AwsCredentials;
            s3Client = new AmazonS3Client(credentials, Amazon.RegionEndpoint.GetBySystemName(ConfigurationManager.AppSettings["AWSRegion"]));
        }

        // Method to check the table status
        public async Task WaitForTableToBeActiveAsync(string tableName)
        {
            var client = app.DynamoDbClient;
            bool isTableActive = false;

            while (!isTableActive)
            {
                var response = await client.DescribeTableAsync(new DescribeTableRequest
                {
                    TableName = tableName
                });

                if (response.Table.TableStatus == TableStatus.ACTIVE)
                {
                    isTableActive = true;
                }
                else
                {
                    // Wait for a short period before checking again
                    await Task.Delay(1000);
                }
            }
        }                     

        // Check if a table exists
        public async Task<bool> DoesTableExistAsync(string tableName) 
        {
            var client = app.DynamoDbClient;
            // list tables
            var tables = await client.ListTablesAsync();
            // true if the table is on the list
            return tables.TableNames.Contains(tableName);
        }

        // Method to get a PDF from S3
        public async Task<MemoryStream> GetPdfFromS3Async(string bucketName, string objectKey)
        {
            // full key for the PDF file in S3 - match exact name
            string key = objectKey + ".pdf";

            try
            {
                // create the request to get the object
                var request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = key
                };

                // get the object from S3
                using var response = await s3Client.GetObjectAsync(request);

                // get the response stream
                using var responseStream = response.ResponseStream;

                // create a MemoryStream to store the PDF
                MemoryStream documentStream = new MemoryStream();

                // copy the response stream to a MemoryStream
                await responseStream.CopyToAsync(documentStream);

                // reset position to the beginning
                documentStream.Position = 0;

                // return the MemoryStream containing the PDF
                return documentStream;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching PDF from S3: {ex.Message}");
                return null;
            }
        }
    }
}
