using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bookshelf_aws_app
{
    public class SessionManager
    {
        private static SessionManager _instance;

        // Properties to hold the current user and their bookshelf
        public User CurrentUser { get; set; }
        public Bookshelf CurrentBookshelf { get; set; }

        // Private constructor to prevent instantiation from other classes
        private SessionManager() { }

        // Method to get the single instance of the SessionManager class
        public static SessionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SessionManager();
                }
                return _instance;
            }
        }
    }
}
