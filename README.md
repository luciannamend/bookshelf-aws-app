# bookshelf-aws-app

The project demonstrates how to use AWS DynamoDB for user authentication and bookshelf management, while the books themselves are stored in an AWS S3 bucket. This eBook reader app integrates multiple AWS services, following the specified lab guidelines.

**Project Features**
1. User Authentication
- DynamoDB Table Creation: The application programmatically creates a DynamoDB table to store user credentials (username and password) using C#.
- Data Insertion: Inserts at least three login credentials into the DynamoDB table.
- Login Logic: Implements the business logic for user login.

2. Bookshelf Management
- DynamoDB Table Creation in AWS Console: A DynamoDB table named Bookshelf is created via the AWS Management Console, with each user having at least two books on their bookshelf.
- Modeling in C#: The structure of the Bookshelf table is modeled in the C# code.

3. Listing Books
- List Books: After logging in, users can view all books on their bookshelf, with the most recently read book listed at the top.
- Code Logic: The code retrieves and displays books in the correct order.

4. Updating Reading Activity
- Book Content View: Users can start reading a book by double-clicking on it.
- Bookmark Functionality: The app updates the user's reading progress (bookmarked page and time) when the user closes the reading window.
- Data Storage: The current page number, bookmark time, and other reading progress information are stored in the DynamoDB table for each book.

**Technology Stack**
  - C# with WPF for the desktop application.
  - AWS SDK for .NET to interact with DynamoDB.
  - Syncfusion.PdfViewer for displaying PDF book content.
  - AWS DynamoDB for user and bookshelf data storage.
  - AWS S3 Buckets for storing the actual book files.
    
**How to Use**
 - Login: Users must log in with their credentials stored in the DynamoDB table.
 - Create Account: Users can create a new account (empty bookshelf)
 - Bookshelf View: After logging in, the user’s bookshelf is displayed, showing all books with the most recent one at the top.
 - Reading Books: Users can double-click on a book to start reading.
 - Bookmarking Progress: The user's reading progress is automatically bookmarked when they close the reading window.
