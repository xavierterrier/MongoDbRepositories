using System;
using Com.Xterr.MongoDbRepositories.Model;

namespace Com.Xterr.MongoDbRepositories.Exceptions
{
    public class ValidationException : Exception
    {

        public ValidationException(ValidationError errorsDto)
        {
            this.ErrorsDto = errorsDto;
        }

        public ValidationException(string field, string message)
        {
            this.ErrorsDto = new ValidationError();
            this.ErrorsDto.InvalidInputs.Add(field, message);
        }

        public ValidationError ErrorsDto { get; private set; }
    }
}
