using MDRCloudServices.Interfaces;
using System;
using System.Runtime.Serialization;

namespace MDRCloudServices.DataLayer.Models;

[Serializable]
public class ValidationException : ApplicationException
{
    public IErrorResponse Result { get; set; } = new ErrorResponse();

    public ValidationException(IErrorResponse result)
    {
        Result = result;
    }

    public ValidationException(IErrorResponse result, Exception exception) : base(string.Empty, exception)
    {
        Result = result;
    }

    public ValidationException(string Message, Exception innerException) : base(Message, innerException) { }

    public ValidationException(string Message) : base(Message) { }

    public ValidationException() { }

    protected ValidationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
