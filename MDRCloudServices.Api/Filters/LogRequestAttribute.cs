using System.Diagnostics;
using MDRDB.Service;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using NPoco;

namespace MDRCloudServices.Api.Filters;

[AttributeUsage(AttributeTargets.Method)]
public class LogRequestAttribute : Attribute, IFilterFactory
{
    public bool IsReusable => false;

    public LogRequestAttribute()
    {
        ObjectType = string.Empty;
        ObjectVerb = string.Empty;
    }

    public LogRequestAttribute(string objectType, string objectVerb)
    {
        ObjectType = objectType;
        ObjectVerb = objectVerb;
    }

    /// <summary>The type of object being recorded</summary>
    public string ObjectType { get; set; }

    /// <summary>The action on the object</summary>
    public string ObjectVerb { get; set; }

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        var db = serviceProvider.GetService(typeof(IDatabase)) as IDatabase;

        return new LogRequestFilterAttribute(db, ObjectType, ObjectVerb);
    }
}

/// <summary>Action filter to log request to database</summary>
public class LogRequestFilterAttribute : ActionFilterAttribute
{
    private readonly IDatabase? _db;

    public LogRequestFilterAttribute()
    {
        _db = null;
        ObjectType = string.Empty;
        ObjectVerb = string.Empty;
    }

    public LogRequestFilterAttribute(IDatabase? db, string objectType, string objectVerb)
    {
        _db = db;
        ObjectType = objectType;
        ObjectVerb = objectVerb;
    }

    /// <summary>The type of object being recorded</summary>
    public string ObjectType { get; set; }

    /// <summary>The action on the object</summary>
    public string ObjectVerb { get; set; }

    private Stopwatch? Stopwatch { get; set; }

    /// <summary>On Action Executing event handler</summary>
    /// <param name="context"></param>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        Stopwatch = new Stopwatch();
        Stopwatch.Start();

        base.OnActionExecuting(context);
    }

    /// <summary>On Action Executed event handler</summary>
    /// <param name="context"></param>
    public override void OnResultExecuted(ResultExecutedContext context)
    {
        Stopwatch?.Stop();
        var actionLog = new ActionLog();

        actionLog.ResponseTime = Stopwatch?.Elapsed.TotalSeconds;
        actionLog.ObjectType = ObjectType;
        actionLog.ObjectVerb = ObjectVerb;
#if INTERNAL
        actionLog.Internal = true;
#else
        actionLog.Internal = false;
#endif
        // My exception log was reporting errors here, I can't reproduce them but I'll go ultra-defensive on these calls just in case.
        try
        {
            actionLog.Url = $"{context.HttpContext.Request.Host}{context.HttpContext.Request.Path}";
        }
        catch
        {
            actionLog.Url = "UNKNOWN";
        }
        try
        {
            actionLog.Method = context.HttpContext.Request.Method.ToString();
        }
        catch
        {
            actionLog.Method = "UNKNOWN";
        }
        try
        {
            actionLog.IpAddress = context.HttpContext.Connection?.RemoteIpAddress?.ToString() ?? "UNKNOWN";
        }
        catch
        {
            actionLog.IpAddress = "UNKNOWN";
        }
        actionLog.Date = DateTime.UtcNow;
        try
        {
            actionLog.StatusCode = context.HttpContext.Response.StatusCode;
        }
        catch
        {
            actionLog.StatusCode = -1;
        }

        ExtractObjectId(context, actionLog);

        try
        {
            context.HttpContext.Items.TryGetValue("Param", out var param);
            actionLog.AdditionalParam = (param ?? string.Empty).ToString();
        }
        catch
        {
            actionLog.AdditionalParam = null;
        }

        actionLog.UserAgent = GetHeaderValue(context.HttpContext.Request.Headers, "User-Agent");

        actionLog.Origin = GetHeaderValue(context.HttpContext.Request.Headers, "Origin");

        actionLog.Referer = GetHeaderValue(context.HttpContext.Request.Headers, "Referer");

        if (_db != null)
        {
            _db.OpenSharedConnection();
            // Using Insert instead of InsertAsync as using async causes exceptions
            // due to the connection being closed.
            _db.Insert(actionLog);
            _db.CloseSharedConnection();
        }
    }

    private static void ExtractObjectId(ResultExecutedContext context, ActionLog actionLog)
    {
        try
        {
            var routes = context.RouteData.Values.ToDictionary(x => x.Key.ToLowerInvariant(), x => x.Value);
            var parameters = context.HttpContext.Items.ToDictionary(x => (x.Key.ToString() ?? string.Empty).ToLowerInvariant(), x => x.Value);

            if (routes.TryGetValue("id", out var value))
            {
                if (value.CanBeCastTo(out int num))
                {
                    actionLog.ObjectId = num;
                }
                else if (value.CanBeCastTo(out string str) && int.TryParse(str, out var i))
                {
                    actionLog.ObjectId = i;
                }
                else
                {
                    actionLog.ObjectId = null;
                }
            }
            else if (parameters.TryGetValue("id", out var value2))
            {
                if (value2.CanBeCastTo(out int num2))
                {
                    actionLog.ObjectId = num2;
                }
                else if (value2.CanBeCastTo(out string str) && int.TryParse(str, out var i))
                {
                    actionLog.ObjectId = i;
                }
                else
                {
                    actionLog.ObjectId = null;
                }
            }
            else actionLog.ObjectId = null;
        }
        catch
        {
            actionLog.ObjectId = null;
        }
    }

    private static string? GetHeaderValue(IHeaderDictionary headers, string headerKey)
    {
        string? headerValue = null;
        try
        {
            if (headers.TryGetValue(headerKey, out var value))
            {
                var userAgentValues = value;
                headerValue = userAgentValues.Equals(StringValues.Empty) ? "UNKNOWN" : userAgentValues.ToString();
            }
        }
        catch
        {
            headerValue = null;
        }

        return headerValue;
    }
}
