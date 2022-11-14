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
    using System.Collections.Concurrent;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// Extension methods for working with <see cref="ISchema"/>.
    /// </summary>
    public static class SchemaExtensions
    {
        /// <summary>
        /// Returns the value of the given schema used to uniquely qualify it amongst all other
        /// schema type's that may exist in a set of registered subscription events.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <returns>System.String.</returns>
        public static string FullyQualifiedSchemaTypeName(this ISchema schema)
        {
            if (schema == null)
                return null;

            return RetrieveFullyQualifiedTypeName(schema.GetType());
        }

        /// <summary>
        /// Retrieves the name of the fully qualified data object type passed as part of any raised subscription event.
        /// </summary>
        /// <param name="dataObjectType">Type of the data object.</param>
        /// <returns>System.String.</returns>
        public static string RetrieveFullyQualifiedTypeName(Type dataObjectType)
        {
            if (dataObjectType == null)
                return null;

            return dataObjectType.AssemblyQualifiedName;
        }
    }
}