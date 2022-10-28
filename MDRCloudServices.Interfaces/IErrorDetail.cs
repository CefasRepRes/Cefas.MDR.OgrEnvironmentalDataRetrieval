using System.Net;

namespace MDRCloudServices.Interfaces;

public interface IErrorDetail
{
    HttpStatusCode StatusCode { get; set; }
    string? Description { get; set; }
    string? Source { get; set; }
    object? SourceId { get; set; }
}
