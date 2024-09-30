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

namespace bookshelf_aws_app
{
    class DynamoDBOperation
    {
        AmazonDynamoDBClient client;
        DynamoDBContext context;
        Amazon.Runtime.BasicAWSCredentials credentials;

        public DynamoDBOperation()
        {
            credentials = new Amazon.Runtime.BasicAWSCredentials(ConfigurationManager.AppSettings["accessId"], ConfigurationManager.AppSettings["secretKey"]);
            client = new AmazonDynamoDBClient(credentials, Amazon.RegionEndpoint.USEast1);
            context = new DynamoDBContext(client);
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
    }
}
