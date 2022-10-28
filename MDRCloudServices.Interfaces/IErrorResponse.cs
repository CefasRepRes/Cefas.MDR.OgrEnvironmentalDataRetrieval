using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace MDRCloudServices.Interfaces;

public interface IErrorResponse : IEnumerable<IErrorDetail>
{
    bool HasErrors { get; set; }
    IActionResult ToActionResult();
    HttpStatusCode ResponseCode { get; }
    void Add(HttpStatusCode responsecode, string description, string source, object? sourceid);
    void Add(IErrorDetail detail);
    void AddRange(IEnumerable<IErrorDetail> errorDetails);
}
