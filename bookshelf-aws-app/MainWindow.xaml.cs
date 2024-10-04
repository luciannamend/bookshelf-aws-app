using System.Windows;

namespace bookshelf_aws_app
{
    /// <summary>
    /// Serves as the login interface, initializing AWS DynamoDB operations upon loading. 
    /// It creates the User table, inserts sample users, and enables the Login button only after successful setup.
    /// User logs in by entering their credentials, which are validated against the DynamoDB database;
    /// Additionally, users can create new accounts, existing usernames are checked to avoid duplicates. 
    /// </summary>
    public partial class MainWindow : Window
    {
        DynamoDBOperation dynamoDBOperation = new DynamoDBOperation();
        DynamoDBUserOperation dynamoDBUserOperation = new DynamoDBUserOperation();
        DynamoDBBookselfOperation dynamoDBBookselfOperation = new DynamoDBBookselfOperation();
        Random random = new Random();
        private TaskCompletionSource<bool> _usersCreated = new TaskCompletionSource<bool>();
        public string tableName = "User";

        public MainWindow()
        {
            InitializeComponent();
            InitializeDynamoDBAsync();
        }

        // Initialize DynamoDB async operations
        private async void InitializeDynamoDBAsync()
        {            
            try 
            {
                // create user table
                await dynamoDBUserOperation.CreateUserTableAsync();

                // check if it is created
                await dynamoDBOperation.WaitForTableToBeActiveAsync(tableName);

                // if ready, insert three users
                await dynamoDBUserOperation.CreateThreeUsersAsync();

                // set the TaskCompletionSource to true when users are successfully created
                _usersCreated.SetResult(true);

                // enable the LoginButton after users are created
                LoginButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating User table: " + ex.Message, 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                
                // set TaskCompletionSource to false on failure
                _usersCreated.SetResult(false);
            }
        }

        // CreateLogin button click handler
        private async void CreateLoginButton_Click(object sender, RoutedEventArgs e)
        {
            // get username and password
            string username = UserNameText.Text;
            string password = UserPasswordText.Text;

            // create random id
            int id = random.Next(100, 1001);

            // check if the text boxes are empty
            if (username == "" || password == "")
            {
                MessageBox.Show("Please enter a valid username and password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // check if the user exists in the database
            User retreivedUser = await dynamoDBUserOperation.GetUserByUsername(username);

            // if user exists, return
            if (retreivedUser != null)
            {
                MessageBox.Show("User already exist", "Existent user", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // create new user
            await dynamoDBUserOperation.CreateUserAsync(id, username, password); 
        }

        // Login button click handler
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // get the user credentials
            string username = UserNameText.Text;
            string password = UserPasswordText.Text;

            // Validate the user credentials
            User retrievedUser = await GetValidUser(username, password);

            if (retrievedUser == null)
            {
                // Invalid credentials
                MessageBox.Show("Invalid credentials. Please check your username and password.", 
                    "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return; 
            }

            // set the CurrentUser in the App class to make it accessible globally
            var app = (App)Application.Current;
            app.CurrentUser = retrievedUser;            

            // open the BookshelfWindow and close the login window
            BookshelfWindow bookshelfWindow = new BookshelfWindow();
            bookshelfWindow.Show();
            this.Close();
        }

        // Method to validate the user credentials
        private async Task<User> GetValidUser(string username, string password)
        {
            // get user by username
            User retreivedUser = await dynamoDBUserOperation.GetUserByUsername(username);

            // if user is not null (exists)
            if (retreivedUser != null)
            {
                // check if the password is correct
                if (retreivedUser.Password == password)
                {
                    // If the password is correct, return the existing user
                    return retreivedUser;
                }
            }
            // If the user does not exist or the password is incorrect, show an error message
            MessageBox.Show("Authentication failed. Please check your username and password.", 
                "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);

            // and return null 
            return null;
        }
    }    
}