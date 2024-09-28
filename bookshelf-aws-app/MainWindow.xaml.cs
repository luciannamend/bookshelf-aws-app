using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace bookshelf_aws_app
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DynamoDBOperation dynamoDBOperation = new DynamoDBOperation();
        public MainWindow()
        {
            InitializeComponent();
            InitializeDynamoDB();
        }

        // Create the User table programatically
        private async void InitializeDynamoDB()
        {
            await dynamoDBOperation.CreateUserTable();
        }


        // Create login button click event
        private async void CreateLoginButton_Click(object sender, RoutedEventArgs e)
        {
            String username = UserNameText.Text;
            String password = UserPasswordText.Text;
            String id = Guid.NewGuid().ToString();

            await dynamoDBOperation.CreateUser(id, username, password);
        }

        // Login button click event
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate the user credentials
            string username = UserNameText.Text;
            string password = UserPasswordText.Text;

            // Check if the user credentials are valid
            bool isUserValid = await ValidateUserCredentials(username, password);

            // If invalid, return
            if (isUserValid == false)
            {
                return;
            }

            // If valid, open the BookListWindow
            BookListWindow bookListWindow = new BookListWindow();
            bookListWindow.Show();
            this.Close();
        }

        // Method to validate the user credentials
        private async Task<bool> ValidateUserCredentials(string username, string password)
        {       
            User retreivedUser = await dynamoDBOperation.RetrieveUser(username);
            // Check if the user exists in the database
            if (retreivedUser != null)
            {
                // If the user exists, check if the password is correct
                if (retreivedUser.Password == password)
                {
                    // If the password is correct, return true
                    return true;
                }
            }
            // Show authentication error, return false
            MessageBox.Show("Authentication failed. Please check your username and password.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        
    }
}