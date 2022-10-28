using System.Collections;
using System.Net;
using System.Runtime.Serialization;
using MDRCloudServices.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MDRCloudServices.DataLayer.Models;

[DataContract]
[KnownType(typeof(ErrorDetail))]
public class ErrorResponse : IErrorResponse
{
    [DataMember]
    public bool HasErrors
    {
        get { return Errors.Any(); }
        set { throw new NotImplementedException(); }
    }

    public IActionResult ToActionResult() => new ObjectResult(Errors.Select(x => (ErrorDetail)x)) { StatusCode = (int)ResponseCode };

    // get the most common response code returned from the individual errors
    public HttpStatusCode ResponseCode => Errors
        .GroupBy(x => x.StatusCode)
        .OrderByDescending(x => x.Count())
        .SelectMany(x => x)
        .First()
        .StatusCode;

    private List<IErrorDetail> Errors { get; set; } = new List<IErrorDetail>();

    public void Add(HttpStatusCode responsecode, string description, string? source, object? sourceid)
    {
        Errors.Add(new ErrorDetail()
        {
            StatusCode = responsecode,
            Description = description,
            Source = source,
            SourceId = sourceid
        });
    }

    public void Add(IErrorDetail detail)
    {
        if (detail != null) Errors.Add(detail);
    }

    public void AddRange(IEnumerable<IErrorDetail> errorDetails)
    {
        if (errorDetails != null) Errors.AddRange(errorDetails);
    }

    public IEnumerator GetEnumerator()
    {
        return ((IEnumerable)Errors).GetEnumerator();
    }

    IEnumerator<IErrorDetail> IEnumerable<IErrorDetail>.GetEnumerator()
    {
        return ((IEnumerable<IErrorDetail>)Errors).GetEnumerator();
    }

    internal void Add(IErrorResponse errorResponse)
    {
        Errors.AddRange(errorResponse);
    }
}

[DataContract]
public class ErrorDetail : IErrorDetail
{
    public HttpStatusCode StatusCode { get; set; }
    [DataMember] public string? Description { get; set; }
    [DataMember] public string? Source { get; set; }
    [DataMember] public object? SourceId { get; set; }
}
