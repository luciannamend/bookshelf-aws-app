﻿using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bookshelf_aws_app
{
    [DynamoDBTable("User")]
    public class User
    {
        //Partition key
        [DynamoDBHashKey("Id")]
        public int Id {get; set;}

        [DynamoDBProperty("UserName")]
        public string UserName {get; set;}

        [DynamoDBProperty("Password")]
        public string Password {get; set;}
    }
}
