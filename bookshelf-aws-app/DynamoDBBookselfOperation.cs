﻿using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Amazon.DynamoDBv2.Model;
using System.Windows;

namespace bookshelf_aws_app
{
    class DynamoDBBookselfOperation
    {
        AmazonDynamoDBClient client;
        DynamoDBContext context;
        Amazon.Runtime.BasicAWSCredentials credentials;

        DynamoDBOperation dynamoDBOperation = new DynamoDBOperation();
        DynamoDBUserOperation dynamoDBUserOperation = new DynamoDBUserOperation();

        public DynamoDBBookselfOperation()
        {
            credentials = new Amazon.Runtime.BasicAWSCredentials(ConfigurationManager.AppSettings["accessId"], ConfigurationManager.AppSettings["secretKey"]);
            client = new AmazonDynamoDBClient(credentials, Amazon.RegionEndpoint.USEast1);
            context = new DynamoDBContext(client);
        }

        // Create a bookshelf table
        public async Task CreateBookshelfTableAsync()
        {
            string tableName = "Bookshelf";

            // if table already exists, return
            if (await dynamoDBOperation.DoesTableExistAsync(tableName))
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
                        AttributeType="N"
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

        // Insert books
        public async Task InsertBooks()
        {
            // create a book list
            List<Book> books = CreateBooksList();

            // Get all users from user table
            List<User> users = await dynamoDBUserOperation.GetAllUsersAsync();

            try
            {
                foreach (var user in users)
                {
                    var existingBookshelf = await context.LoadAsync<Bookshelf>(user.Id);
                    HashSet<string> existingBookTitles = new HashSet<string>();

                    if (existingBookshelf != null)
                    {
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

                MessageBox.Show("Bookshelf insertion successful", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception e)
            {
                MessageBox.Show("Bookshelf insertion failed" + e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task AddLastViewedPageNumber(int userId, string selectedTitle, int lastViewedPageNumber, DateTime closingTime)
        {
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

                        MessageBox.Show("Your book on the Bookshelf was updated successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Book not found in the bookshelf.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Bookshelf not found for the given user ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error updating bookshelf: " + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Create a list of random bookList
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
                Authors = new List<string> { "F. Scott Fitzgerald" },
                LastViewedPage = 1,
                ClosingTime = DateTime.UtcNow.ToString()
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
            // Query the Bookshelf table using the userId (assuming userId is the partition key)
            var bookshelf = await context.LoadAsync<Bookshelf>(userId);

            return bookshelf;
        }
    }
}
