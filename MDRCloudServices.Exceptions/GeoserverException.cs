﻿using System;
using System.Runtime.Serialization;

namespace MDRCloudServices.Exceptions;

/// <summary>Generated when an error happens communicating with GeoServer</summary>
[Serializable]
public class GeoserverException : Exception
{
    /// <summary>Initializes a new instance of the MDRCloudServices.Exceptions.GeoserverException class</summary>
    public GeoserverException()
    {
    }

    /// <summary>Initializes a new instance of the MDRCloudServices.Exceptions.GeoserverException class with a specified error message</summary>
    /// <param name="message">The message that dedscribes the error.</param>
    public GeoserverException(string message) : base(message)
    {
    }

    /// <summary>Initializes a new instance of the MDRCloudServices.Exceptions.GeoserverException class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public GeoserverException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    /// <summary>Initializes a new instance of the MDRCloudServices.Exceptions.GeoserverException class with serialized data.</summary>
    /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual information about the source or destination.</param>
    protected GeoserverException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
