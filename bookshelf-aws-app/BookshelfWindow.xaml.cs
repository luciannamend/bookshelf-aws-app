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

        public BookshelfWindow()
        {
            InitializeComponent();
            InitializeDynamoDB();
        }

        private async void InitializeDynamoDB()
        {
            await dynamoDBBookselfOperation.CreateBookshelfTableAsync();

            await dynamoDBOperation.WaitForTableToBeActiveAsync("Bookshelf");

            await dynamoDBBookselfOperation.InsertBooks();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ViewPDFWindow viewPDFWindow = new ViewPDFWindow();
            viewPDFWindow.Show();
            this.Close();
        }
    }
}
