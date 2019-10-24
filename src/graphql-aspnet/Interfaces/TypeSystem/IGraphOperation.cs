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
    using GraphQL.AspNet.Execution;

    /// <summary>
    /// An interface representing a top level object graph type with in a schema
    /// that is typed for one of the allowed operation types of graphql.
    /// </summary>
    public interface IGraphOperation : IObjectGraphType
    {
        /// <summary>
        /// Gets the graph operation represented by this instance.
        /// </summary>
        /// <value>The type of the root.</value>
        GraphCollection OperationType { get; }
    }
}