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
    using System.Collections.Generic;

    /// <summary>
    /// A representation of a type in a graph that implements a set
    /// of unique interfaces, organized by their name.
    /// </summary>
    public interface IInterfaceContainer
    {
        /// <summary>
        /// Gets the internal collection of interfaces this object graph type implements.
        /// </summary>
        /// <value>The interfaces.</value>
        HashSet<string> InterfaceNames { get; }
    }
}