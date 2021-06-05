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

    /// <summary>
    /// A declaration of a union in the graph schema.
    /// </summary>
    public interface IUnionGraphType : IGraphType
    {
        /// <summary>
        /// Gets the possible concrete types this union could be.
        /// </summary>
        /// <value>The possible graph type names.</value>
        IEnumerable<Type> PossibleConcreteTypes { get; }

        /// <summary>
        /// Gets the possible graph type names this union could be.
        /// </summary>
        /// <value>The possible graph type names.</value>
        IEnumerable<string> PossibleGraphTypeNames { get; }

        /// <summary>
        /// Gets the proxy object that was defined at design type which created this union type.
        /// </summary>
        /// <value>The proxy.</value>
        IGraphUnionProxy Proxy { get; }
    }
}