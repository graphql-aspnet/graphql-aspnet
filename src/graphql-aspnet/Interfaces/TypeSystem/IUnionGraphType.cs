// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// A declaration of a union in the graph schema.
    /// </summary>
    public interface IUnionGraphType : IGraphType
    {
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
    }
}