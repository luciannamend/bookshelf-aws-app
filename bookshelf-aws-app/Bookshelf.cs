using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bookshelf_aws_app
{
    [DynamoDBTable("Bookshelf")]
    internal class Bookshelf
    {
        // Partition key
        [DynamoDBHashKey]
        public string UserId { get; set; }

        [DynamoDBProperty("Books")]
        public List<Book> Books { get; set; }
    }

    class Book
    {
        public string ISBN { get; set; }
        public string Title { get; set; }
        public List<string> Authors { get; set; }
    }
}
