// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    ///     Helper methods for use with <see cref="GraphInputUnion" /> metadata objects.
    /// </summary>
    public static class GraphInputUnionExtensions
    {
        /// <summary>
        ///     Returns the value of a property defined on the input union if it was supplied on the target
        ///     graphql query. If the value was not supplied, the default value will be returned.
        /// </summary>
        /// <param name="entity">The <see cref="GraphInputUnion" /> to operate on.</param>
        /// <param name="selector">The selector indicating which property to inspect.</param>
        /// <param name="fallbackValue">The fallback value to use if the target property was NOT supplied on the query.</param>
        /// <typeparam name="TType">The concrete type of <see cref="GraphInputUnion" /> being referenced.</typeparam>
        /// <typeparam name="TValue">
        ///     The type value expected to be returned from the <paramref name="selector" /> chosen property.
        ///     This should be a nullable type that matches the target <typeparamref name="TReturn" /> (e.g. <c>int?</c>).
        /// </typeparam>
        /// <typeparam name="TReturn">
        ///     The return type of this method. Generally, this may be a non-nullable
        ///     type that matches the nullable property of the union. (e.g. <c>int</c> for <c>int?</c>
        /// </typeparam>
        public static TValue ValueOrDefault<TType, TValue, TReturn>(
            this TType entity,
            Expression<Func<TType, TValue>> selector,
            TReturn fallbackValue = default)
            where TType : GraphInputUnion
        {
            return default;
        }
    }
}