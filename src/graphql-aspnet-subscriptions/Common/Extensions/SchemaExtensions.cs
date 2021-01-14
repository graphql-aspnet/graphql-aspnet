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

            return RetrieveFullyQualifiedSchemaTypeName(schema.GetType());
        }

        /// <summary>
        /// Retrieves the value used for a fully qualified schema type name from the schema's
        /// raw <see cref="Type"/>.
        /// </summary>
        /// <param name="schemaType">The type of the schema.</param>
        /// <returns>System.String.</returns>
        public static string RetrieveFullyQualifiedSchemaTypeName(Type schemaType)
        {
            if (schemaType == null)
                return null;

            return schemaType.AssemblyQualifiedName;
        }

        /// <summary>
        /// Retrieves the name of the fully qualified data object type passed as part of any raised subscription event.
        /// </summary>
        /// <param name="dataObjectType">Type of the data object.</param>
        /// <returns>System.String.</returns>
        public static string RetrieveFullyQualifiedDataObjectTypeName(Type dataObjectType)
        {
            if (dataObjectType == null)
                return null;

            return dataObjectType.AssemblyQualifiedName;
        }
    }
}