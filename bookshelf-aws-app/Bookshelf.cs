using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bookshelf_aws_app
{
    [DynamoDBTable("Bookshelf")]
    public class Bookshelf
    {
        // Partition key
        [DynamoDBHashKey("UserId")]
        public int UserId { get; set; }

        [DynamoDBProperty("Books")]
        public List<Book> Books { get; set; }
    }

    public class Book
    {
        [DynamoDBProperty("ISBN")]
        public string ISBN { get; set; }

        [DynamoDBProperty("Title")]
        public string Title { get; set; }

        [DynamoDBProperty("Authors")]
        public List<string> Authors { get; set; }

        [DynamoDBProperty("LastViewedPage")]
        public int LastViewedPage { get; set; }

        [DynamoDBProperty("ClosingTime")]
        public string? ClosingTime { get; set; }
    }
}
