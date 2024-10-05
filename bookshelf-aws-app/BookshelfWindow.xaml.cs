using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace bookshelf_aws_app
{
    /// <summary>
    /// Manages the user's bookshelf. It initializes the connection to a DynamoDB database to 
    /// create and maintain a bookshelf table. Upon instantiation, it checks for existing books 
    /// associated with the logged-in user and populates a data grid with this information. 
    /// The class also handles user interactions, allowing users to double-click on a book entry 
    /// to open a new window that displays the selected book's content
    /// </summary>
    public partial class BookshelfWindow : Window
    {

        DynamoDBOperation dynamoDBOperation = new DynamoDBOperation();
        DynamoDBBookshelfOperation dynamoDBBookselfOperation = new DynamoDBBookshelfOperation();
        DynamoDBUserOperation dynamoDBUserOperation = new DynamoDBUserOperation();
        private App app;
        public string tableName = "Bookshelf";

        public BookshelfWindow()
        {
            InitializeComponent();
            app = (App)Application.Current;
            InitializeDynamoDB();
        }

        // CHECK 
        public BookshelfWindow(string username) : this() { }

        private async void InitializeDynamoDB()
        {
            try
            {
                // create and wait for the Bookshelf table to be active
                await dynamoDBBookselfOperation.CreateBookshelfTableAsync();
                await dynamoDBOperation.WaitForTableToBeActiveAsync(tableName);

                // insert books into the bookshelf table
                await dynamoDBBookselfOperation.InsertBooks();

                // populate the data grid with books for the current user
                await PopulateDataGrid();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing database: {ex.Message}");
            }
        }

        // Select book click handler
        private void BooksDataGrid_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            // check if selected and open the selected book
            if (BookshelfDataGrid.SelectedItem is Book selectedBook)
            {
                // pass the user and selected book title to the ViewPDFWindow
                var viewPDFWindow = new ViewPDFWindow(selectedBook.Title, app.CurrentUser.UserName);
                viewPDFWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Please select a book to read");
            }
        }

        // populates the data grid according to the most recent read books
        public async Task PopulateDataGrid()
        {
            try
            {
                // get the user
                User retrievedUser = app.CurrentUser;

                // get the list of books by user id
                List<Book> bookList = await dynamoDBBookselfOperation.GetBooksByUser(retrievedUser.Id);

                // if there are no books
                if (bookList == null || bookList.Count == 0)
                {
                    Debug.WriteLine("No books found for the current user.");
                    return;
                }

                // sort the books by ClosingTime (most recent first)
                var sortedBookList = bookList.OrderByDescending(book => book.ClosingTime).ToList();

                // display on data grid
                BookshelfDataGrid.ItemsSource = sortedBookList;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error populating data grid: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
