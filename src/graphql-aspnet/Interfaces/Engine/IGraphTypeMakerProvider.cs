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
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A abstract factory interface for creating type generator for the various graph type creation operations.
    /// </summary>
    public interface IGraphTypeMakerProvider
    {
        /// <summary>
        /// Creates an appropriate graph type maker for the given concrete type.
        /// </summary>
        /// <param name="schema">The schema for which the maker should generate graph types for.</param>
        /// <param name="kind">The kind of graph type to create. If null, the factory will attempt to deteremine the
        /// most correct maker to use.</param>
        /// <returns>IGraphTypeMaker.</returns>
        IGraphTypeMaker CreateTypeMaker(ISchema schema, TypeKind kind);

        /// <summary>
        /// Creates a "maker" that can generate graph fields.
        /// </summary>
        /// <param name="schema">The schema to which the created fields should belong.</param>
        /// <returns>IGraphFieldMaker.</returns>
        IGraphFieldMaker CreateFieldMaker(ISchema schema);

        /// <summary>
        /// Creates a "maker" that can generate unions for the target schema.
        /// </summary>
        /// <param name="schema">The schema to generate unions for.</param>
        /// <returns>IUnionGraphTypeMaker.</returns>
        IUnionGraphTypeMaker CreateUnionMaker(ISchema schema);

        /// <summary>
        /// Attempts to create a union proxy from the given proxy type definition.
        /// </summary>
        /// <param name="proxyType">The type definition of the union proxy to create.</param>
        /// <returns>IGraphUnionProxy.</returns>
        IGraphUnionProxy CreateUnionProxyFromType(Type proxyType);
    }
}