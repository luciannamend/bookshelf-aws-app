﻿using System.Text;
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
        DynamoDBUserOperation dynamoDBUserOperation = new DynamoDBUserOperation();
        DynamoDBBookselfOperation dynamoDBBookselfOperation = new DynamoDBBookselfOperation();
        public MainWindow()
        {
            InitializeComponent();
            InitializeDynamoDBAsync();
        }

        // Create the User table programatically
        private async void InitializeDynamoDBAsync()
        {            
            try 
            {
                // create user table
                await dynamoDBUserOperation.CreateUserTableAsync();

                // check if it is created
                await dynamoDBOperation.WaitForTableToBeActiveAsync("User");

                // if ready, create three users
                dynamoDBUserOperation.CreateThreeUsersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating User table: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Create login button
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

            // Check if the user exists in the database
            User retreivedUser = await dynamoDBUserOperation.GetUserByUsername(username);
            
            if (retreivedUser != null)
            {
                MessageBox.Show("User already exist", "Existent user", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            await dynamoDBUserOperation.CreateUserAsync(id, username, password); 
        }

        // Login button
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate the user credentials
            string username = UserNameText.Text;
            string password = UserPasswordText.Text;

            // Check if the user credentials are valid
            bool isUserValid = await IsUserValidAsync(username, password);

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
        private async Task<bool> IsUserValidAsync(string username, string password)
        {       
            User retreivedUser = await dynamoDBUserOperation.GetUserByUsername(username);
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