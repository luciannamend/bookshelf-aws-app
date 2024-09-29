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

        // Method to create a user table
        public async Task CreateUserTableAsync()
        {
            string tableName = "User";

            if (await DoesTableExistAsync(tableName))
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
                    await Task.Delay(300);
                }
            }
        }

        // Create a new user
        public async Task CreateUserAsync(string id,string username, string password) 
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

                //// Show a message box if the user creation failed
                //else
                //{
                //    MessageBox.Show("User creation failed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //    return;
                //}

            }
            catch (Exception e)
            {
                MessageBox.Show("User creation failed" + e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        // Retrieve user by username
        public async Task<User> GetUserByUsername(string username)
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
        // Update user by id
        public async Task UpdateUserAsync(string id, string username, string password)
        {
            User user = await context.LoadAsync<User>(id);
            user.UserName = username;
            user.Password = password;

            await context.SaveAsync(user);
        }

        // Delete user by id
        public async Task DeleteUserByIdAsync(string id)
        {
            await context.DeleteAsync<User>(id);

            User deletedUser = await context.LoadAsync<User>(id, new DynamoDBContextConfig
            {
                ConsistentRead = true
            });

            if (deletedUser == null)
                Console.WriteLine("User has been deleted");
        }
        
        // Create a bookshelf table
        public async Task CreateBookshelfTableAsync() 
        {          
            string tableName = "Bookshelf";

            // if table already exists, return
            if (await DoesTableExistAsync(tableName))
            {
                return;
            }

            CreateTableRequest request = new CreateTableRequest
            {
                TableName = tableName,
                AttributeDefinitions = new List<AttributeDefinition>
                {   
                    // Partition key
                    new AttributeDefinition
                    {
                        AttributeName="UserId",
                        AttributeType="S"
                    }
                },
                KeySchema = new List<KeySchemaElement>
                {                
                    new KeySchemaElement
                    {
                        AttributeName="UserId",
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

        // Create a book
        public async Task InsertBooks()
        {
            // cria list books
            List<Book> books = CreateBooksList();

            // cria lista de users da User table
            List<User> users = await GetAllUsersAsync();

            // para cada user na user table,
            // adiciona o user com um livro da booklist
            try 
            {

                foreach (var user in users)
                {
                    var bookshelf = new Bookshelf
                    {
                        UserId = user.Id,
                        Books = new List<Book> { books[users.IndexOf(user)] }
                    };

                    await context.SaveAsync(bookshelf);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Bookshelf insertion failed" + e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        // Create a list of random bookList
        public static List<Book> CreateBooksList() 
        {
            List<Book> bookList = new List<Book>();

            bookList.Add(new Book
            {
                ISBN = "978-3-16-148410-0",
                Title = "The Art of Coding",
                Authors = new List<string> { "John Smith", "Emily White" },
                CoverPage = "https://example.com/covers/the-art-of-coding.jpg"
            });

            bookList.Add(new Book
            {
                ISBN = "978-0-14-312854-0",
                Title = "Data Structures Unleashed",
                Authors = new List<string> { "Alice Johnson" },
                CoverPage = "https://example.com/covers/data-structures-unleashed.jpg"
            });

            bookList.Add(new Book
            {
                ISBN = "978-1-25-012334-7",
                Title = "Mastering Algorithms",
                Authors = new List<string> { "David Lee", "Sophia Brown" },
                CoverPage = "https://example.com/covers/mastering-algorithms.jpg"
            });

            bookList.Add(new Book
            {
                ISBN = "978-0-19-953556-9",
                Title = "Design Patterns in C#",
                Authors = new List<string> { "Michael Green" },
                CoverPage = "https://example.com/covers/design-patterns-in-csharp.jpg"
            });

            bookList.Add(new Book
            {
                ISBN = "978-1-61-729585-1",
                Title = "Building Scalable Systems",
                Authors = new List<string> { "Linda Martinez" },
                CoverPage = "https://example.com/covers/building-scalable-systems.jpg"
            });

            bookList.Add(new Book
            {
                ISBN = "978-0-321-87758-1",
                Title = "Introduction to Cloud Computing",
                Authors = new List<string> { "Robert James", "Jessica Park" },
                CoverPage = "https://example.com/covers/introduction-to-cloud-computing.jpg"
            });
            return bookList;
        }
    }
}
