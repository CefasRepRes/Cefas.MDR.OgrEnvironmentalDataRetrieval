﻿using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MDRCloudServices.Helpers.Hyperlinkr;

/// <summary>
/// Creates URIs from type-safe expressions.
/// </summary>
public interface IResourceLinker
{
    /// <summary>
    /// Creates an URI based on a type-safe expression.
    /// </summary>
    /// <typeparam name="T">
    /// The type of resource to link to. This will typically be the type of an
    /// <see cref="Microsoft.AspNetCore.Mvc.Controller" />, but doesn't have to be.
    /// </typeparam>
    /// <param name="method">
    /// An expression which identifies the action method that serves the desired resource.
    /// </param>
    /// <returns>
    /// An <see cref="Uri" /> instance which represents the resource identified by
    /// <paramref name="method" />.
    /// </returns>
    Uri GetUri<T>(Expression<Action<T>> method);

    /// <summary>
    /// Creates an URI based on a type-safe expression.
    /// </summary>
    /// <typeparam name="T">
    /// The type of resource to link to. This will typically be the type of
    /// an <see cref="Microsoft.AspNetCore.Mvc.Controller" />, but doesn't have
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
    /// An <see cref="Uri" /> instance which represents the resource
    /// identified by <paramref name="method" />.
    /// </returns>
    Uri GetUri<T, TResult>(Expression<Func<T, TResult>> method);

    /// <summary>
    /// Creates an URI based on a type-safe expression.
    /// </summary>
    /// <typeparam name="T">
    /// The type of resource to link to. This will typically be the type of
    /// an <see cref="Microsoft.AspNetCore.Mvc.Controller" />, but doesn't have
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
    Uri GetUri<T, TResult>(Expression<Func<T, Task<TResult>>> method);
}
