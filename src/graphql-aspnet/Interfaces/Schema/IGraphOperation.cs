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
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An interface representing a top level object graph type within a schema
    /// that is typed for one of the allowed operation types of graphql.
    /// </summary>
    public interface IGraphOperation : IObjectGraphType
    {
        /// <summary>
        /// Gets the enum value representing the graph operation.
        /// </summary>
        /// <value>The root type this operation represents.</value>
        GraphOperationType OperationType { get; }
    }
}