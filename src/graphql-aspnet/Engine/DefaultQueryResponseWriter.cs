﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Response;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.Response;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Web;

    /// <summary>
    /// A standard implementation that writes the results of an executed document
    /// to a stream.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this writer works for.</typeparam>
    public class DefaultQueryResponseWriter<TSchema> : BaseResponseWriter, IGraphQueryResponseWriter<TSchema>
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
        public virtual async Task WriteAsync(Stream streamToWriteTo, IGraphOperationResult resultToWrite, ResponseOptions options = null, CancellationToken cancelToken = default)
        {
            options = options ?? ResponseOptions.Default;

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
        public virtual void Write(Utf8JsonWriter jsonWriter, IGraphOperationResult resultToWrite, ResponseOptions options = null)
        {
            this.WriteResult(jsonWriter, resultToWrite, options);
        }

        /// <summary>
        /// Converts the operation reslt into a dictionary map of the required fields for a graphql response.
        /// Spec: https://graphql.github.io/graphql-spec/October2021/#sec-Response .
        /// </summary>
        /// <param name="writer">The json writer to output the reslts to.</param>
        /// <param name="resultToWrite">The operation result to write.</param>
        /// <param name="options">A set options to customize how the response is serialized to the stream.</param>
        protected virtual void WriteResult(Utf8JsonWriter writer, IGraphOperationResult resultToWrite, ResponseOptions options)
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

        /// <summary>
        /// Writes a single response item to the supplied writer.
        /// </summary>
        /// <param name="writer">The writer to stream to.</param>
        /// <param name="dataItem">The data item to serialize.</param>
        protected virtual void WriteResponseItem(Utf8JsonWriter writer, IResponseItem dataItem)
        {
            if (dataItem == null)
            {
                this.WriteLeafValue(writer, dataItem);
                return;
            }

            switch (dataItem)
            {
                case IResponseFieldSet fieldSet:
                    this.WriteObjectCollection(writer, fieldSet);
                    break;
                case IResponseList list:
                    this.WriteList(writer, list);
                    break;
                case IResponseSingleValue singleValue:
                    this.WriteLeafValue(writer, singleValue.Value);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        $"Unknown {nameof(IResponseItem)} type. " +
                        $"Default writer is unable to write type '{dataItem.GetType().FriendlyName()}' to the output stream.");
            }
        }

        /// <summary>
        /// Walks the the object collection and writes it to the provided writer.
        /// </summary>
        /// <param name="writer">The json writer to output the reslts to.</param>
        /// <param name="data">The dictionary to output to the writer.</param>
        private void WriteObjectCollection(Utf8JsonWriter writer, IResponseFieldSet data)
        {
            if (data == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStartObject();
                foreach (var kvp in data.Fields)
                {
                    writer.WritePropertyName(kvp.Key);
                    this.WriteResponseItem(writer, kvp.Value);
                }

                writer.WriteEndObject();
            }
        }

        /// <summary>
        /// Writes the list of values as an array into the response stream.
        /// </summary>
        /// <param name="writer">The writer to stream to.</param>
        /// <param name="list">The list to write.</param>
        private void WriteList(Utf8JsonWriter writer, IResponseList list)
        {
            if (list?.Items == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStartArray();
                foreach (var item in list.Items)
                {
                    this.WriteResponseItem(writer, item);
                }

                writer.WriteEndArray();
            }
        }
    }
}