// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution
{
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A request to a invoke some action to perform some meaningful work that produces a result.
    /// </summary>
    public interface IInvocationRequest : IDataRequest
    {
        /// <summary>
        /// Gets the source data item feeding this request.
        /// </summary>
        /// <value>The source data.</value>
        GraphFieldDataSource DataSource { get; }
    }
}