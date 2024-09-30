using System;
using System.Collections.Generic;
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
    /// Interaction logic for BookshelfWindow.xaml
    /// </summary>
    public partial class BookshelfWindow : Window
    {

        DynamoDBOperation dynamoDBOperation = new DynamoDBOperation();
        DynamoDBBookselfOperation dynamoDBBookselfOperation = new DynamoDBBookselfOperation();
        DynamoDBUserOperation dynamoDBUserOperation = new DynamoDBUserOperation();

        public string Username { get; }

        public BookshelfWindow() {}

        public BookshelfWindow(string username)
        {
            Username = username;

            InitializeComponent();
            InitializeDynamoDB();
        }

        private async void InitializeDynamoDB()
        {
            string tableName = "Bookshelf";

            await dynamoDBBookselfOperation.CreateBookshelfTableAsync();

            await dynamoDBOperation.WaitForTableToBeActiveAsync(tableName);

            await dynamoDBBookselfOperation.InsertBooks();

            await PopulateDataGrid(Username);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Book selectedBook = (Book)BookshelfDataGrid.SelectedItem;

            if (selectedBook == null)
            {
                MessageBox.Show("Please select a book to read");
                return;
            }

            MessageBox.Show("Opening book: \n" + selectedBook.Title);

            ViewPDFWindow viewPDFWindow = new ViewPDFWindow(selectedBook.Title);
            viewPDFWindow.Show();
            this.Close();
        }

        private async Task PopulateDataGrid(string username)
        {
            // get the user
            User retreivedUser = await dynamoDBUserOperation.GetUserByUsername(username);
            // and its id
            string userId = retreivedUser.Id;
            // get the list of books by user id
            List<Book> bookList = await dynamoDBBookselfOperation.GetBooksByUser(userId);
            // display on data grid
            BookshelfDataGrid.ItemsSource = bookList;
        }
    }
}
