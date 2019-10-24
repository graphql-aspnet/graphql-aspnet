// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Engine
{
    using System;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An interface describing the templating classes used by a schema, at runtime, to parse C# types and build valid graph types.
    /// This object is used as a singleton instance for this graphql server and any object implementing this interface
    /// should be designed in a thread-safe, singleton fashion. Only one instance of this provider exists for the application instance.
    /// </summary>
    public interface IGraphTypeTemplateProvider
    {
        /// <summary>
        /// Removes all cached templates.
        /// </summary>
        void Clear();

        /// <summary>
        /// Parses the provided type, extracting the metadata to used in type generation for the object graph.
        /// </summary>
        /// <typeparam name="TObjectType">The type of the object to parse.</typeparam>
        /// <param name="kind">The graph <see cref="TypeKind"/> to create a template for. If not supplied the template provider
        /// will attempt to assign the best graph type possible.</param>
        /// <returns>IGraphItemTemplate.</returns>
        IGraphItemTemplate ParseType<TObjectType>(TypeKind? kind = null);

        /// <summary>
        /// Parses the provided type, extracting the metadata to used in type generation for the object graph.
        /// </summary>
        /// <param name="objectType">The type of the object to parse.</param>
        /// <param name="kind">The graph <see cref="TypeKind" /> to create a template for. If not supplied the template provider
        /// will attempt to assign the best graph type possible.</param>
        /// <returns>IGraphTypeTemplate.</returns>
        IGraphTypeTemplate ParseType(Type objectType, TypeKind? kind = null);

        /// <summary>
        /// Gets the count of registered <see cref="IGraphTypeTemplate"/> objects.
        /// </summary>
        /// <value>The count.</value>
        int Count { get; }

        /// <summary>
        /// Gets or sets a value indicating whether templates, once parsed, are retained.
        /// </summary>
        /// <value><c>true</c> if templates are cached after the first creation; otherwise, <c>false</c>.</value>
        bool CacheTemplates { get; set; }
    }
}