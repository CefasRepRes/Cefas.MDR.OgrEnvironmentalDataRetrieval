using System;
using System.Runtime.Serialization;

namespace MDRCloudServices.Exceptions;

/// <summary>Raised when the item cannot be found</summary>
[Serializable]
public class NotFoundException : Exception
{
    /// <summary>Initializes a new instance of the MDRCloudServices.Exceptions.NotFoundException class</summary>
    public NotFoundException()
    {
    }

    /// <summary>Initializes a new instance of the MDRCloudServices.Exceptions.NotFoundException class with a specified error message</summary>
    /// <param name="message">The message that dedscribes the error.</param>
    public NotFoundException(string message) : base(message)
    {
    }

    /// <summary>Initializes a new instance of the MDRCloudServices.Exceptions.NotFoundException class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public NotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>Initializes a new instance of the MDRCloudServices.Exceptions.NotFoundException class with serialized data.</summary>
    /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual information about the source or destination.</param>
    protected NotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
