using System;
using System.Linq.Expressions;

namespace MDRCloudServices.Helpers.Hyperlinkr;

internal static class ExpressionExtensions
{
    internal static MethodCallExpression GetMethodCallExpression(
        this LambdaExpression expression)
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));

        if (expression.Body is not MethodCallExpression methodCallExpression)
        {
            throw new ArgumentException(
                "The expression's body must be a MethodCallExpression. The code block supplied should invoke a method.\nExample: x => x.Foo().",
                nameof(expression));
        }

        return methodCallExpression;
    }
}
