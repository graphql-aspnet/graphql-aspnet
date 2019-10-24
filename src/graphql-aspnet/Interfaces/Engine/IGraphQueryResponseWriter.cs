// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Engine
{
    using System.IO;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Response;

    /// <summary>
    /// An entity that can take a <see cref="IGraphOperationResult" /> and generate a graphql compliant
    /// and formatted response to send to the requestor.
    /// </summary>
    public interface IGraphQueryResponseWriter
    {
        /// <summary>
        /// Attempts to write the provided <see cref="IGraphOperationResult" /> to the stream. Generally this stream
        /// will be the response stream for an HTTP request.
        /// </summary>
        /// <param name="streamToWriteTo">The stream to write to.</param>
        /// <param name="resultToWrite">The result to write.</param>
        /// <param name="options">A set options to customize how the response is serialized to the stream.</param>
        /// <returns>Task.</returns>
        Task WriteAsync(Stream streamToWriteTo, IGraphOperationResult resultToWrite, GraphQLResponseOptions options = null);
    }
}