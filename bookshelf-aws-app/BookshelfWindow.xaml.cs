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

        public BookshelfWindow()
        {
            
        }

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

            await dynamoDBBookselfOperation.WaitForObjectInsertion(tableName);

            await PopulateDataGrid(Username);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ViewPDFWindow viewPDFWindow = new ViewPDFWindow();
            viewPDFWindow.Show();
            this.Close();
        }

        private async Task PopulateDataGrid(string username)
        {
            User retreivedUser = await dynamoDBUserOperation.GetUserByUsername(username);

            string userId = retreivedUser.Id;

            List<Book> bookList = await dynamoDBBookselfOperation.GetBooksByUser(userId);

            BookshelfDataGrid.ItemsSource = bookList;
        }
    }
}
