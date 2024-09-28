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
            try 
            {
                // create user table
                await dynamoDBOperation.CreateUserTable();

                // check if it is created
                await dynamoDBOperation.WaitForTableToBeActive("User");

                // if ready, create three users
                CreateThreeUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating User table: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CreateThreeUsers() 
        {
            List<User> users = new List<User>();

            users.Add(new User { Id = "1", UserName = "user1", Password = "password1" });
            users.Add(new User { Id = "2", UserName = "user2", Password = "password2" });
            users.Add(new User { Id = "3", UserName = "user3", Password = "password3" });

            foreach (User user in users)
            {
                await dynamoDBOperation.CreateUser(user.Id, user.UserName, user.Password);
            }

            MessageBox.Show("Three users created successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Create login button click event
        private async void CreateLoginButton_Click(object sender, RoutedEventArgs e)
        {
            String username = UserNameText.Text;
            String password = UserPasswordText.Text;
            String id = Guid.NewGuid().ToString();

            if (username == "" || password == "")
            {
                MessageBox.Show("Please enter a valid username and password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //  TODO:
            //  CHECK IF THE USERNAME ALREADY EXISTS
            //  IF YES, SHOW MESSAGE 
            //  ELSE CREATE THE USER


            await dynamoDBOperation.CreateUser(id, username, password);          

            MessageBox.Show("UserName and Password created successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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