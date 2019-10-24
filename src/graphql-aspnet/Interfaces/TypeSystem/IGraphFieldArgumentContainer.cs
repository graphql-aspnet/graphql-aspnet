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
    /// <summary>
    /// An interface that identifies that the entity defines and contains a
    /// collection of input items.
    /// </summary>
    public interface IGraphFieldArgumentContainer : INamedItem
    {
        /// <summary>
        /// Gets a collection of arguments this instance can accept on a query.
        /// </summary>
        /// <value>A collection of arguments assigned to this item.</value>
        IGraphFieldArgumentCollection Arguments { get; }
    }
}