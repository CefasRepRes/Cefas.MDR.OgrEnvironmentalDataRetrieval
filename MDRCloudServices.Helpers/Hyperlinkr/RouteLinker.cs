using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MDRCloudServices.Helpers.Hyperlinkr;

/// <summary>
/// Creates URIs from type-safe expressions, based on routing configuration.
/// </summary>
/// <remarks>
/// <para>
/// The purpose of this class is to create correct URIs to other resources within an ASP.NET
/// Web API solution. Instead of hard-coding URIs or building them from hard-coded URI
/// templates which may go out of sync with the routes defined in an route collection,
/// the RouteLinker class provides a method where URIs can be built from the routes
/// defined in the route collection.
/// </para>
/// </remarks>
/// <seealso cref="GetUri{T}(Expression{Action{T}})" />
public class RouteLinker : IResourceLinker
{
    private readonly IRouteValuesQuery valuesQuery;
    private readonly IRouteDispatcher dispatcher;
    private readonly IUrlHelper helper;

    /// <summary>
    /// Initializes a new instance of the <see cref="RouteLinker"/> class.
    /// </summary>
    /// <param name="helper">Instance of IUrlHelper</param>
    public RouteLinker(IUrlHelper helper)
        : this(helper, new DefaultRouteDispatcher())
    {
    }

    /// <summary>Initializes a new instance of the <see cref="RouteLinker" /> class.</summary>
    /// <param name="helper">The current request.</param>
    /// <param name="routeValuesQuery">A Strategy for extracting route values.</param>
    public RouteLinker(IUrlHelper helper, IRouteValuesQuery routeValuesQuery)
        : this(helper, routeValuesQuery, new DefaultRouteDispatcher())
    {
    }

    /// <summary>Initializes a new instance of the <see cref="RouteLinker"/> class.</summary>
    /// <param name="helper">The current URL helper.</param>
    /// <param name="dispatcher">A custom dispatcher.</param>
    public RouteLinker(IUrlHelper helper, IRouteDispatcher dispatcher)
        : this(helper, new ScalarRouteValuesQuery(), dispatcher)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="RouteLinker" /> class.</summary>
    /// <param name="helper">The current URL helper.</param>
    /// <param name="routeValuesQuery">A Strategy for extracting route values.</param>
    /// <param name="dispatcher">A custom dispatcher.</param>
    /// <remarks>
    /// <para>
    /// This constructor overload requires custom Strategies to be
    /// injected. If you don't want to supply one or both custom
    /// Strategies, you can use a simpler constructor overload.
    /// </para>
    /// <para>
    /// After initialization, the parameter values are available as
    /// read-only properties.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// request is null
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// routeValuesQuery is null
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// dispatcher is null
    /// </exception>
    public RouteLinker(
        IUrlHelper helper,
        IRouteValuesQuery routeValuesQuery,
        IRouteDispatcher dispatcher)
    {
        this.helper = helper ?? throw new ArgumentNullException(nameof(helper));
        valuesQuery = routeValuesQuery ?? throw new ArgumentNullException(nameof(routeValuesQuery));
        this.dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    /// <summary>Creates an URI based on a type-safe expression.</summary>
    /// <typeparam name="T">
    /// The type of resource to link to. This will typically be the type of an
    /// <see cref="ControllerBase" />, but doesn't have to be.
    /// </typeparam>
    /// <typeparam name="TResult">
    /// The return type of the Action Method of the resource.
    /// </typeparam>
    /// <param name="method">
    /// An expression which identifies the action method that serves the desired resource.
    /// </param>
    /// <returns>
    /// An <see cref="Uri" /> instance which represents the resource identified by
    /// <paramref name="method" />.
    /// </returns>
    public Uri GetUri<T, TResult>(Expression<Func<T, TResult>> method)
    {
        var methodCallExp = method.GetMethodCallExpression();
        return GetUri(methodCallExp);
    }

    /// <summary>
    /// Creates an URI based on a type-safe expression.
    /// </summary>
    /// <typeparam name="T">
    /// The type of resource to link to. This will typically be the type of an
    /// <see cref="ControllerBase" />, but doesn't have to be.
    /// </typeparam>
    /// <param name="method">
    /// An expression which identifies the action method that serves the desired resource.
    /// </param>
    /// <returns>
    /// An <see cref="Uri" /> instance which represents the resource identifed by
    /// <paramref name="method" />.
    /// </returns>
    /// <example>
    /// This example demonstrates how to create an <see cref="Uri" /> instance for a GetById
    /// method defined on a FooController class.
    /// <code>
    /// var uri = linker.GetUri&lt;FooController&gt;(r => r.GetById(1337));
    /// </code>
    /// Given the default API route configuration, the resulting URI will be something like
    /// this (assuming that the base URI is http://localhost): http://localhost/api/foo/1337
    /// </example>
    public Uri GetUri<T>(Expression<Action<T>> method)
    {
        var methodCallExp = method.GetMethodCallExpression();
        return GetUri(methodCallExp);
    }

    /// <summary>
    /// Creates an URI based on a type-safe expression.
    /// </summary>
    /// <typeparam name="T">
    /// The type of resource to link to. This will typically be the type of
    /// an <see cref="Controller" />, but doesn't have
    /// to be.
    /// </typeparam>
    /// <typeparam name="TResult">
    /// The return type of the Action Method of the resource.
    /// </typeparam>
    /// <param name="method">
    /// An expression which identifies the action method that serves the
    /// desired resource.
    /// </param>
    /// <returns>
    /// A <see cref="Task{Uri}" /> instance which represents the resource
    /// identified by <paramref name="method" />.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is used to build valid URIs for resources represented
    /// by code. In the ASP.NET Web API, resources are served by Action
    /// Methods on Controllers. If building a REST service with hypermedia
    /// controls, you will want to create links to various other resources
    /// in your service. Viewed from code, these resources are encapsulated
    /// by Action Methods, but you need to build valid URIs that, when
    /// requested via HTTP, invokes the desired Action Method.
    /// </para>
    /// <para>
    /// The target Action Method can be type-safely identified by the
    /// <paramref name="method" /> expression.
    /// The <typeparamref name="T" /> type argument will typically indicate
    /// a particular class which derives from
    /// <see cref="ControllerBase" />, but there's no
    /// generic constraint on the type argument, so this is not required.
    /// </para>
    /// <para>
    /// Based on the Action Method identified by the supplied expression,
    /// the ASP.NET Web API routing configuration is consulted to build an
    /// appropriate URI which matches the Action Method. The routing
    /// configuration is pulled from the <see cref="IUrlHelper" />
    /// instance supplied to the constructor of the
    /// <see cref="RouteLinker" /> class.
    /// </para>
    /// <para>
    /// This overload supports extracting valid URI instances from async
    /// Controllers.
    /// </para>
    /// </remarks>
    /// <seealso cref="GetUri{T}(Expression{Action{T}})" />
    /// <seealso cref="GetUri{T, TResult}(Expression{Func{T, TResult}})" />
    /// <exception cref="ArgumentNullException">method is null</exception>
    /// <exception cref="ArgumentException">The expression's body isn't a MethodCallExpression. The code block supplied should invoke a method.\nExample: x => x.Foo().</exception>
    /// <example>
    /// This example demonstrates how to create an <see cref="Uri" />
    /// instance for a Get method defined on an AsyncController class.
    /// <code>
    /// Uri actual = linker.GetUriAsync((AsyncController c) => c.Get(id)).Result;
    /// </code>
    /// Given the default API route configuration, the resulting URI will
    /// be something like this (assuming that the base URI is
    /// http://localhost): http://localhost/api/async/1337
    /// </example>
    public Uri GetUri<T, TResult>(Expression<Func<T, Task<TResult>>> method)
    {
        var methodCallExp = method.GetMethodCallExpression();
        return GetUri(methodCallExp);
    }

    private Uri GetUri(MethodCallExpression methodCallExp)
    {
        var r = Dispatch(methodCallExp);
        var link = helper.Link(r.RouteName, r.RouteValues);
        if (link == null)
            throw new InvalidOperationException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    "The route string returned by Route(string, IDictionary<string, object>) is null, which indicates an error. This can happen if the Action Method identified by the RouteLinker.GetUri method doesn't have a matching route with the name \"{0}\", or if the route parameter names don't match the method arguments.",
                    r.RouteName));

        return new Uri(link);
    }

    private Rouple Dispatch(MethodCallExpression methodCallExp)
    {
        var routeValues = valuesQuery.GetRouteValues(methodCallExp);
        return dispatcher.Dispatch(methodCallExp, routeValues);
    }
}
