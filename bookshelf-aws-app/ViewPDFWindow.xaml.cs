using Amazon.DynamoDBv2.Model;
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
using Syncfusion.Windows.PdfViewer;
using System.IO;
using Syncfusion.Licensing;
using System.Configuration;

namespace bookshelf_aws_app
{
    /// <summary>
    /// Interaction logic for ViewPDFWindow.xaml
    /// </summary>
    public partial class ViewPDFWindow : Window
    {
        DynamoDBOperation dynamoDBOperation = new DynamoDBOperation();
        // Property to hold the PDF document stream
        public MemoryStream DocumentStream { get; set; }

        public ViewPDFWindow()
        {
        }

        public ViewPDFWindow(string title)
        {
            SyncfusionLicenseProvider.RegisterLicense(ConfigurationManager.AppSettings["syncfusionlicense"]);
            Title = title;
            InitializeComponent();

            LoadPDF(title);
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
    } 
}
