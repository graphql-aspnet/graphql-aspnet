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
    /// A representation of an object that can have a collection of unique interfaces, organized by their name.
    /// </summary>
    public interface IGraphInterfaceContainer
    {
        /// <summary>
        /// Gets the internal collection of interfaces this object graph type implements.
        /// </summary>
        /// <value>The interfaces.</value>
        HashSet<string> InterfaceNames { get; }
    }
}