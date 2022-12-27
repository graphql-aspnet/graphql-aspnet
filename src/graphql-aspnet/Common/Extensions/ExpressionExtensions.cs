// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Common.Extensions
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Extension methods for working with Expression trees.
    /// </summary>
    internal static class ExpressionExtensions
    {
        /// <summary>
        /// Retrieves the Type declared <see cref="PropertyInfo"/> expressed by the
        /// provided expression.
        /// </summary>
        /// <typeparam name="T">The type to extract the property from.</typeparam>
        /// <param name="expression">The expression inidcating which property to extract.</param>
        /// <returns>PropertyInfo.</returns>
        public static PropertyInfo RetrievePropertyInfo<T>(Expression<Func<T, object>> expression)
        {
            switch (expression?.Body)
            {
                case UnaryExpression unaryExp when unaryExp.Operand is MemberExpression memberExp:
                    return (PropertyInfo)memberExp.Member;

                case MemberExpression memberExp:
                    return (PropertyInfo)memberExp.Member;
            }

            return null;
        }

        /// <summary>
        /// Retrieves the Type declared <see cref="MethodInfo"/> expressed by the
        /// provided expression.
        /// </summary>
        /// <typeparam name="T">The type to extract the method info from.</typeparam>
        /// <param name="expression">The expression inidcating which method group to extract.</param>
        /// <returns>PropertyInfo.</returns>
        public static MethodInfo RetrieveMethodInfo<T>(Expression<Func<T, Delegate>> expression)
        {
            var unaryExpression = expression.Body as UnaryExpression;
            if (unaryExpression?.Operand is MethodCallExpression mce)
            {
                if (mce.Object is ConstantExpression ce)
                    return ce.Value as MethodInfo;
            }

            return null;
        }
    }
}