using System;
using MongoDB.Driver;

namespace Com.Xterr.MongoDbRepositories.Exceptions
{
    public class DatabaseException : Exception
    {
        public DatabaseException(string message) : base(message)
        {

        }

        public DatabaseException(WriteConcernResult result) : base(result.LastErrorMessage)
        {

        }
    }
}

