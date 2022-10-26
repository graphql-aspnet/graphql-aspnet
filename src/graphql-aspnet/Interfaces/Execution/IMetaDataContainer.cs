// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution
{
    using GraphQL.AspNet.Execution;

    /// <summary>
    /// An interface defining an object as containing a collection of metadata items.
    /// </summary>
    public interface IMetaDataContainer
    {
        /// <summary>
        /// Gets a collection of items used to store and retrieve various pieces of data.
        /// </summary>
        /// <value>The collection items.</value>
        MetaDataCollection Items { get; }
    }
}