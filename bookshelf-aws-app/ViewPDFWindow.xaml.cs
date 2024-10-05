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
    public class ViewPDFWindow : Window
    {
        DynamoDBOperation dynamoDBOperation = new DynamoDBOperation();
        DynamoDBBookshelfOperation dynamoDBBookselfOperation = new DynamoDBBookshelfOperation();
        DynamoDBUserOperation dynamoDBUserOperation = new DynamoDBUserOperation();
        BookshelfWindow BookshelfWindow = new BookshelfWindow();


        public MemoryStream DocumentStream { get; set; }
        public string Username { get; set; }
        public string Title { get; set; }
        public int lastViewedPage;

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

            // Load the last viewed page from the current bookshelf's specific book
            lastViewedPage = GetLastViewedPageFromBookshelf(app.CurrentBookshelf, Title);

            // load the PDF from s3 bucket
            LoadPDF(title);
            // track page changes
            PDFViewer.CurrentPageChanged += PDFViewer_CurrentPageChanged;
        }

        // get the last viewed page from the bookshelf
        private int GetLastViewedPageFromBookshelf(Bookshelf bookshelf, string title)
        {
            // Find the book with the specified title
            var book = bookshelf.Books.FirstOrDefault(b => b.Title.Equals(title, StringComparison.OrdinalIgnoreCase));

            // Return the LastViewedPage or 0 if the book is not found
            return book?.LastViewedPage ?? 0; 
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
                    PDFViewer.GotoPage(lastViewedPage);
                }
                else
                {
                    Debug.WriteLine("Failed to load PDF document.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error to load PDF: {ex.Message}");
            }

        }

        // ||||||||||||||||  SAVING LAST PAGE VIEWED  ||||||||||||||||||  //
        private void PDFViewer_CurrentPageChanged(object sender, EventArgs e)
        {
            // Small delay to allow the page to update properly
            //Task.Delay(6000);
            lastViewedPage = PDFViewer.CurrentPage;
        }

        protected override async void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            DateTime closingTime = DateTime.Now;        

            // Save the current page number to persistent storage before closing
            await SaveLastPage(lastViewedPage, closingTime);

            BookshelfWindow bookshelfWindow = new BookshelfWindow(Username);
            bookshelfWindow.Show();
        }

        private async Task SaveLastPage(int pageNumber, DateTime closingTime)
        {
            try
            {
                // Access the current user from the global application instance
                var app = (App)Application.Current;
                User user = app.CurrentUser;

                if (user == null)
                {
                    await Task.Delay(5000);
                }

                Debug.WriteLine($"Last page viewed: {pageNumber}, for userid: {user.Id}");

                await dynamoDBBookselfOperation.AddLastViewedPageNumber(user.Id, Title, pageNumber, closingTime);

            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error saving last page viewed: {e}");
            }            
        }
    }     
}
