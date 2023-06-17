// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Configuration;

    /// <summary>
    /// Extension methods for configuring minimal API methods as fields on the graph.
    /// </summary>
    public static partial class GraphQLMinimalApiExtensions
    {
        /// <summary>
        /// Registers a new batched type extension to a given type for the target schema.
        /// </summary>
        /// <remarks>
        /// The supplied resolver must declare a parameter that implements an <see cref="IEnumerable{T}"/>
        /// for the supplied <typeparamref name="TType"/>.
        /// </remarks>
        /// <typeparam name="TType">The concrete interface, class or struct to extend with a new field.</typeparam>
        /// <param name="schemaOptions">The configuration options for the target schema.</param>
        /// <param name="fieldName">Name of the field to add to the <typeparamref name="TType"/>.</param>
        /// <param name="resolverMethod">The resolver method to be called when the field is requested.</param>
        /// <returns>IGraphQLResolvedFieldTemplate.</returns>
        public static IGraphQLResolvedFieldTemplate MapBatchTypeExtension<TType>(this SchemaOptions schemaOptions, string fieldName, Delegate resolverMethod = null)
        {
            return schemaOptions.MapBatchTypeExtension(
                typeof(TType),
                fieldName,
                resolverMethod);
        }

        /// <summary>
        /// Registers a new batched type extension to a given type for the target schema.
        /// </summary>
        /// <remarks>
        /// The supplied resolver must declare a parameter that implements an <see cref="IEnumerable{T}"/>
        /// for the supplied <paramref name="typeToExtend"/>.
        /// </remarks>
        /// <param name="schemaOptions">The configuration options for the target schema.</param>
        /// <param name="typeToExtend">The concrete interface, class or struct to extend with a new field.</param>
        /// <param name="fieldName">Name of the field to add to the <paramref name="typeToExtend"/>.</param>
        /// <param name="resolverMethod">The resolver method to be called when the field is requested.</param>
        /// <returns>IGraphQLResolvedFieldTemplate.</returns>
        public static IGraphQLResolvedFieldTemplate MapBatchTypeExtension(this SchemaOptions schemaOptions, Type typeToExtend, string fieldName, Delegate resolverMethod = null)
        {
            var field = MapTypeExtension(
                schemaOptions,
                typeToExtend,
                fieldName,
                FieldResolutionMode.Batch);

            if (resolverMethod != null)
                field = field.AddResolver(resolverMethod);

            return field;
        }

        /// <summary>
        /// Registers a new type extension to a given type for the target schema.
        /// </summary>
        /// <remarks>
        /// The supplied resolver must declare a parameter that is of the same type as <typeparamref name="TType"/>.
        /// </remarks>
        /// <typeparam name="TType">The concrete interface, class or struct to extend with a new field.</typeparam>
        /// <param name="schemaOptions">The configuration options for the target schema.</param>
        /// <param name="fieldName">Name of the field to add to the <typeparamref name="TType"/>.</param>
        /// <param name="resolverMethod">The resolver method to be called when the field is requested.</param>
        /// <returns>IGraphQLResolvedFieldTemplate.</returns>
        public static IGraphQLResolvedFieldTemplate MapTypeExtension<TType>(this SchemaOptions schemaOptions, string fieldName, Delegate resolverMethod = null)
        {
            return schemaOptions.MapTypeExtension(
                typeof(TType),
                fieldName,
                resolverMethod);
        }

        /// <summary>
        /// Registers a new type extension to a given type for the target schema.
        /// </summary>
        /// <remarks>
        /// The supplied resolver must declare a parameter that is of the same type as
        /// <paramref name="typeToExtend"/>.
        /// </remarks>
        /// <param name="schemaOptions">The configuration options for the target schema.</param>
        /// <param name="typeToExtend">The concrete interface, class or struct to extend with a new field.</param>
        /// <param name="fieldName">Name of the field to add to the <paramref name="typeToExtend"/>.</param>
        /// <param name="resolverMethod">The resolver method to be called when the field is requested.</param>
        /// <returns>IGraphQLResolvedFieldTemplate.</returns>
        public static IGraphQLResolvedFieldTemplate MapTypeExtension(this SchemaOptions schemaOptions, Type typeToExtend, string fieldName, Delegate resolverMethod = null)
        {
            var field = MapTypeExtension(
                schemaOptions,
                typeToExtend,
                fieldName,
                FieldResolutionMode.PerSourceItem);

            if (resolverMethod != null)
                field = field.AddResolver(resolverMethod);

            return field;
        }
    }
}