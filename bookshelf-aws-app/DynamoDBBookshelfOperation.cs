using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Amazon.DynamoDBv2.Model;
using System.Windows;
using System.Diagnostics;

namespace bookshelf_aws_app
{
    class DynamoDBBookshelfOperation
    {

        DynamoDBOperation dynamoDBOperation = new DynamoDBOperation();
        DynamoDBUserOperation dynamoDBUserOperation = new DynamoDBUserOperation();
        private App app;
        public string tableName = "Bookshelf";
        public string userIdAttribute = "UserId";

        public DynamoDBBookshelfOperation()
        {
            app = (App)Application.Current;
        }

        // Create a bookshelf table
        public async Task CreateBookshelfTableAsync()
        {
            // access the DynamoDB client 
            var client = app.DynamoDbClient;

            // if table already exists, return
            if (await dynamoDBOperation.DoesTableExistAsync(tableName))
            {
                return;
            }

            // if doesn't exist, create
            CreateTableRequest request = new CreateTableRequest
            {
                TableName = tableName,
                AttributeDefinitions = new List<AttributeDefinition>
                {   
                    new AttributeDefinition
                    {
                        AttributeName = userIdAttribute,
                        AttributeType="N"
                    }
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = userIdAttribute,
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
                Debug.WriteLine($"An error occurred on the server side {iee}");
            }
            catch (LimitExceededException lee)
            {
                Debug.WriteLine($"you are creating a table with one or more secondary indexes+ {lee}");
            }
        }

        // Insert books
        public async Task InsertBooks()
        {
            // access the DynamoDB context
            var context = app.DynamoDbContext;

            // create a book list
            List<Book> books = CreateBooksList();

            // Get all users from user table
            List<User> users = await dynamoDBUserOperation.GetAllUsersAsync();

            try
            {
                foreach (var user in users)
                {
                    // load the bookshelf associated with userId
                    var existingBookshelf = await context.LoadAsync<Bookshelf>(user.Id);

                    // set current bookshelf
                    existingBookshelf = app.CurrentBookshelf;

                    // create a hashset to store existing book titles
                    HashSet<string> existingBookTitles = new HashSet<string>();

                    if (existingBookshelf != null)
                    {
                        Debug.WriteLine($"Bookshelf already exists");
                        return;
                    }

                    List<Book> booksToInsert = new List<Book>();

                    foreach (var book in books) 
                    {
                        if (!existingBookTitles.Contains(book.Title))
                        {
                            booksToInsert.Add(book);
                        }
                    }

                    if (booksToInsert.Count > 0)
                    {
                        var bookshelf = new Bookshelf
                        {
                            // Assign the user's Id
                            UserId = user.Id,
                            // Assign two books to each user's bookshelf
                            Books = new List<Book>
                        {
                            // First book (calc based on the user index)
                            books[users.IndexOf(user) * 2], 
                            // Second book 
                            books[(users.IndexOf(user) * 2) + 1]
                        }
                        };
                        // save the bookshelf object to the 'Bookshelf' table
                        await context.SaveAsync(bookshelf);
                    }
                }

                Debug.WriteLine("Bookshelf insertion successful");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Bookshelf insertion failed {e}");
            }
        }

        public async Task AddLastViewedPageNumber(int userId, string selectedTitle, int lastViewedPageNumber, DateTime closingTime)
        {
            // Access the DynamoDB context
            var context = app.DynamoDbContext;

            try
            {
                // Load the bookshelf associated with the given userId
                var bookshelf = await context.LoadAsync<Bookshelf>(userId);

                // Check if the bookshelf exists
                if (bookshelf != null && bookshelf.Books != null)
                {
                    // Find the book by ISBN in the bookshelf
                    var book = bookshelf.Books.FirstOrDefault(b => b.Title == selectedTitle);

                    if (book != null)
                    {
                        // Update the last viewed page and closing time for the book
                        book.LastViewedPage = lastViewedPageNumber;
                        book.ClosingTime = closingTime.ToString();

                        // Save the updated bookshelf to DynamoDB
                        await context.SaveAsync(bookshelf);

                        Debug.WriteLine("Your book on the Bookshelf was updated successfully");
                    }
                    else
                    {
                        Debug.WriteLine("Book not found in the bookshelf.");
                    }
                }
                else
                {
                    Debug.WriteLine("Bookshelf not found for the given user ID.");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error updating bookshelf: " + e.Message);
            }
        }

        // Create a list of random books
        public static List<Book> CreateBooksList()
        {
            List<Book> bookList = new List<Book>();

            bookList.Add(new Book
            {
                ISBN = "9780451531384",
                Title = "A Room With a View",
                Authors = new List<string> { "E. M. Forster"}
            });

            bookList.Add(new Book
            {
                ISBN = "0486282112",
                Title = "Frankenstein; Or, The Modern Prometheus",
                Authors = new List<string> { "Mary Wollstonecraft Shelley" }
            });

            bookList.Add(new Book
            {
                ISBN = "0743273567",
                Title = "The Great Gatsby",
                Authors = new List<string> { "F. Scott Fitzgerald" }
            });

            bookList.Add(new Book
            {
                ISBN = "1503287274",
                Title = "Narrative of the Life of Frederick Douglass, an American Slave",
                Authors = new List<string> { "Frederick Douglass" }
            });

            bookList.Add(new Book
            {
                ISBN = "1503222683",
                Title = "Alice's Adventures in Wonderland",
                Authors = new List<string> { "Lewis Carroll" }
            });

            bookList.Add(new Book
            {
                ISBN = "0486266885",
                Title = "The Strange Case of Dr. Jekyll and Mr. Hyde",
                Authors = new List<string> { "Robert Louis Stevenson" }
            });
            return bookList;
        }

        public async Task<List<Book>> GetBooksByUser(int userId) 
        {
            // Access the DynamoDB context
            var context = app.DynamoDbContext;

            // Create a list to store the books
            var bookList = new List<Book>();

            try 
            {
                var bookshelf = await context.LoadAsync<Bookshelf>(userId, new DynamoDBOperationConfig
                {
                    ConsistentRead = true
                });

                bookList = bookshelf.Books;
            }
            catch (Exception e)
            {
                MessageBox.Show("Error getting books by user: " + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return bookList;

        }

        public async Task<Bookshelf> GetBookshelfByUserId(int userId)
        {
            // Access the DynamoDB context
            var context = app.DynamoDbContext;

            // Query the Bookshelf table using the userId (assuming userId is the partition key)
            var bookshelf = await context.LoadAsync<Bookshelf>(userId);

            return bookshelf;
        }
    }
}
