using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace bookshelf_aws_app
{
    class DynamoDBUserOperation
    {
        AmazonDynamoDBClient client;
        DynamoDBContext context;
        Amazon.Runtime.BasicAWSCredentials credentials;

        DynamoDBOperation dynamoDBOperation = new DynamoDBOperation();

        public DynamoDBUserOperation() 
        {
            credentials = new Amazon.Runtime.BasicAWSCredentials(ConfigurationManager.AppSettings["accessId"], ConfigurationManager.AppSettings["secretKey"]);
            client = new AmazonDynamoDBClient(credentials, Amazon.RegionEndpoint.USEast1);
            context = new DynamoDBContext(client);
        }

        // Method to create a user table
        public async Task CreateUserTableAsync()
        {
            string tableName = "User";

            if (await dynamoDBOperation.DoesTableExistAsync(tableName))
            {
                return;
            }

            CreateTableRequest request = new CreateTableRequest
            {
                TableName = tableName,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        AttributeName="Id",
                        AttributeType="N"
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
                    MessageBox.Show("Table created successfully");
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

        // Create three users programatically 
        public async Task CreateThreeUsersAsync()
        {
            List<User> users = new List<User>();

            users.Add(new User { Id = 1, UserName = "user1", Password = "password1" });
            users.Add(new User { Id = 2, UserName = "user2", Password = "password2" });
            users.Add(new User { Id = 3, UserName = "user3", Password = "password3" });

            foreach (User user in users)
            {
                await CreateUserAsync(user.Id, user.UserName, user.Password);
            }
        }

        // Create a new user
        public async Task CreateUserAsync(int id, string username, string password)
        {
            try
            {
                User user = new User
                {
                    Id = id,
                    UserName = username,
                    Password = password
                };

                // check if the user is on the table
                User existingUser = await context.LoadAsync<User>(id);
                if (existingUser != null)
                {
                    MessageBox.Show("User already exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Save the user object to the 'User' table
                await context.SaveAsync(user);

                //// check if the user is on the table
                User createdUser = await context.LoadAsync<User>(id);

                // Show a message box if the user is created successfully
                if (createdUser != null)
                {
                    MessageBox.Show($"User {username} created successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("User creation failed" + e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Retrieve user by username
        public async Task<User> GetUserByUsername(string username)
        {
            try 
            {
                // Create the condition to find the user by username in the 'User' table
                var conditions = new List<ScanCondition>
                {
                    new ScanCondition("UserName", ScanOperator.Equal, username)
                };

                // Scan to find the user by username
                var search = context.ScanAsync<User>(conditions);

                //MessageBox.Show("Searching for user: " + username);

                Debug.WriteLine("Searching for user: " + username);

                // Get the result of the scan
                var result = await search.GetNextSetAsync(); // CODE BREAKS HERE

                //MessageBox.Show("Result count: " + result.Count);

                // If the result is not empty, return the first user
                if (result.Count > 0)
                {
                    return result.First();
                }

                // Return null if no user found
                return null;
            }
            catch (Exception e)
            {
                MessageBox.Show("Error : " + e.Message);
                return null;
            }
        }

        // Get all users
        public async Task<List<User>> GetAllUsersAsync()
        {
            // Scan the 'User' table to get all users
            var search = context.ScanAsync<User>(new List<ScanCondition>());

            // Get the result of the scan
            var result = await search.GetNextSetAsync();

            // Return the list of users
            return result;
        }
    }
}
