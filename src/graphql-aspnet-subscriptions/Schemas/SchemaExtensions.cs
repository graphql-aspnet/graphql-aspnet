// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas
{
    using System;
    using System.Collections.Concurrent;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// Extension methods for working with <see cref="ISchema"/>.
    /// </summary>
    public static class SchemaExtensions
    {
        private static readonly ConcurrentDictionary<Type, string> _assemblyQualifiedNameCache;

        /// <summary>
        /// Initializes static members of the <see cref="SchemaExtensions"/> class.
        /// </summary>
        static SchemaExtensions()
        {
            _assemblyQualifiedNameCache = new ConcurrentDictionary<Type, string>();
        }

        /// <summary>
        /// Returns the value of the given schema used to uniquely qualify it amongst all other
        /// schema type's that may exist in a set of registered subscription events.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <returns>System.String.</returns>
        public static string FullyQualifiedSchemaTypeName(this ISchema schema)
        {
            return RetrieveFullyQualifiedTypeName(schema?.GetType());
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

            // the 'Type.AssemblyQualifiedName' property allocates a new
            // string on each invocation of the property
            //
            // in a high volume subscription server this produces A LOT
            // of unecessary strings and adds a lot of GC pressure unecessarily.
            //
            // Cache the results to prevent this.
            if (!_assemblyQualifiedNameCache.TryGetValue(dataObjectType, out var typeName))
            {
                typeName = dataObjectType.AssemblyQualifiedName;
                _assemblyQualifiedNameCache.TryAdd(dataObjectType, typeName);
            }

            return typeName;
        }
    }
}