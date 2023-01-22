// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine
{
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Response;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A standard implementation that writes the results of an executed document
    /// to a stream.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this writer works for.</typeparam>
    public class DefaultQueryResponseWriter<TSchema> : ResponseWriterBase, IQueryResponseWriter<TSchema>
         where TSchema : class, ISchema
    {
        private readonly GraphMessageSeverity _minSeverityLevel;
        private readonly JsonWriterOptions _writerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultQueryResponseWriter{TSchema}" /> class.
        /// </summary>
        /// <param name="schema">The schema from which repsonse settings should be drawn.</param>
        public DefaultQueryResponseWriter(TSchema schema)
            : base(schema)
        {
            Validation.ThrowIfNull(schema, nameof(schema));
            _minSeverityLevel = schema.Configuration.ResponseOptions.MessageSeverityLevel;
            _writerOptions = new JsonWriterOptions()
            {
                Indented = schema.Configuration.ResponseOptions.IndentDocument,
            };
        }

        /// <inheritdoc />
        public virtual async Task WriteAsync(Stream streamToWriteTo, IQueryExecutionResult resultToWrite, ResponseWriterOptions options = null, CancellationToken cancelToken = default)
        {
            options = options ?? ResponseWriterOptions.Default;

            Utf8JsonWriter writer = null;
            try
            {
                writer = new Utf8JsonWriter(streamToWriteTo, _writerOptions);
                this.Write(writer, resultToWrite, options);
            }
            finally
            {
                if (writer != null)
                {
                    await writer.FlushAsync().ConfigureAwait(false);
                    await writer.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        /// <inheritdoc />
        public virtual void Write(Utf8JsonWriter jsonWriter, IQueryExecutionResult resultToWrite, ResponseWriterOptions options = null)
        {
            this.WriteResult(jsonWriter, resultToWrite, options);
        }

        /// <summary>
        /// Converts the operation reslt into a dictionary map of the required fields for a graphql response.
        /// Spec: <see href="https://graphql.github.io/graphql-spec/October2021/#sec-Response" /> .
        /// </summary>
        /// <param name="writer">The writer to stream to.</param>
        /// <param name="resultToWrite">The operation result to write.</param>
        /// <param name="options">A set options to customize how the response is serialized to the stream.</param>
        protected virtual void WriteResult(Utf8JsonWriter writer, IQueryExecutionResult resultToWrite, ResponseWriterOptions options)
        {
            writer.WriteStartObject();

            if (resultToWrite.Messages.Severity >= _minSeverityLevel)
            {
                writer.WriteStartArray("errors");
                foreach (var message in resultToWrite.Messages.Where(x => x.Severity >= _minSeverityLevel))
                    this.WriteMessage(writer, message, options);

                writer.WriteEndArray();
            }

            if (resultToWrite.Data != null || resultToWrite.Messages.Count == 0)
            {
                writer.WritePropertyName("data");
                this.WriteObjectCollection(writer, resultToWrite.Data);
            }

            if (options.ExposeMetrics && resultToWrite.Metrics != null)
            {
                var result = resultToWrite.Metrics.GenerateResult();
                writer.WritePropertyName("extensions");
                this.WriteObjectCollection(writer, result);
            }

            writer.WriteEndObject(); // {document}
        }
    }
}