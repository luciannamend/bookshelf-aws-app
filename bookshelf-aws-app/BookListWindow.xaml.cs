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
    /// Interaction logic for BookListWindow.xaml
    /// </summary>
    public partial class BookListWindow : Window
    {
        DynamoDBOperation dynamoDBOperation = new DynamoDBOperation();
        public BookListWindow()
        {
            InitializeComponent();
            InitializeDynamoDB();
        }

        private async void InitializeDynamoDB()
        {
            await dynamoDBOperation.CreateBookTable();
        }

    }
}
