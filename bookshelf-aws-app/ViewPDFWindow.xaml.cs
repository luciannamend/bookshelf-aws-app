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
    /// Interaction logic for ViewPDFWindow.xaml
    /// </summary>
    public partial class ViewPDFWindow : Window
    {
        DynamoDBOperation dynamoDBOperation = new DynamoDBOperation();
        DynamoDBBookshelfOperation dynamoDBBookselfOperation = new DynamoDBBookshelfOperation();
        DynamoDBUserOperation dynamoDBUserOperation = new DynamoDBUserOperation();
        BookshelfWindow BookshelfWindow = new BookshelfWindow();

        // Property to hold the PDF document stream
        public MemoryStream DocumentStream { get; set; }

        private int currentPageNumber;
        public string Username { get; set; }
        public string Title { get; set; }

        public ViewPDFWindow() 
        {
            InitializeComponent();
        }

        public ViewPDFWindow(string title, string username) : this()
        {
            var app = (App)Application.Current;

            SyncfusionLicenseProvider.RegisterLicense(ConfigurationManager.AppSettings["syncfusionlicense"]);
            Username = username;
            Title = title;

            LoadPDF(title);
            PDFViewer.CurrentPageChanged += PDFViewer_CurrentPageChanged;
        }

        public async void LoadPDF(string title)
        {
            string bucketName = "bookshelf-app-book-list";
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
                    PDFViewer.GotoPage(currentPageNumber);
                }
                else
                {
                    MessageBox.Show("Failed to load PDF document.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }

        }

        // ||||||||||||||||  SAVING LAST PAGE VIEWED  ||||||||||||||||||  //
        private void PDFViewer_CurrentPageChanged(object sender, EventArgs e)
        {
            Task.Delay(6000);  // Small delay to allow the page to update properly
            currentPageNumber = PDFViewer.CurrentPage;
        }

        protected override async void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            DateTime closingTime = DateTime.Now;

            // Save the current page number to persistent storage before closing
            await SaveLastPage(currentPageNumber, closingTime);

            BookshelfWindow bookshelfWindow = new BookshelfWindow(Username);
            bookshelfWindow.Show();
        }

        private async Task SaveLastPage(int pageNumber, DateTime closingTime)
        {
            try
            {
                MessageBox.Show($"Attempting to save page number: {pageNumber}"); //debug

                User user = await dynamoDBUserOperation.GetUserByUsername(Username);

                if (user == null)
                {
                    await Task.Delay(5000);
                }

                MessageBox.Show($"Last page viewed: {pageNumber}, for userid: {user.Id}");

                await dynamoDBBookselfOperation.AddLastViewedPageNumber(user.Id, Title, pageNumber, closingTime);

            }
            catch (Exception e)
            {
                MessageBox.Show("Error saving last page viewed: " + e);
            }
        }
    }
}