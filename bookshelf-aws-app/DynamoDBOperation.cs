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

        public async Task CreateUserTable()
        {
            string tableName = "User";

            // Check if the table already exists
            var tables = await client.ListTablesAsync();
            if (tables.TableNames.Contains(tableName))
            {
                return; // Exit if the table already exists
            }

            CreateTableRequest request = new CreateTableRequest
            {
                TableName = tableName,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        AttributeName="Id",
                        AttributeType="S"
                    }
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName="Id",
                        KeyType="HASH"
                    }
                },
                BillingMode = BillingMode.PROVISIONED,
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 2,
                    WriteCapacityUnits = 1
                }
            };

            try
            {
                var response = await client.CreateTableAsync(request);
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK) 
                {
                    MessageBox.Show("Table created successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                };

            }
            catch (InternalServerErrorException iee)
            {
                MessageBox.Show("An error occurred on the server side ", iee.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (LimitExceededException lee)
            {
                MessageBox.Show("you are creating a table with one or more secondary indexes+ ", lee.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Create a new user
        public async Task CreateUser(string id,string username, string password) 
        {
            User user = new User
            {
                Id = id,
                UserName = username,
                Password = password
            };

            var updateItemRequest = new UpdateItemRequest
            {
                TableName = "User", // Name of your DynamoDB table
                Key = new Dictionary<string, AttributeValue>
        {
            { "Id", new AttributeValue { S = id } } // Primary key attribute
        },
                UpdateExpression = "SET UserName = :username, Password = :password",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
        {
            { ":username", new AttributeValue { S = username } },
            { ":password", new AttributeValue { S = password } }
        }
            };

            try
            {
                await client.UpdateItemAsync(updateItemRequest);
                MessageBox.Show("UserName and Password created successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            //// Create a new user object
            //User user = new User
            //{
            //    Id = id,
            //    UserName = username,
            //    Password = password
            //};

            //// Save the user object to the 'User' table
            //await context.SaveAsync(user);

            //// check if the user is on the table
            //User createdUser = await context.LoadAsync<User>(id);

            //// Show a message box if the user is created successfully
            //if (createdUser != null)
            //{
            //    MessageBox.Show("User created successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            //}

            //// Show a message box if the user creation failed
            //else
            //{
            //    MessageBox.Show("User creation failed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}
        }

        // Retrieve user by username
        public async Task<User> RetrieveUser(string username)
        {
            // Create the condition to find the user by username in the 'User' table
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("UserName", ScanOperator.Equal, username)
            };

            // Scan to find the user by username
            var search = context.ScanAsync<User>(conditions);

            // Get the result of the scan
            var result = await search.GetNextSetAsync();

            // If the result is not empty, return the first user
            if (result.Count > 0)
            {
                return result.First();
            }

            // Return null if no user found
            return null;  
        }

        public async Task UpdateUser(string id, string username, string password)
        {
            User user = await context.LoadAsync<User>(id);
            user.UserName = username;
            user.Password = password;

            await context.SaveAsync(user);
        }

        public async Task DeleteUserById(string id)
        {
            await context.DeleteAsync<User>(id);

            User deletedUser = await context.LoadAsync<User>(id, new DynamoDBContextConfig
            {
                ConsistentRead = true
            });

            if (deletedUser == null)
                Console.WriteLine("User has been deleted");
        }

        public async Task CreateBookTable() 
        {          
            string tableName = "Book";

            // Check if the table already exists
            var tables = await client.ListTablesAsync();
            if (tables.TableNames.Contains(tableName))
            {
                return; // Exit if the table already exists
            }

            CreateTableRequest request = new CreateTableRequest
            {
                TableName = tableName,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        AttributeName="ISBN",
                        AttributeType="S"
                    }
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName="ISBN",
                        KeyType="HASH"
                    }
                },
                BillingMode = BillingMode.PROVISIONED,
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 2,
                    WriteCapacityUnits = 1
                }
            };

            try
            {
                var response = await client.CreateTableAsync(request);
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    MessageBox.Show("Table created successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                };

            }
            catch (InternalServerErrorException iee)
            {
                MessageBox.Show("An error occurred on the server side ", iee.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (LimitExceededException lee)
            {
                MessageBox.Show("you are creating a table with one or more secondary indexes+ ", lee.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task CreateBook(string isbn, string title, List<string> authors)
        {
            //ProductCatalog table
            Book myBook = new Book
            {
                ISBN = "999-000001",
                Title = "AWS Certified Developer Guide: architecture  ",
                BookAuthors = new List<string> { "Tong Kim", "Cindy Smith" },
                CoverPage = "The cover page"
            };

            await context.SaveAsync(myBook);
        }
    }
}
