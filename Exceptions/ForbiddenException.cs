using System;

namespace Com.Xterr.MongoDbRepositories.Exceptions
{
    public class ForbiddenException : Exception
    {

        public ForbiddenException(string message)
            : base(message)
        {

        }

    }
}
