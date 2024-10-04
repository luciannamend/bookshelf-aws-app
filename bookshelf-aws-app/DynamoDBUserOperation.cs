using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using System.Diagnostics;
using System.Windows;

namespace bookshelf_aws_app
{
    /// <summary>
    /// 
    /// </summary>
    class DynamoDBUserOperation
    {
        DynamoDBOperation dynamoDBOperation = new DynamoDBOperation();
        private App app;
        public string tableName = "User";

        public DynamoDBUserOperation() 
        {
            app = (App)Application.Current;
        }

        // Method to create a user table
        public async Task CreateUserTableAsync()
        {
            // access the DynamoDB client and context from the App class
            var client = app.DynamoDbClient;
            var context = app.DynamoDbContext;

            // if table exists, return
            if (await DoesTableExistAsync(client, tableName))
            {
                return;
            }

            // Create a new table request
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
                    Debug.WriteLine("Table created successfully");
                };

            }
            catch (InternalServerErrorException iee)
            {
                Debug.WriteLine("An error occurred on the server side " + iee);
            }
            catch (LimitExceededException lee)
            {
                Debug.WriteLine("you are creating a table with one or more secondary indexes+ " + lee);
            }
        }

        // Create three users programatically 
        public async Task CreateThreeUsersAsync()
        {
            List<User> users = new List<User>
            {
                new User { Id = 1, UserName = "user1", Password = "password1" },
                new User { Id = 2, UserName = "user2", Password = "password2" },
                new User { Id = 3, UserName = "user3", Password = "password3" }
            };

            foreach (User user in users)
            {
                await CreateUserAsync(user.Id, user.UserName, user.Password);
            }
        }

        // Create a new user
        public async Task CreateUserAsync(int id, string username, string password)
        {
            // access the DynamoDB context from the App class
            var context = app.DynamoDbContext;

            try
            {
                // Hash the password for security
                string hashedPassword = HashPassword(password);

                User user = new User
                {
                    Id = id,
                    UserName = username,
                    Password = hashedPassword
                };

                // check if the user is on the table
                User existingUser = await context.LoadAsync<User>(id);

                // if user exists, return
                if (existingUser != null)
                {
                    Debug.WriteLine("User already exists: " + username);
                    return;
                }

                // save the user
                await context.SaveAsync(user);
                Debug.WriteLine($"User {username} created successfully");
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error creating user: " + e.Message);
            }
        }

        // encrypt the password
        private string HashPassword(string password)
        {
            // Generate a salt and hash the password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            // return the hashed password
            return hashedPassword; 
        }

        // verify the hashed password
        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        // Retrieve user by username
        public async Task<User> GetUserByUsername(string username)
        {
            // access the DynamoDB context from the App class
            var context = app.DynamoDbContext;
            try 
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
            catch (Exception e)
            {
                Debug.WriteLine($"Error to get user {username} : " + e.Message);
                return null;
            }
        }

        // Get all users
        public async Task<List<User>> GetAllUsersAsync()
        {
            // access the DynamoDB context from the App class
            var context = app.DynamoDbContext;

            // Scan User table to get all users
            var search = context.ScanAsync<User>(new List<ScanCondition>());

            // Get the result of the scan
            var result = await search.GetNextSetAsync();

            // Return the list of users
            return result;
        }

        // PASS THIS () TO THE DB OPERATIONS GENERAL
        // Helper method to check if the table exists
        private async Task<bool> DoesTableExistAsync(AmazonDynamoDBClient client, string tableName)
        {
            var tables = await client.ListTablesAsync();
            return tables.TableNames.Contains(tableName);
        }
    }
}
