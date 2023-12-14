using MDRCloudServices.Interfaces;

namespace MDRCloudServices.DataLayer.Models;

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

    public ValidationException(string Message, Exception innerException) : base(Message, innerException)
    {
    }

    public ValidationException(string Message) : base(Message)
    {
    }

    public ValidationException()
    { }
}
