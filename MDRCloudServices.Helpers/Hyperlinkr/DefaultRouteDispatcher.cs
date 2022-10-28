using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace MDRCloudServices.Helpers.Hyperlinkr;

/// <summary>
/// The default Strategy for dispatching Action Methods to a route name, by
/// always dispatching to a single, named route.
/// </summary>
/// <seealso cref="IRouteDispatcher" />
public class DefaultRouteDispatcher : IRouteDispatcher
{
    private readonly string routeName;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultRouteDispatcher" /> class.
    /// </summary>
    public DefaultRouteDispatcher()
        : this("DefaultApi")
    {
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="DefaultRouteDispatcher" /> class with the supplied route
    /// name.
    /// </summary>
    /// <param name="routeName">
    /// The route name which will be used by the
    /// <see cref="Dispatch(MethodCallExpression, IDictionary{string, object})" />
    /// method as the <see cref="Rouple.RouteName" /> value.
    /// </param>
    /// <remarks>
    /// <para>
    /// After initialization, the <paramref name="routeName" /> value is
    /// available through the <see cref="RouteName" /> property.
    /// </para>
    /// </remarks>
    public DefaultRouteDispatcher(string routeName)
    {
        this.routeName = routeName ?? throw new ArgumentNullException(nameof(routeName));
    }

    /// <summary>
    /// Provides dispatch information based on an Action Method.
    /// </summary>
    /// <param name="method">The method expression.</param>
    /// <param name="routeValues">Route values.</param>
    /// <returns>
    /// An object containing the route name, as well as the route values.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">method</exception>
    /// <remarks>
    /// <para>
    /// The returned <see cref="Rouple.RouteName" /> will be the value of
    /// the <see cref="RouteName" /> property.
    /// </para>
    /// <para>
    /// The returned <see cref="Rouple.RouteValues" /> will be all entries
    /// of the <paramref name="routeValues" />, plus a value for an
    /// additional "controller" key, derived from
    /// <paramref name="method" />.
    /// </para>
    /// </remarks>
    public Rouple Dispatch(
        MethodCallExpression method,
        IDictionary<string, object> routeValues)
    {
        if (method == null)
            throw new ArgumentNullException(nameof(method));

        var routeAttribute = method
            .Method
            .GetCustomAttribute<RouteAttribute>(false);

        if (routeAttribute != null && routeAttribute.Name != null)
        {
            return new Rouple(routeAttribute.Name, routeValues);
        }

        var newRouteValues = new Dictionary<string, object>(routeValues);

        var controllerName = method
            .Object?
            .Type
            .Name
            .ToLowerInvariant()
            .Replace("controller", "");

        if (controllerName != null)
        {
            newRouteValues["controller"] = controllerName;
        }

        return new Rouple(routeName, newRouteValues);
    }

    /// <summary>
    /// Gets the route name.
    /// </summary>
    /// <seealso cref="DefaultRouteDispatcher(string)" />
    public string RouteName
    {
        get { return routeName; }
    }
}
