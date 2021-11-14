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
    using System.Collections.Immutable;
    using GraphQL.AspNet.Interfaces.Execution;

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

        /// <summary>
        /// Gets or sets the resolver that can determine resolve the type of a given value to one contained in this union.
        /// </summary>
        /// <value>The type resolver.</value>
        IUnionValueTypeResolver TypeResolver { get; set; }
    }
}