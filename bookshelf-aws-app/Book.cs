using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bookshelf_aws_app
{
    [DynamoDBTable("Bookshelf")]
    internal class Book
    {
        [DynamoDBHashKey]
        public string UserId { get; set; }

        [DynamoDBHashKey]
        public string ISBN { get; set; }

        [DynamoDBProperty("Title")]
        public string Title { get; set; }

        [DynamoDBProperty("Authors")]
        public List<string> BookAuthors { get; set; }

        [DynamoDBIgnore]
        public string CoverPage { get; set; }
    }
}
