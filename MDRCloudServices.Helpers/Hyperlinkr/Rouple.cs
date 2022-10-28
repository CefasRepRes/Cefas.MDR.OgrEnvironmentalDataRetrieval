﻿using System;
using System.Collections.Generic;

namespace MDRCloudServices.Helpers.Hyperlinkr;

/// <summary>
/// A route tuple: a rouple - pardon the pun.
/// </summary>
/// <remarks>
/// <para>
/// This class is simply a tuple of <see cref="RouteName" /> and
/// <see cref="RouteValues" />.
/// </para>
/// </remarks>
public class Rouple
{
    private readonly string routeName;
    private readonly IDictionary<string, object> routeValues;

    /// <summary>
    /// Initializes a new instance of the <see cref="Rouple"/> class.
    /// </summary>
    /// <param name="routeName">A route name.</param>
    /// <param name="routeValues">Route values.</param>
    /// <remarks>
    /// <para>
    /// The <paramref name="routeName" /> is available after initialization
    /// via the <see cref="RouteName" /> property.
    /// </para>
    /// <para>
    /// The <paramref name="routeValues" /> are available after
    /// initialization via the <see cref="RouteValues" /> property.
    /// </para>
    /// </remarks>
    public Rouple(string routeName, IDictionary<string, object> routeValues)
    {
        this.routeName = routeName ?? throw new ArgumentNullException(nameof(routeName));
        this.routeValues = routeValues ?? throw new ArgumentNullException(nameof(routeValues));
    }

    /// <summary>
    /// Gets the route name.
    /// </summary>
    /// <seealso cref="Rouple(string, IDictionary{string, object})" />
    public string RouteName
    {
        get { return this.routeName; }
    }

    /// <summary>
    /// Gets the route values.
    /// </summary>
    /// <seealso cref="Rouple(string, IDictionary{string, object})" />
    public IDictionary<string, object> RouteValues
    {
        get { return this.routeValues; }
    }
}
