using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MDRCloudServices.Helpers.Hyperlinkr;

/// <summary>
/// A class with extension methods for UrlHelper
/// </summary>
public static class UrlHelperExtensions
{
    /// <summary>
    /// Returns URI matching helper's request and expression using RouteLinker's default route dispatcher
    /// </summary>
    /// <typeparam name="T">A class that derives from ApiController</typeparam>
    /// <typeparam name="TResult">Any result type</typeparam>
    /// <param name="helper">Provides the requested URI via helper.Request</param>
    /// <param name="expression">Method call expression of T</param>
    /// <returns>URI for the request and controller expression. </returns>
    public static Uri GetLink<T, TResult>(this IUrlHelper helper, Expression<Func<T, TResult>> expression)
        where T : ControllerBase
    {
        if (helper == null)
            throw new ArgumentNullException(nameof(helper));

        var linker = new RouteLinker(helper);

        return linker.GetUri(expression);
    }

    /// <summary>
    /// Returns URI matching helper's request and expression using RouteLinker's default route dispatcher.
    /// </summary>
    /// <typeparam name="T">A class that derives from ApiController</typeparam>
    /// <param name="helper">Provides the requested URI via helper.Request</param>
    /// <param name="expression">Expression of T</param>
    /// <returns>URI for the request and controller expression. </returns>
    public static Uri GetLink<T>(this IUrlHelper helper, Expression<Action<T>> expression)
        where T : ControllerBase
    {
        if (helper == null)
            throw new ArgumentNullException(nameof(helper));

        var linker = new RouteLinker(helper);

        return linker.GetUri(expression);
    }

    /// <summary>
    /// Returns URI matching helper's request and expression using RouteLinker's default route dispatcher
    /// </summary>
    /// <typeparam name="T">A class that derives from ApiController</typeparam>
    /// <typeparam name="TResult">Any result type</typeparam>
    /// <param name="helper">Provides the requested URI via helper.Request</param>
    /// <param name="method">Method call expression of T</param>
    /// <returns>
    /// A <see cref="Task{Uri}" /> instance which represents the resource
    /// identified by <paramref name="method" />.
    /// </returns>
    public static Uri GetLink<T, TResult>(this IUrlHelper helper, Expression<Func<T, Task<TResult>>> method)
        where T : ControllerBase
    {
        if (helper == null)
            throw new ArgumentNullException(nameof(helper));

        var linker = new RouteLinker(helper);

        return linker.GetUri(method);
    }
}
