﻿// *************************************************************
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
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Response;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// An entity that can take a <see cref="IQueryExecutionResult" /> and generate a graphql compliant
    /// and formatted response to send to the requestor.
    /// </summary>
    public interface IQueryResponseWriter
    {
        /// <summary>
        /// Attempts to write the provided <see cref="IQueryExecutionResult" /> to the stream. Generally this stream
        /// will be the response stream for an HTTP request.
        /// </summary>
        /// <param name="streamToWriteTo">The stream to write to.</param>
        /// <param name="resultToWrite">The result to write.</param>
        /// <param name="options">A set options to customize how the response is serialized to the stream.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        Task WriteAsync(Stream streamToWriteTo, IQueryExecutionResult resultToWrite, ResponseWriterOptions options = null, CancellationToken cancelToken = default);

        /// <summary>
        /// Attempts to serialize the provided <see cref="IQueryExecutionResult" /> directly to the
        /// provided json writer.
        /// </summary>
        /// <param name="jsonWriter">The json writer.</param>
        /// <param name="resultToWrite">The result to write.</param>
        /// <param name="options">A set options to customize how the response is serialized to the stream.</param>
        void Write(Utf8JsonWriter jsonWriter, IQueryExecutionResult resultToWrite, ResponseWriterOptions options = null);
    }
}