using System;

namespace Com.Xterr.MongoDbRepositories.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException()
        {
            
        }

        public NotFoundException(string msg) : base(msg)
        {

        }
    }
}
