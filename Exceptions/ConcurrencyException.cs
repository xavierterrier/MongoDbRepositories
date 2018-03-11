using System;

namespace Com.Xterr.MongoDbRepositories.Exceptions
{
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException(string message)
            : base(message)
        {
            
        }
    }
}
