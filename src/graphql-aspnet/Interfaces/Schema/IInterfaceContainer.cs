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
    using System.Collections.Generic;

    /// <summary>
    /// A representation of a type in a graph that implements a set
    /// of unique interfaces, organized by their name.
    /// </summary>
    public interface IInterfaceContainer : IGraphType
    {
        /// <summary>
        /// Gets a set of names of interfaces this item may implement if
        /// they are part of the schema. All names should be stored in their schema-specific
        /// naming format.
        /// </summary>
        /// <value>The interfaces this item may implement.</value>
        HashSet<string> InterfaceNames { get; }
    }
}