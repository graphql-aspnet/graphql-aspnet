// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Resolvers
{
    using System;
    using System.Reflection;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A helper class for generating valid metadata for introspection and other internal value resolvers
    /// </summary>
    internal static class InternalFieldResolverMetaData
    {
        /// <summary>
        /// Creates a metadata item for a method that will function correctly for any calls, checks or log entries. This
        /// metadata points to a method that would never be invoked.
        /// </summary>
        /// <param name="owningType">The type that will masqurade as "owning" the resolver.</param>
        /// <returns>IGraphFieldResolverMetaData.</returns>
        public static IGraphFieldResolverMetaData CreateMetadata(Type owningType)
        {
            Validation.ThrowIfNull(owningType, nameof(owningType));

            var methodInfo = typeof(InternalFieldResolverMetaData)
                .GetMethod(nameof(InternalValueResolver), BindingFlags.Static | BindingFlags.NonPublic);

            return new FieldResolverMetaData(
                methodInfo,
                new FieldResolverParameterMetaDataCollection(),
                typeof(int),
                false,
                nameof(InternalValueResolver),
                $"{owningType.FriendlyName(true)}.{nameof(InternalValueResolver)}",
                owningType,
                owningType.FriendlyName(),
                owningType.FriendlyName(true));
        }

        private static int InternalValueResolver()
        {
            return 0;
        }
    }
}