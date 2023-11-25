// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Schema
{
    using System;
    using System.Collections.Immutable;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A declaration of a union in the graph schema.
    /// </summary>
    public interface IUnionGraphType : IGraphType
    {
        /// <summary>
        /// Creates a shallow clone of this instance, replacing the type name with the
        /// provided value if provided.
        /// </summary>
        /// <param name="typeName">When provided, represents the new graph type name to use for the cloned value.</param>
        /// <param name="possibleGraphTypeNameFormatter">A format function that, if supplied, will update all the
        /// possible graph type names on the clone to match the provided format.</param>
        /// <returns>IGraphType.</returns>
        IGraphType Clone(string typeName = null, Func<string, string> possibleGraphTypeNameFormatter = null);

        /// <summary>
        /// Gets the complete list of possible concrete types contained in this union.
        /// </summary>
        /// <value>The possible graph type names.</value>
        IImmutableSet<Type> PossibleConcreteTypes { get; }

        /// <summary>
        /// Gets the complete list possible graph type names contained in this union.
        /// </summary>
        /// <value>The possible graph type names.</value>
        IImmutableSet<string> PossibleGraphTypeNames { get; }

        /// <summary>
        /// Gets or sets the mapper that can determine which unioned type a value from a resolved field
        /// should be interpreted as.
        /// </summary>
        /// <value>The type mapper.</value>
        IUnionGraphTypeMapper TypeMapper { get; set; }
    }
}