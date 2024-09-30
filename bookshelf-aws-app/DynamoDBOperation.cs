using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;
using System.Windows;
using System.IO;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;
using System.IO.Packaging;

namespace bookshelf_aws_app
{
    class DynamoDBOperation
    {
        AmazonDynamoDBClient client;
        DynamoDBContext context;
        Amazon.Runtime.BasicAWSCredentials credentials;
        AmazonS3Client s3Client;

        public DynamoDBOperation()
        {
            credentials = new Amazon.Runtime.BasicAWSCredentials(ConfigurationManager.AppSettings["accessId"], ConfigurationManager.AppSettings["secretKey"]);
            client = new AmazonDynamoDBClient(credentials, Amazon.RegionEndpoint.USEast1);
            context = new DynamoDBContext(client);
            s3Client = new AmazonS3Client(credentials, Amazon.RegionEndpoint.USEast1);
        }

        // Method to check the table status
        public async Task WaitForTableToBeActiveAsync(string tableName)
        {
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
            // list tables
            var tables = await client.ListTablesAsync();
            // true if the table is on the list
            return tables.TableNames.Contains(tableName);
        }

        // Method to get a PDF from S3
        public async Task<MemoryStream> GetPdfFromS3Async(string bucketName, string objectKey)
        {        
            string key = objectKey + ".pdf";

            try
            {
                // Create the request to get the object
                var request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = key
                };

                // Get the object from S3
                using (var response = await s3Client.GetObjectAsync(request))

                using (var responseStream = response.ResponseStream)
                {
                    // Copy the response stream to a MemoryStream
                    MemoryStream documentStream = new MemoryStream();
                    await responseStream.CopyToAsync(documentStream);
                    documentStream.Position = 0; // Reset position to the beginning
                    return documentStream;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error)
                Console.WriteLine($"Error fetching PDF from S3: {ex.Message}");
                return null;
            }
        }
    }
}
