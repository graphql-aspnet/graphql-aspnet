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
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
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
    }
}