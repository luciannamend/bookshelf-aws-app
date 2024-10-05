using System.Windows;
using System.IO;
using Syncfusion.Licensing;
using System.Configuration;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using System.Diagnostics;

namespace bookshelf_aws_app
{
    /// <summary>
    /// Allows users to view and interact with PDF documents stored in an AWS S3 bucket. 
    /// It uses Syncfusion's PDF viewer to display the PDF. Loads the PDF for a given user, 
    /// remembers the last page they viewed, to resume from that page. It saves the user's 
    /// progress (last viewed page) to DynamoDB when the window is closed.
    /// </summary>
    public partial class ViewPDFWindow : Window
    {
        DynamoDBOperation dynamoDBOperation = new DynamoDBOperation();
        DynamoDBBookshelfOperation dynamoDBBookselfOperation = new DynamoDBBookshelfOperation();
        DynamoDBUserOperation dynamoDBUserOperation = new DynamoDBUserOperation();
        BookshelfWindow BookshelfWindow = new BookshelfWindow();

        // Property to hold the PDF document stream
        public MemoryStream DocumentStream { get; set; }

        public int CurrentPageNumber;
        public string Username { get; set; }
        public string Title { get; set; }

        public ViewPDFWindow() 
        {
            InitializeComponent();
        }

        public ViewPDFWindow(string title, string username, int lastViewedPage) : this()
        {
            var app = (App)Application.Current;

            // Register Syncfusion license using app settings
            SyncfusionLicenseProvider.RegisterLicense(ConfigurationManager.AppSettings["syncfusionlicense"]);
            Username = username;
            Title = title;
            CurrentPageNumber = lastViewedPage;

            // Load the PDF and set up event handler for page change
            LoadPDF(title);
            PDFViewer.CurrentPageChanged += PDFViewer_CurrentPageChanged;
        }

        public async void LoadPDF(string title)
        {
            // S3 bucket where PDFs are stored
            string bucketName = "bookshelf-app-book-list";

            // The key used to identify the PDF in S3
            string objectKey = title;            

            try
            {
                // Fetch the PDF from S3
                DocumentStream = await dynamoDBOperation.GetPdfFromS3Async(bucketName, objectKey);

                // Check if the DocumentStream is not null
                if (DocumentStream != null)
                {
                    // Set the ItemSource of the PDFViewer
                    PDFViewer.ItemSource = DocumentStream;
                    // go to the last viewed page
                    PDFViewer.GotoPage(CurrentPageNumber);
                }
                else
                {
                    // Notify user if the PDF could not be loaded
                    MessageBox.Show("Failed to load PDF document.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
            }
        }

        private void PDFViewer_CurrentPageChanged(object sender, EventArgs e)
        {
            // Update the current page number based on the page the user goes to
            CurrentPageNumber = PDFViewer.CurrentPage;
        }

        // Overides onClosing event to save the last viewed page number and timestamp
        protected override async void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Record the time the window is being closed
            DateTime closingTime = DateTime.Now;

            // Save the user's last viewed page number for the PDF
            await SaveLastPage(CurrentPageNumber, closingTime);

            BookshelfWindow bookshelfWindow = new BookshelfWindow(Username);
            bookshelfWindow.Show();
        }

        // Saves the last viewed page number and timestamp to DynamoDB
        private async Task SaveLastPage(int pageNumber, DateTime closingTime)
        {
            try
            {
                // get user by username
                User user = await dynamoDBUserOperation.GetUserByUsername(Username);

                // if needed, wait for user to be retrieved
                if (user == null)
                {
                    await Task.Delay(5000);
                }

                // Save the last viewed page number to DynamoDB
                await dynamoDBBookselfOperation.AddLastViewedPageNumber(user.Id, Title, pageNumber, closingTime);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error saving last page viewed: " + e);
            }
        }
    }
}