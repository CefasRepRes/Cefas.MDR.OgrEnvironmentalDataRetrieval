﻿using System;

namespace MDRCloudServices.Exceptions;

/// <summary>Merge service exception</summary>
public class MergeServiceException : Exception
{
    /// <summary>Initializes a new instance of the MDRCloudServices.Exceptions.MergeServiceException class</summary>
    public MergeServiceException()
    {
    }

    /// <summary>Initializes a new instance of the MDRCloudServices.Exceptions.MergeServiceException class with a specified error message</summary>
    /// <param name="message">The message that dedscribes the error.</param>
    public MergeServiceException(string message) : base(message)
    {
    }

    /// <summary>Initializes a new instance of the MDRCloudServices.Exceptions.MergeServiceException class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public MergeServiceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
