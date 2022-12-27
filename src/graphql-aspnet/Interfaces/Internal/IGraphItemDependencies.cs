// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Internal
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Internal.TypeTemplates;

    /// <summary>
    /// An interface representing the collection of dependencies rendered during the creation of an item related to the object graph.
    /// </summary>
    public interface IGraphItemDependencies
    {
        /// <summary>
        /// Gets a collection of concrete types (and their associated type kinds) that this graph type is dependent on if/when a resolution against a field
        /// of this type occurs.
        /// </summary>
        /// <value>The collected set of dependent types.</value>
        IEnumerable<DependentType> DependentTypes { get; }

        /// <summary>
        /// Gets A collection of pre-created, abstract graph types (Unions and Interfaces) that this graph type is dependent on if/when a resolution
        /// against this type occurs.
        /// </summary>
        /// <value>The collected set of dependent abstract graph types.</value>
        IEnumerable<IGraphType> AbstractGraphTypes { get; }
    }
}